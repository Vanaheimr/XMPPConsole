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

using NUnit.Framework;

#endregion

namespace org.GraphDefined.Vanaheimr.XMPPConsole.Tests
{

    /// <summary>
    /// Which endpoint this console signs on to - and which it refuses.
    /// </summary>
    /// <remarks>
    /// The one decision the console makes before any protocol runs. Everything
    /// downstream - the pinning, the mechanism ranking, the whole SASL
    /// apparatus in Ratatoskr - assumes the connection underneath it is
    /// TLS-protected. Where it is not, the first login is taken, and the
    /// pinning that would have noticed pins the theft instead.
    /// </remarks>
    [TestFixture]
    public class EndpointPolicyTests
    {

        #region Wss_IsWhatThisClientIsFor()

        /// <summary>
        /// The ordinary case, and the only one that needs no further word.
        /// </summary>
        [Test]
        public void Wss_IsWhatThisClientIsFor()
        {
            Assert.That(EndpointPolicy.Refuse("wss://xmpp.example.com:5443/ws"), Is.Null);
        }

        #endregion

        #region Ws_IsRefused_AndTheRefusalNamesTheWayOut()

        /// <summary>
        /// The refusal has to carry the option with it. A message that only
        /// says no leaves whoever meant it - a server on their own machine -
        /// with nothing but the guess that the program cannot do it.
        /// </summary>
        [Test]
        public void Ws_IsRefused_AndTheRefusalNamesTheWayOut()
        {

            var objection = EndpointPolicy.Refuse("ws://localhost:5280/xmpp-websocket");

            Assert.Multiple(() =>
            {
                Assert.That(objection, Is.Not.Null);
                Assert.That(objection, Does.Contain("--insecure"));
            });

        }

        #endregion

        #region Ws_WithInsecure_IsLetThrough()

        /// <summary>
        /// Expressly asked for, expressly allowed. The judgement about the way
        /// belongs to whoever knows it.
        /// </summary>
        [Test]
        public void Ws_WithInsecure_IsLetThrough()
        {
            Assert.That(EndpointPolicy.Refuse("ws://localhost:5280/xmpp-websocket", AllowInsecure: true),
                        Is.Null);
        }

        #endregion

        #region Insecure_OpensNothingButWs()

        /// <summary>
        /// <b>The flag is not a switch that turns the checking off.</b> It
        /// permits the one thing it is named for. An http:// endpoint stays
        /// refused with it, because it is not a WebSocket endpoint at all -
        /// that is a different objection, and a flag about encryption has no
        /// business answering it.
        /// </summary>
        [Test]
        public void Insecure_OpensNothingButWs()
        {
            Assert.That(EndpointPolicy.Refuse("http://xmpp.example.com/ws", AllowInsecure: true),
                        Is.Not.Null);
        }

        #endregion

        #region NoEndpointAtAll_IsNoObjection()

        /// <summary>
        /// Naming none is the normal way to start this program: the library
        /// then asks the host-meta of the domain and otherwise stays at
        /// wss://{domain}:5443/ws. Neither road can yield a plain endpoint -
        /// XEP-0156 takes only wss:// out of a host-meta, and the fallback
        /// names it itself.
        /// </summary>
        [Test]
        public void NoEndpointAtAll_IsNoObjection()
        {

            Assert.Multiple(() =>
            {
                Assert.That(EndpointPolicy.Refuse(null),   Is.Null);
                Assert.That(EndpointPolicy.Refuse(""),     Is.Null);
                Assert.That(EndpointPolicy.Refuse("   "),  Is.Null);
            });

        }

        #endregion

        #region Https_IsRefused_WithTheAddressItWasProbablyMeantToBe()

        /// <summary>
        /// The likeliest mistyping of them all, because every other address a
        /// person copies out of a browser begins that way. Answering it with
        /// the corrected address costs one line and saves the ten minutes of
        /// staring at a refusal one agrees with.
        /// </summary>
        [Test]
        public void Https_IsRefused_WithTheAddressItWasProbablyMeantToBe()
        {

            var objection = EndpointPolicy.Refuse("https://xmpp.example.com/ws");

            Assert.Multiple(() =>
            {
                Assert.That(objection, Is.Not.Null);
                Assert.That(objection, Does.Contain("wss://xmpp.example.com/ws"));
            });

        }

        #endregion

        #region AnAddressWithoutAScheme_IsRefused()

        /// <summary>
        /// "xmpp.example.com:5443/ws" parses - as an absolute URI whose scheme
        /// is the host name. So the check must not ask "does this parse" but
        /// "is the scheme one of the two", or a forgotten wss:// would go
        /// through as something nobody can classify.
        /// </summary>
        [Test]
        public void AnAddressWithoutAScheme_IsRefused()
        {

            Assert.Multiple(() =>
            {
                Assert.That(EndpointPolicy.Refuse("xmpp.example.com:5443/ws"), Is.Not.Null);
                Assert.That(EndpointPolicy.Refuse("not an address"),           Is.Not.Null);
            });

        }

        #endregion

        #region TheSchemeIsReadWithoutRegardToCase()

        /// <summary>
        /// URI schemes are case-insensitive (RFC 3986, section 3.1), and
        /// <c>Uri.Scheme</c> hands them back lowercased already - an ordinal
        /// comparison would pass this too. The test is here for the caller's
        /// question, not the implementation's: whoever types "WSS://" wants to
        /// know that the shift key does not decide anything. It also keeps the
        /// day honest on which somebody replaces the Uri parsing with a
        /// StartsWith, where the case does start to decide.
        /// </summary>
        [Test]
        public void TheSchemeIsReadWithoutRegardToCase()
        {

            Assert.Multiple(() =>
            {
                Assert.That(EndpointPolicy.Refuse("WSS://xmpp.example.com/ws"), Is.Null);
                Assert.That(EndpointPolicy.Refuse("WS://xmpp.example.com/ws"),  Is.Not.Null);
            });

        }

        #endregion

    }

}
