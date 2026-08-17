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

using System.Text;

#endregion

namespace org.GraphDefined.Vanaheimr.XMPPConsole.ChatLogs
{

    /// <summary>
    /// What kind of event a line records. The character is what stands in the
    /// file, so it is part of the format and not a detail.
    /// </summary>
    public enum ChatLogKind
    {

        /// <summary>
        /// Something arrived.
        /// </summary>
        Incoming,

        /// <summary>
        /// Something was sent from here.
        /// </summary>
        Outgoing,

        /// <summary>
        /// A presence change of the far end.
        /// </summary>
        Presence,

        /// <summary>
        /// The session itself: connected, lost, a contact request.
        /// </summary>
        System,

        /// <summary>
        /// A file was fetched, or was not.
        /// </summary>
        Media

    }


    /// <summary>
    /// Writes the conversations as text, one line per event, one file per
    /// conversation and month.
    /// </summary>
    /// <remarks>
    /// Text and not the XML that produced it: the raw stream is already
    /// available live through <c>/raw</c>, and it answers a different question.
    /// Whoever reads a chat log wants to know what was said, in an order, a
    /// year later.
    ///
    /// <b>One line per event, always.</b> A message may contain line breaks,
    /// and a log in which one message occupies an unknown number of lines
    /// cannot be read by anything simpler than a parser - grep would show half
    /// a sentence with no author and no time. Line breaks are therefore written
    /// as <c>\n</c> and a backslash as <c>\\</c>, which is reversible, and the
    /// header of every file says so.
    ///
    /// <b>Nothing here may take the console down.</b> A chat log is a
    /// convenience; a full disk, a directory gone missing or a file locked by a
    /// backup must cost the log line and nothing further. Every write is
    /// therefore guarded, and a failure is reported once and then only counted.
    /// </remarks>
    public sealed class ChatLogWriter
    {

        #region Data

        private readonly String        root;
        private readonly Action<String> reportProblem;
        private readonly Lock          fileLock = new();

        private Boolean  problemReported;
        private UInt64   problemsSince;

        #endregion

        #region Properties

        /// <summary>
        /// The directory everything is written under.
        /// </summary>
        public String Root => root;

        /// <summary>
        /// How many writes have failed after the first one, which was reported.
        /// </summary>
        public UInt64 SuppressedProblems => problemsSince;

        #endregion

        #region Constructor(s)

        /// <summary>
        /// Creates a chat log under this directory.
        /// </summary>
        /// <param name="Root">
        /// The directory given with --chatlogs. Relative paths are resolved
        /// against the working directory, once, here - so that a later
        /// Directory.SetCurrentDirectory cannot move the log somewhere else
        /// mid-session.
        /// </param>
        /// <param name="ReportProblem">
        /// How to say that writing failed. Called at most once per session; see
        /// the remarks on the class.
        /// </param>
        public ChatLogWriter(String Root, Action<String> ReportProblem)
        {

            this.root           = Path.GetFullPath(Root);
            this.reportProblem  = ReportProblem;

        }

        #endregion


        #region Message (PeerJID, When, Kind, Actor, Text, Note = null)

        /// <summary>
        /// Records something that was said.
        /// </summary>
        /// <param name="PeerJID">
        /// Whose conversation this belongs to - always the far end, never
        /// ourselves, so that what was said and what was answered stand in one
        /// file.
        /// </param>
        /// <param name="Actor">Who said it, as it should appear.</param>
        /// <param name="Note">
        /// What the line is besides a message: that it is a correction, that it
        /// came in late, that it arrived by carbon.
        /// </param>
        public void Message(String   PeerJID,
                            DateTime When,
                            ChatLogKind Kind,
                            String   Actor,
                            String?  Text,
                            String?  Note   = null)
        {

            var text = Text is null || Text.Length == 0
                           ? "(no content)"
                           : Text;

            Write(PeerJID,
                  When,
                  Kind,
                  Actor,
                  Note is null ? text : $"{text}  [{Note}]");

        }

        #endregion

        #region Presence(PeerJID, When, Actor, State, Status = null)

        /// <summary>
        /// Records that the far end came, went or changed what it says about
        /// itself.
        /// </summary>
        /// <remarks>
        /// The status text goes along where there is one. Without it the line
        /// says "away" where the person wrote "back at four", and the second is
        /// the part somebody reads a log for.
        /// </remarks>
        public void Presence(String   PeerJID,
                             DateTime When,
                             String   Actor,
                             String   State,
                             String?  Status = null)

            => Write(PeerJID,
                     When,
                     ChatLogKind.Presence,
                     Actor,
                     Status is null || Status.Trim().Length == 0
                         ? State
                         : $"{State}  ({Status})");

        #endregion

        #region System  (When, Text, PeerJID = null)

        /// <summary>
        /// Records something about the session itself.
        /// </summary>
        /// <param name="PeerJID">
        /// The conversation it belongs to, where it belongs to one - a contact
        /// request does. Without one it goes to the session log, because a lost
        /// connection belongs to no conversation and to all of them.
        /// </param>
        public void System(DateTime When, String Text, String? PeerJID = null)

            => Write(PeerJID ?? ChatLogPaths.SessionName,
                     When,
                     ChatLogKind.System,
                     PeerJID is null ? "session" : PeerJID,
                     Text);

        #endregion

        #region Media   (PeerJID, When, Text)

        /// <summary>
        /// Records what became of a shared file - stored under this name, or
        /// not fetched and why.
        /// </summary>
        public void Media(String PeerJID, DateTime When, String Text)

            => Write(PeerJID,
                     When,
                     ChatLogKind.Media,
                     PeerJID,
                     Text);

        #endregion


        #region (private) Write(PeerJID, When, Kind, Actor, Text)

        private void Write(String      PeerJID,
                           DateTime    When,
                           ChatLogKind Kind,
                           String      Actor,
                           String      Text)
        {

            var line = String.Concat(When.ToString("yyyy-MM-dd HH:mm:ss"),
                                     "  ",
                                     Marker(Kind),
                                     "  ",
                                     Escape(Actor),
                                     "  ",
                                     Escape(Text));

            try
            {

                var path = ChatLogPaths.LogFileFor(root, PeerJID, When);

                lock (fileLock)
                {

                    Directory.CreateDirectory(Path.GetDirectoryName(path)!);

                    // The header is written when the file is made and never
                    // again, so a month's file explains itself to whoever opens
                    // it without this program at hand.
                    if (!File.Exists(path))
                        File.AppendAllText(path, Header(PeerJID, When), Encoding.UTF8);

                    File.AppendAllText(path, line + Environment.NewLine, Encoding.UTF8);

                }

            }
            catch (Exception e)
            {
                NoteProblem(e);
            }

        }

        #endregion

        #region (private) NoteProblem(Exception)

        private void NoteProblem(Exception Exception)
        {

            lock (fileLock)
            {

                if (problemReported)
                {
                    problemsSince++;
                    return;
                }

                problemReported = true;

            }

            // Outside the lock: whoever reports this writes to the console, and
            // the console has a lock of its own.
            try
            {
                reportProblem($"Chat log not written: {Exception.Message} " +
                              $"(reported once; further failures are counted only)");
            }
            catch
            { }

        }

        #endregion

        #region (private) Marker(Kind) / Header(...) / Escape(Text)

        private static String Marker(ChatLogKind Kind)

            => Kind switch {
                   ChatLogKind.Incoming  => "<",
                   ChatLogKind.Outgoing  => ">",
                   ChatLogKind.Presence  => "*",
                   ChatLogKind.Media     => "@",
                   _                     => "#"
               };


        private static String Header(String PeerJID, DateTime When)

            => String.Concat("# XMPPConsole chat log - ", PeerJID, " - ", When.ToString("yyyy-MM"), Environment.NewLine,
                             "# <date time>  <marker>  <who>  <what>", Environment.NewLine,
                             "# markers: '<' received, '>' sent, '*' presence, '#' session, '@' file", Environment.NewLine,
                             "# a line break inside a message is written as \\n, a backslash as \\\\", Environment.NewLine);


        /// <summary>
        /// Keeps one event on one line, reversibly.
        /// </summary>
        /// <remarks>
        /// The backslash goes first. Escaping the line break first would turn a
        /// message that literally contains "\n" into one indistinguishable from
        /// a message containing a line break.
        /// </remarks>
        internal static String Escape(String Text)

            => Text.Replace("\\", "\\\\").
                    Replace("\r\n", "\\n").
                    Replace("\n",   "\\n").
                    Replace("\r",   "\\n").
                    Replace("\t",   "\\t");

        #endregion

    }

}
