/*
 * Copyright (c) 2010-2026 GraphDefined GmbH <achim.friedland@graphdefined.com>
 * This file is part of XMPPConsole <https://www.github.com/Vanaheimr/XMPPConsole>
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#region Usings

using System.Net;
using System.Net.Sockets;

using org.GraphDefined.Vanaheimr.Ratatoskr;

#endregion

namespace org.GraphDefined.Vanaheimr.XMPPConsole.ChatLogs
{

    /// <summary>
    /// Fetches the files shared in a conversation - and refuses to fetch most
    /// other things.
    /// </summary>
    /// <remarks>
    /// <b>This turns a message into a network request, so the sender decides
    /// what this machine fetches.</b> Everybody who may write to us can put a
    /// URL in front of us, and only what is in the rules below stops that URL
    /// from being an address inside the network this console runs in. Hence:
    /// https only, no address that belongs to this machine or to a private
    /// network, redirects followed by hand and checked at every hop, an upper
    /// size, and a timeout.
    ///
    /// One gap is named rather than papered over: the host is resolved, checked
    /// and then handed to the HTTP client, which resolves it a second time. A
    /// name that answers differently on the second query gets through
    /// (DNS rebinding). Closing it means connecting to the address that was
    /// checked and carrying the name only in SNI and the Host header, which is
    /// a socket handler of one's own. What stands here raises the cost; it does
    /// not make it impossible.
    /// </remarks>
    public sealed class MediaStore
    {

        #region Data

        /// <summary>
        /// The most that is fetched for a single file. A shared photo is a few
        /// megabytes; this leaves room for a video and still says no to
        /// somebody pointing us at an installation image.
        /// </summary>
        public const Int64 MaxBytes = 100L * 1024 * 1024;

        /// <summary>How long one file may take altogether.</summary>
        public static readonly TimeSpan Timeout = TimeSpan.FromMinutes(5);

        /// <summary>
        /// How often a redirect is followed. Every hop is checked like the
        /// first address.
        /// </summary>
        public const Int32 MaxRedirects = 5;

        private readonly String      root;
        private readonly HttpClient  httpClient;

        #endregion

        #region Constructor(s)

        /// <summary>
        /// Creates a store below the chat log directory.
        /// </summary>
        public MediaStore(String Root)
        {

            root = Path.GetFullPath(Root);

            // Redirects by hand: AllowAutoRedirect would follow a 302 into the
            // private network the first address was refused for.
            httpClient = new HttpClient(new HttpClientHandler {
                             AllowAutoRedirect = false
                         }) {
                             Timeout = Timeout
                         };

        }

        #endregion


        #region FetchAsync(PeerJID, URL, ReceivedAt, CancellationToken)

        /// <summary>
        /// Fetches one file and puts it beside the conversation it belongs to.
        /// </summary>
        /// <param name="ReceivedAt">
        /// When the download happened. It goes into the file name, because it
        /// is the one time this side actually knows: the timestamp inside a
        /// message is what the sender claims.
        /// </param>
        /// <returns>
        /// What to write into the chat log - the relative path on success, the
        /// reason on refusal. Never throws for an expected failure; a file that
        /// cannot be fetched is an ordinary outcome of talking to strangers.
        /// </returns>
        public async Task<String> FetchAsync(String             PeerJID,
                                             Uri                URL,
                                             DateTime           ReceivedAt,
                                             CancellationToken  CancellationToken = default)
        {

            Byte[]? key    = null;
            Byte[]? nonce  = null;

            var isEncrypted = AesGcmUrl.IsAesGcmUrl(URL);
            var address     = URL;

            if (isEncrypted)
            {

                if (!AesGcmUrl.TryParse(URL, out key, out nonce, out var problem))
                    return $"not fetched: {problem}";

                address = AesGcmUrl.ToHttps(URL);

            }

            try
            {

                var payload = await DownloadAsync(address, CancellationToken);

                if (payload.Problem is not null)
                    return $"not fetched: {payload.Problem}";

                var content = payload.Content!;

                if (isEncrypted)
                {
                    try
                    {
                        content = AesGcmUrl.Decrypt(content, key!, nonce!);
                    }
                    catch (Exception e)
                    {
                        // A failing tag is not a broken download but a file
                        // that is not what the sender's key says it is. It is
                        // not stored: a file that fails its own check has no
                        // business in an archive.
                        return $"not stored: decryption failed ({e.Message})";
                    }
                }

                var directory = ChatLogPaths.MediaDirectoryFor(root, PeerJID);
                Directory.CreateDirectory(directory);

                var name = ChatLogPaths.MediaFileName(ReceivedAt, SuggestedNameOf(address));
                var path = Path.Combine(directory, name);

                await File.WriteAllBytesAsync(path, content, CancellationToken);

                return $"stored: media/{name}  ({content.Length} bytes" +
                       (isEncrypted ? ", decrypted)" : ")");

            }
            catch (OperationCanceledException)
            {
                return "not fetched: cancelled";
            }
            catch (Exception e)
            {
                return $"not fetched: {e.Message}";
            }

        }

        #endregion

        #region (private) DownloadAsync(Address, CancellationToken)

        private async Task<(Byte[]? Content, String? Problem)> DownloadAsync(Uri                Address,
                                                                             CancellationToken  CancellationToken)
        {

            var address = Address;

            for (var hop = 0; hop <= MaxRedirects; hop++)
            {

                if (!address.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase))
                    return (null, $"{address.Scheme} is not fetched, https only");

                var refusal = await AddressIsRefusedAsync(address, CancellationToken);
                if (refusal is not null)
                    return (null, refusal);

                using var request = new HttpRequestMessage(HttpMethod.Get, address);

                var response = await httpClient.SendAsync(request,
                                                          HttpCompletionOption.ResponseHeadersRead,
                                                          CancellationToken);

                using (response)
                {

                    if (response.StatusCode is HttpStatusCode.Moved
                                            or HttpStatusCode.Found
                                            or HttpStatusCode.SeeOther
                                            or HttpStatusCode.TemporaryRedirect
                                            or HttpStatusCode.PermanentRedirect)
                    {

                        var location = response.Headers.Location;

                        if (location is null)
                            return (null, $"redirect {(Int32) response.StatusCode} without a target");

                        address = location.IsAbsoluteUri
                                      ? location
                                      : new Uri(address, location);

                        continue;

                    }

                    if (!response.IsSuccessStatusCode)
                        return (null, $"HTTP {(Int32) response.StatusCode} {response.ReasonPhrase}");

                    // The announced length is a claim and is checked first only
                    // to save the transfer; the real limit is counted while
                    // reading, because the claim may be missing or wrong.
                    if (response.Content.Headers.ContentLength > MaxBytes)
                        return (null, $"{response.Content.Headers.ContentLength} bytes announced, " +
                                      $"more than the {MaxBytes} allowed");

                    return await ReadAtMostAsync(response, CancellationToken);

                }

            }

            return (null, $"more than {MaxRedirects} redirects");

        }

        #endregion

        #region (private) ReadAtMostAsync(Response, CancellationToken)

        private static async Task<(Byte[]? Content, String? Problem)> ReadAtMostAsync(HttpResponseMessage  Response,
                                                                                      CancellationToken    CancellationToken)
        {

            using var stream  = await Response.Content.ReadAsStreamAsync(CancellationToken);
            using var buffer  = new MemoryStream();

            var chunk  = new Byte[81920];
            var total  = 0L;

            while (true)
            {

                var read = await stream.ReadAsync(chunk, CancellationToken);

                if (read == 0)
                    break;

                total += read;

                if (total > MaxBytes)
                    return (null, $"larger than the {MaxBytes} bytes allowed");

                buffer.Write(chunk, 0, read);

            }

            return (buffer.ToArray(), null);

        }

        #endregion

        #region (private) AddressIsRefusedAsync(Address, CancellationToken)

        /// <summary>
        /// Why this address is not fetched - or null when it may be.
        /// </summary>
        private static async Task<String?> AddressIsRefusedAsync(Uri                Address,
                                                                 CancellationToken  CancellationToken)
        {

            IPAddress[] addresses;

            if (IPAddress.TryParse(Address.Host.Trim('[', ']'), out var literal))
                addresses = [literal];

            else
            {
                try
                {
                    addresses = await Dns.GetHostAddressesAsync(Address.Host, CancellationToken);
                }
                catch (Exception e)
                {
                    return $"{Address.Host} did not resolve ({e.Message})";
                }
            }

            if (addresses.Length == 0)
                return $"{Address.Host} did not resolve";

            // Every address, not the first: a name pointing at one public and
            // one internal address must not get through because the public one
            // was looked at first.
            foreach (var address in addresses)
                if (IsLocalOrPrivate(address))
                    return $"{Address.Host} resolves to {address}, which is not a public address";

            return null;

        }

        #endregion

        #region (private) IsLocalOrPrivate(Address)

        private static Boolean IsLocalOrPrivate(IPAddress Address)
        {

            if (IPAddress.IsLoopback(Address))
                return true;

            if (Address.AddressFamily == AddressFamily.InterNetworkV6)
            {

                if (Address.IsIPv6LinkLocal || Address.IsIPv6SiteLocal || Address.IsIPv6Multicast)
                    return true;

                // Unique local addresses, fc00::/7.
                var v6 = Address.GetAddressBytes();
                if ((v6[0] & 0xFE) == 0xFC)
                    return true;

                // An IPv4 address in v6 clothing is still that address.
                if (Address.IsIPv4MappedToIPv6)
                    return IsLocalOrPrivate(Address.MapToIPv4());

                return Address.Equals(IPAddress.IPv6Any);

            }

            var v4 = Address.GetAddressBytes();

            return v4[0] switch {
                       0    => true,                                   // this network
                       10   => true,                                   // RFC 1918
                       127  => true,                                   // loopback
                       169  => v4[1] == 254,                           // link local
                       172  => v4[1] >= 16 && v4[1] <= 31,             // RFC 1918
                       192  => (v4[1] == 168) ||                       // RFC 1918
                               (v4[1] == 0 && v4[2] == 0),             // IETF protocol assignments
                       198  => v4[1] == 18 || v4[1] == 19,             // benchmarking
                       _    => v4[0] >= 224                            // multicast and reserved
                   };

        }

        #endregion

        #region (private) SuggestedNameOf(Address)

        /// <summary>
        /// What the URL calls the file. A suggestion and nothing more - it is a
        /// stranger's text and goes through the same sanitising as a JID.
        /// </summary>
        private static String? SuggestedNameOf(Uri Address)
        {

            var last = Address.AbsolutePath.TrimEnd('/');
            var slash = last.LastIndexOf('/');

            if (slash >= 0 && slash < last.Length - 1)
                last = last[(slash + 1)..];

            last = Uri.UnescapeDataString(last);

            return last.Length == 0 ? null : last;

        }

        #endregion

    }

}
