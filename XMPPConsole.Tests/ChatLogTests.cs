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
    /// Where a conversation is written and what a line of it looks like.
    /// </summary>
    /// <remarks>
    /// What is checked here is the half a stranger can steer. A JID is chosen
    /// by whoever owns it, not by us, and it becomes a directory name - so the
    /// interesting cases are not "does it write a file" but "what happens when
    /// the resource is called <c>..\..\Windows</c>".
    /// </remarks>
    [TestFixture]
    public class ChatLogTests
    {

        #region Data

        private String root = null!;

        #endregion

        #region SetUp / TearDown

        [SetUp]
        public void Setup()
        {
            root = Path.Combine(Path.GetTempPath(),
                                "XMPPConsoleChatLogTests",
                                Guid.NewGuid().ToString("N"));
        }

        [TearDown]
        public void CleanUp()
        {
            try
            {
                if (Directory.Exists(root))
                    Directory.Delete(root, true);
            }
            catch
            { }
        }

        #endregion


        #region ATraversalInTheJid_StaysInsideTheRoot()

        /// <summary>
        /// A JID is not a path, and the parts of it that are not a path are
        /// exactly the parts a stranger picks.
        /// </summary>
        /// <remarks>
        /// The localpart forbids '/' and ':' (RFC 7622, section 3.3), the
        /// resource forbids next to nothing. <c>eve@example.org/..\..\x</c> is
        /// a JID anybody may carry, and without the sanitising it would be a
        /// way of writing into a directory this program never chose.
        /// </remarks>
        [Test]
        public void ATraversalInTheJid_StaysInsideTheRoot()
        {

            var evil = @"eve@example.org/..\..\..\Windows";
            var name = ChatLogPaths.SafeName(evil);
            var path = Path.GetFullPath(ChatLogPaths.LogFileFor(root, evil, new DateTime(2026, 8, 15)));

            Assert.Multiple(() =>
            {

                Assert.That(path.StartsWith(Path.GetFullPath(root), StringComparison.Ordinal),
                            Is.True,
                            $"The log left the given directory: {path}");

                // What makes it harmless is that it is one segment. The dots
                // survive - '.' is a permitted character and a file may be
                // called 'a..b' - but with the separators gone they are no
                // longer a step upwards, they are letters in a name.
                Assert.That(Path.GetFileName(name), Is.EqualTo(name),
                            $"The JID became more than one path segment: {name}");

                Assert.That(name, Does.Not.Contain(@"\"));
                Assert.That(name, Does.Not.Contain("/"));

            });

        }

        #endregion

        #region AJidOfNothingButDots_IsStillAName()

        /// <summary>
        /// '.' and '..' consist of permitted characters and are nevertheless
        /// not names. They are the case the character filter alone lets
        /// through.
        /// </summary>
        [Test]
        public void AJidOfNothingButDots_IsStillAName()
        {

            Assert.Multiple(() =>
            {
                Assert.That(ChatLogPaths.SafeName(".."),  Is.EqualTo("_"));
                Assert.That(ChatLogPaths.SafeName("."),   Is.EqualTo("_"));
                Assert.That(ChatLogPaths.SafeName(""),    Is.EqualTo("_"));
            });

        }

        #endregion

        #region AReservedWindowsName_IsMovedAside()

        /// <summary>
        /// <c>nul@example.org</c> is an ordinary JID; <c>NUL</c> is not an
        /// ordinary file name. Only the whole name matters, and only after it
        /// has been shortened.
        /// </summary>
        [Test]
        public void AReservedWindowsName_IsMovedAside()
        {

            Assert.Multiple(() =>
            {

                Assert.That(ChatLogPaths.SafeName("NUL"),  Is.EqualTo("_NUL"));
                Assert.That(ChatLogPaths.SafeName("com1"), Is.EqualTo("_com1"));

                // The JID with a domain is no longer the reserved name and is
                // left as it is.
                Assert.That(ChatLogPaths.SafeName("nul@example.org"), Is.EqualTo("nul@example.org"));

            });

        }

        #endregion

        #region TheFileIsNamedByMonth()

        [Test]
        public void TheFileIsNamedByMonth()
        {

            var path = ChatLogPaths.LogFileFor(root, "a@b.org", new DateTime(2026, 8, 15, 23, 59, 59));

            Assert.That(Path.GetFileName(path), Is.EqualTo("a@b.org_202608.log"));

        }

        #endregion

        #region MediaIsNamedByTheMomentItArrived()

        /// <summary>
        /// The time of the download and not of the message: the second is what
        /// the sender claims, the first is what happened here.
        /// </summary>
        [Test]
        public void MediaIsNamedByTheMomentItArrived()
        {

            var name = ChatLogPaths.MediaFileName(new DateTime(2026, 8, 15, 13, 42, 7), "holiday photo.jpg");

            Assert.Multiple(() =>
            {
                Assert.That(name, Does.StartWith("20260815_134207_"));
                Assert.That(name, Does.Not.Contain(" "));
                Assert.That(name, Does.EndWith(".jpg"));
            });

        }

        #endregion

        #region AMessageStaysOnOneLine()

        /// <summary>
        /// A message may contain line breaks; a log line may not. Otherwise a
        /// grep shows half a sentence without author or time.
        /// </summary>
        /// <remarks>
        /// The order of the two replacements is what is really checked. Were
        /// the line break escaped first, a message containing the two
        /// characters <c>\n</c> would afterwards be indistinguishable from one
        /// containing an actual line break.
        /// </remarks>
        [Test]
        public void AMessageStaysOnOneLine()
        {

            Assert.Multiple(() =>
            {

                Assert.That(ChatLogWriter.Escape("a\nb"),     Is.EqualTo(@"a\nb"));
                Assert.That(ChatLogWriter.Escape("a\r\nb"),   Is.EqualTo(@"a\nb"));
                Assert.That(ChatLogWriter.Escape(@"a\nb"),    Is.EqualTo(@"a\\nb"),
                            "A literal backslash-n has to stay distinguishable from a line break.");

            });

        }

        #endregion

        #region AConversation_IsWrittenWithAHeaderAndOneLinePerEvent()

        /// <summary>
        /// The whole way through, once: two messages and a presence into a
        /// real file.
        /// </summary>
        [Test]
        public void AConversation_IsWrittenWithAHeaderAndOneLinePerEvent()
        {

            var log   = new ChatLogWriter(root, _ => { });
            var when  = new DateTime(2026, 8, 15, 13, 42, 7);

            log.Message ("ahzf@example.org", when,                ChatLogKind.Incoming, "ahzf@example.org/iPhone", "Hi 4!");
            log.Message ("ahzf@example.org", when.AddSeconds(4),  ChatLogKind.Outgoing, "me",                      "Hallo\nzurück");
            log.Presence("ahzf@example.org", when.AddSeconds(27), "ahzf@example.org/iPhone", "available", "back at four");

            var path  = ChatLogPaths.LogFileFor(root, "ahzf@example.org", when);
            var lines = File.ReadAllLines(path);

            var events = lines.Where(line => !line.StartsWith('#')).ToArray();

            Assert.Multiple(() =>
            {

                Assert.That(lines[0], Does.StartWith("# XMPPConsole chat log"));

                Assert.That(events, Has.Length.EqualTo(3),
                            "Three events, three lines - the message with the line break included.");

                Assert.That(events[0], Is.EqualTo("2026-08-15 13:42:07  <  ahzf@example.org/iPhone  Hi 4!"));
                Assert.That(events[1], Is.EqualTo(@"2026-08-15 13:42:11  >  me  Hallo\nzurück"));
                Assert.That(events[2], Does.Contain("*"));
                Assert.That(events[2], Does.Contain("available"));
                Assert.That(events[2], Does.Contain("back at four"),
                            "The status text is the half somebody reads a presence line for.");

            });

        }

        #endregion

        #region AFailingWrite_IsReportedOnceAndDoesNotThrow()

        /// <summary>
        /// A chat log is a convenience. A full disk must cost the line and not
        /// the session.
        /// </summary>
        /// <remarks>
        /// The failure is provoked with a root that is a file: no directory can
        /// be created below it, on any platform.
        /// </remarks>
        [Test]
        public void AFailingWrite_IsReportedOnceAndDoesNotThrow()
        {

            Directory.CreateDirectory(root);

            var blocking = Path.Combine(root, "blocking");
            File.WriteAllText(blocking, "not a directory");

            var reported = new List<String>();
            var log      = new ChatLogWriter(blocking, reported.Add);

            Assert.DoesNotThrow(() => {
                for (var i = 0; i < 5; i++)
                    log.Message("a@b.org", DateTime.Now, ChatLogKind.Incoming, "a@b.org", $"message {i}");
            });

            Assert.Multiple(() =>
            {

                Assert.That(reported, Has.Count.EqualTo(1),
                            "Reported once - a log that cannot write must not fill the screen with saying so.");

                Assert.That(log.SuppressedProblems, Is.EqualTo(4UL),
                            "The rest are counted, so that the number is not simply lost.");

            });

        }

        #endregion

    }

}
