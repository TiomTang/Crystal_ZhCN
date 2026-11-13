using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Server.MirEnvir;
using Server.MirObjects;

namespace Server
{
    /// <summary>
    /// Script-level chat commands. The server registers a name for every line of the default NPC
    /// script that mentions CUSTOMCOMMAND and carries a (name) token, then runs the script page
    /// [@_CUSTOMCOMMAND(name)] when a player types @name. The runtime list is the authority for
    /// what works; the file scan only says where each name is written down.
    /// </summary>
    public static class ScriptCommandCatalog
    {
        public sealed class Entry
        {
            /// <summary>Name the player types after @.</summary>
            public string Name;

            /// <summary>File the registration line was found in; empty when only the runtime knows it.</summary>
            public string SourceFile;

            /// <summary>One-based line number inside SourceFile; zero when unknown.</summary>
            public int Line;
        }

        private static readonly Regex NamePattern = new Regex(@"\((.*?)\)", RegexOptions.Compiled);

        /// <summary>Names the running server has registered, plus anything the file scan finds.</summary>
        public static List<Entry> Load()
        {
            List<Entry> entries = new List<Entry>();
            HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (Entry entry in Scan(Settings.EnvirPath, Settings.NPCPath, Settings.DefaultNPCFilename))
            {
                if (seen.Add(entry.Name)) entries.Add(entry);
            }

            foreach (string name in Envir.Main.CustomCommands)
            {
                if (string.IsNullOrWhiteSpace(name)) continue;
                if (!seen.Add(name)) continue;

                entries.Add(new Entry { Name = name, SourceFile = string.Empty, Line = 0 });
            }

            entries.Sort((left, right) => string.Compare(left.Name, right.Name, StringComparison.OrdinalIgnoreCase));

            return entries;
        }

        /// <summary>
        /// Repeats what NPCScript does for the default script: read the file, append every #INSERT
        /// target once (the loader resolves only the top level), then treat any line containing
        /// CUSTOMCOMMAND as a registration and take its first (name) token.
        /// </summary>
        public static List<Entry> Scan(string envirPath, string npcPath, string defaultFileName)
        {
            List<Entry> entries = new List<Entry>();

            string main = Path.Combine(npcPath, defaultFileName + ".txt");
            if (!File.Exists(main)) return entries;

            List<SourceLine> lines = ReadFile(main);

            foreach (SourceLine line in lines.ToArray())
            {
                if (!line.Text.ToUpper().StartsWith("#INSERT")) continue;

                string[] split = line.Text.Split(' ');
                if (split.Length < 2 || split[1].Length < 3) continue;

                string target = Path.Combine(envirPath, split[1].Substring(1, split[1].Length - 2));
                if (!File.Exists(target)) continue;

                lines.AddRange(ReadFile(target));
            }

            HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (SourceLine line in lines)
            {
                if (!line.Text.ToUpper().Contains("CUSTOMCOMMAND")) continue;

                Match match = NamePattern.Match(line.Text);
                if (!match.Success) continue;

                string name = match.Groups[1].Value.Trim();
                if (name.Length == 0) continue;
                if (!seen.Add(name)) continue;

                entries.Add(new Entry { Name = name, SourceFile = Path.GetFileName(line.File), Line = line.Number });
            }

            entries.Sort((left, right) => string.Compare(left.Name, right.Name, StringComparison.OrdinalIgnoreCase));

            return entries;
        }

        private static List<SourceLine> ReadFile(string path)
        {
            List<SourceLine> lines = new List<SourceLine>();

            try
            {
                string[] raw = File.ReadAllLines(path);

                for (int i = 0; i < raw.Length; i++)
                {
                    lines.Add(new SourceLine { Text = raw[i], File = path, Number = i + 1 });
                }
            }
            catch (IOException)
            {
                // A locked or half-written file just contributes no rows; the page stays usable.
            }
            catch (UnauthorizedAccessException)
            {
            }

            return lines;
        }

        private sealed class SourceLine
        {
            public string Text;
            public string File;
            public int Number;
        }
    }
}
