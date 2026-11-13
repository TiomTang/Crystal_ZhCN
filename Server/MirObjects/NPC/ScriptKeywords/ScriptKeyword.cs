using System;
using System.Collections.Generic;

namespace Server.MirObjects
{
    /// <summary>
    /// Which part of an NPC script a keyword belongs to. The editor filters the command palette by
    /// the block the caret sits in, so this is what decides where a keyword may be inserted.
    /// </summary>
    public enum ScriptKeywordKind
    {
        Action,
        Check,
        Variable,
    }

    /// <summary>Grouping used by the command palette and by the keyword help.</summary>
    public enum ScriptKeywordCategory
    {
        Player,
        Item,
        Social,
        Guild,
        Group,
        Pet,
        Hero,
        Quest,
        World,
        Combat,
        Time,
        Buff,
        Admin,
        Flow,
        Variable,
    }

    /// <summary>Result of matching one script line against the keyword table.</summary>
    public enum ScriptParseOutcome
    {
        /// <summary>The keyword was recognised and its effect applied.</summary>
        Handled,

        /// <summary>The keyword was recognised but the line was too short, so nothing was added.</summary>
        Skipped,

        /// <summary>No keyword of this kind carries that name.</summary>
        Unknown,
    }

    /// <summary>
    /// One row of the NPC script keyword table.
    /// <para>
    /// The table is the single source of truth: dispatch, validation and the editor's command
    /// palette all read it, so adding a keyword means adding one row plus its handler and nothing
    /// else has to be kept in sync by hand.
    /// </para>
    /// </summary>
    public sealed class ScriptKeyword
    {
        public ScriptKeyword(string name, ScriptKeywordKind kind, ScriptKeywordCategory category, string usage,
            string description, int minParts, Func<ScriptParseContext, ScriptParseOutcome> handler)
        {
            Name = name;
            Kind = kind;
            Category = category;
            Usage = usage;
            Description = description;
            MinParts = minParts;
            Handler = handler;
        }

        /// <summary>Keyword as written in scripts, without the leading #.</summary>
        public string Name { get; }

        public ScriptKeywordKind Kind { get; }

        public ScriptKeywordCategory Category { get; }

        /// <summary>Argument template shown by the editor, for example "&lt;operator&gt; &lt;level&gt;".</summary>
        public string Usage { get; }

        /// <summary>
        /// English description, which doubles as the localization key so the editor can show a
        /// translation through GameLanguage.Get.
        /// </summary>
        public string Description { get; }

        /// <summary>Fewest parts a line needs, counting the keyword itself.</summary>
        public int MinParts { get; }

        public Func<ScriptParseContext, ScriptParseOutcome> Handler { get; }
    }
}
