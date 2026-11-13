using System.Collections.Generic;

namespace Server.MirObjects
{
    /// <summary>Everything a keyword handler needs about the line it is parsing.</summary>
    public sealed class ScriptParseContext
    {
        /// <summary>Line split on spaces after %ARG substitution.</summary>
        public string[] Parts;

        /// <summary>Raw line, kept for diagnostics.</summary>
        public string Line;

        /// <summary>Script file being parsed, for diagnostics.</summary>
        public string FileName;

        /// <summary>Page the line belongs to, for diagnostics.</summary>
        public string PageKey;

        /// <summary>Segment receiving the parsed checks and actions.</summary>
        public NPCSegment Segment;

        /// <summary>Target list for action keywords.</summary>
        public List<NPCActions> Acts;
    }

    /// <summary>
    /// Remembers the script file currently being parsed so diagnostics can name it. Parsing runs on
    /// the load thread, so a thread-static value is enough.
    /// </summary>
    public static class ScriptParseScope
    {
        [System.ThreadStatic]
        private static string _fileName;

        public static string FileName
        {
            get { return _fileName; }
        }

        public static void Set(string fileName)
        {
            _fileName = fileName;
        }
    }
}
