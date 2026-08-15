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
    /// Where a chat log and its media go, and what a JID is allowed to look
    /// like once it has become a directory name.
    /// </summary>
    /// <remarks>
    /// A JID is not a file name, and the difference is not academic. The
    /// localpart of a JID forbids <c>"&amp;'/:&lt;&gt;@</c> (RFC 7622, section
    /// 3.3), but the domain and the resource forbid almost nothing: a resource
    /// is any sequence that survives the PRECIS OpaqueString profile, which
    /// includes <c>\</c>, <c>..</c> and a leading dot. That is a path traversal
    /// waiting for whoever names their resource <c>..\..\..\Windows</c>, and it
    /// is a JID a stranger may choose freely before writing to us.
    ///
    /// Everything outside the permitted set therefore becomes '_'. What that
    /// costs is honest and small: two JIDs that differ only in a forbidden
    /// character share a directory. What it saves is a class of bug in which
    /// the answer to "where did the file go" is "anywhere".
    /// </remarks>
    public static class ChatLogPaths
    {

        /// <summary>
        /// The session log, for what belongs to no conversation - the
        /// connection coming and going, above all.
        /// </summary>
        /// <remarks>
        /// Without an '@' by design: every JID this console logs under is a
        /// bare JID and carries one, so this name cannot collide with one.
        /// </remarks>
        public const String SessionName = "_session";

        /// <summary>
        /// A file name may not grow past what the file system takes. 120 leaves
        /// room for the "_yyyyMM.log" that follows and stays well inside the
        /// 255 bytes the usual file systems allow.
        /// </summary>
        private const Int32 MaxNameLength = 120;

        /// <summary>
        /// Reserved device names on Windows. A file called <c>NUL</c> cannot be
        /// created there, and a JID <c>nul@example.org</c> is nothing unusual -
        /// but only the part before the '@' is the danger, so it is the whole
        /// name that is checked, after shortening.
        /// </summary>
        private static readonly String[] reservedNames = [
            "CON", "PRN", "AUX", "NUL",
            "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
            "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
        ];


        #region SafeName(JID)

        /// <summary>
        /// The JID as a single path segment - never empty, never a traversal,
        /// never a reserved name.
        /// </summary>
        public static String SafeName(String JID)
        {

            var builder = new StringBuilder(JID.Length);

            foreach (var character in JID)
            {

                // Letters, digits and the four that occur in a JID and are
                // harmless in a path. Everything else, including every control
                // character and every separator, becomes '_'.
                if (Char.IsLetterOrDigit(character) ||
                    character == '@' || character == '.' ||
                    character == '-' || character == '_')
                {
                    builder.Append(character);
                }

                else
                    builder.Append('_');

            }

            var name = builder.ToString();

            // '.' and '..' survive the filter above - both consist of permitted
            // characters and both mean something else entirely to a path. A
            // name of dots alone is therefore not a name.
            if (name.Trim('.').Length == 0)
                name = "_";

            // A trailing dot or space is dropped silently by Windows, so
            // "a." and "a" would be the same directory while looking different
            // in the log.
            name = name.TrimEnd('.', ' ');

            if (name.Length == 0)
                name = "_";

            if (name.Length > MaxNameLength)
                name = name[..MaxNameLength];

            if (reservedNames.Contains(name, StringComparer.OrdinalIgnoreCase))
                name = "_" + name;

            return name;

        }

        #endregion

        #region DirectoryFor  (Root, JID)

        /// <summary>
        /// The directory holding everything of one conversation.
        /// </summary>
        public static String DirectoryFor(String Root, String JID)
            => Path.Combine(Root, SafeName(JID));

        #endregion

        #region LogFileFor    (Root, JID, When)

        /// <summary>
        /// The log file of one conversation for the month of <paramref name="When"/>.
        /// </summary>
        /// <remarks>
        /// One file per month, because a chat log is read at the far end of a
        /// year and grep does not care, but an editor does.
        /// </remarks>
        public static String LogFileFor(String Root, String JID, DateTime When)
        {

            var name = SafeName(JID);

            return Path.Combine(Root,
                                name,
                                $"{name}_{When:yyyyMM}.log");

        }

        #endregion

        #region MediaDirectoryFor(Root, JID)

        /// <summary>
        /// Where the files of one conversation are put.
        /// </summary>
        public static String MediaDirectoryFor(String Root, String JID)
            => Path.Combine(Root, SafeName(JID), "media");

        #endregion

        #region MediaFileName (ReceivedAt, SuggestedName)

        /// <summary>
        /// The name a downloaded file gets: when it arrived, then what it was
        /// called.
        /// </summary>
        /// <remarks>
        /// The timestamp goes first so that the directory sorts by time on its
        /// own, and because it is the one part of the name this side is sure
        /// of. The rest comes from the URL and is therefore a stranger's text:
        /// it goes through <see cref="SafeName"/> like a JID does.
        ///
        /// The name is not made unique beyond the second. Two files arriving
        /// within the same second under the same name are the price; the
        /// alternative is a counter in the name, which makes every name a
        /// little unreadable to catch a case that needs two uploads inside one
        /// second.
        /// </remarks>
        public static String MediaFileName(DateTime ReceivedAt, String? SuggestedName)
        {

            var name = SuggestedName is null || SuggestedName.Trim().Length == 0
                           ? "file"
                           : SafeName(SuggestedName);

            return $"{ReceivedAt:yyyyMMdd_HHmmss}_{name}";

        }

        #endregion

    }

}
