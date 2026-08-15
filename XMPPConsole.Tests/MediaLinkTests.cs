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

using org.GraphDefined.Vanaheimr.XMPPConsole.ChatLogs;

#endregion

namespace org.GraphDefined.Vanaheimr.XMPPConsole.Tests
{

    /// <summary>
    /// Which link in a message is a shared file - and which is merely a link.
    /// </summary>
    /// <remarks>
    /// This is the decision that turns an incoming message into a network
    /// request from this machine. Everybody who may write here can put a URL in
    /// front of us; the rule separating "handed me a file" from "mentioned a
    /// page" is therefore not a convenience but the whole of the restraint.
    ///
    /// Reading and decrypting an <c>aesgcm://</c> URL is not checked here any
    /// more - that is XEP-0454 and belongs to the protocol, so it lives in
    /// Ratatoskr with its own tests. What stays here is the question this
    /// application answers for itself: whether to fetch at all.
    /// </remarks>
    [TestFixture]
    public class MediaLinkTests
    {

        #region ABodyThatIsNothingButAUrl_IsAHandedOverFile()

        /// <summary>
        /// How a client sends an upload per XEP-0363: the body repeats the URL,
        /// because the receiver may not read the OOB element beside it.
        /// </summary>
        [Test]
        public void ABodyThatIsNothingButAUrl_IsAHandedOverFile()
        {

            var found = MediaLinks.Detect("https://upload.example.org/abc/photo.jpg");

            Assert.Multiple(() =>
            {
                Assert.That(found,                Has.Count.EqualTo(1));
                Assert.That(found[0].AbsoluteUri, Is.EqualTo("https://upload.example.org/abc/photo.jpg"));
            });

        }

        #endregion

        #region AUrlInsideASentence_IsNotFetched()

        /// <summary>
        /// Somebody pointing at something is not somebody handing it over.
        /// </summary>
        /// <remarks>
        /// Without this line every article anybody links would be downloaded,
        /// and whoever may write to us would decide what this machine fetches -
        /// including, with a little thought, an address inside the network it
        /// runs in.
        /// </remarks>
        [Test]
        public void AUrlInsideASentence_IsNotFetched()
        {

            Assert.Multiple(() =>
            {

                Assert.That(MediaLinks.Detect("look at https://example.org/a.jpg"),       Is.Empty);
                Assert.That(MediaLinks.Detect("https://example.org/a.jpg is the one"),    Is.Empty);
                Assert.That(MediaLinks.Detect("https://a.org/1.jpg https://a.org/2.jpg"), Is.Empty,
                            "Two URLs are a sentence about links, not one handed-over file.");

            });

        }

        #endregion

        #region AnAesgcmUrl_IsAlwaysAFile()

        /// <summary>
        /// The scheme exists for nothing else (XEP-0454) - the URL carries the
        /// key to the file. Nobody writes one in passing.
        /// </summary>
        [Test]
        public void AnAesgcmUrl_IsAlwaysAFile()
        {

            var found = MediaLinks.Detect("here you go: aesgcm://up.example.org/x/y.jpg#" + new String('a', 88));

            Assert.Multiple(() =>
            {
                Assert.That(found,           Has.Count.EqualTo(1));
                Assert.That(found[0].Scheme, Is.EqualTo("aesgcm"));
            });

        }

        #endregion

        #region PlainHttp_IsNotFetched()

        /// <summary>
        /// The file would travel readable. A shared file is nothing this side
        /// should make more public than the sender did.
        /// </summary>
        [Test]
        public void PlainHttp_IsNotFetched()
        {
            Assert.That(MediaLinks.Detect("http://example.org/a.jpg"), Is.Empty);
        }

        #endregion

        #region TrailingPunctuation_IsNotPartOfTheAddress()

        /// <summary>
        /// A full stop after a URL ends the sentence, not the address - but a
        /// bracket may well belong to it.
        /// </summary>
        [Test]
        public void TrailingPunctuation_IsNotPartOfTheAddress()
        {

            Assert.Multiple(() =>
            {

                Assert.That(MediaLinks.TrimTrailingPunctuation("https://a.org/b.jpg."),
                            Is.EqualTo("https://a.org/b.jpg"));

                Assert.That(MediaLinks.TrimTrailingPunctuation("https://a.org/b_(c).jpg"),
                            Is.EqualTo("https://a.org/b_(c).jpg"),
                            "A bracket that was opened inside the URL belongs to it.");

                Assert.That(MediaLinks.TrimTrailingPunctuation("https://a.org/b.jpg)"),
                            Is.EqualTo("https://a.org/b.jpg"),
                            "One that was not opened belongs to the sentence.");

            });

        }

        #endregion

    }

}
