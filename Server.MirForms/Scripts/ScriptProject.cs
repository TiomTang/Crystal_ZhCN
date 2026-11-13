using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Server.MirObjects;

namespace Server.MirForms.Scripts
{
    public enum ScriptBlockKind
    {
        Page,
        Check,
        Say,
        Act,
        ElseSay,
        ElseAct,
    }

    public sealed class ScriptBlock
    {
        public ScriptBlockKind Kind;
        public int StartLine;
        public int EndLine;
        public string Tag;
        public string PageKey;
    }

    public sealed class ScriptPageInfo
    {
        public string Key;
        public int Line;
        public readonly List<ScriptBlock> Blocks = new List<ScriptBlock>();
    }

    /// <summary>Facts the checker borrows from the running server; null means "cannot judge".</summary>
    public sealed class ScriptRules
    {
        public Func<string, bool> SpellExists;
        public Func<string, bool> MapExists;
    }

    public sealed class ScriptIssue
    {
        public int Line;
        public bool IsError;
        public string Message;

        public override string ToString()
        {
            return (Line > 0 ? "第 " + Line + " 行  " : string.Empty) + Message;
        }
    }

    /// <summary>
    /// Line model of one NPC script file: pages, blocks, the caret context and the rule checks the
    /// editor runs before the engine parser sees the buffer.
    /// </summary>
    public sealed class ScriptProject
    {
        private static readonly Regex ButtonPattern = new Regex(@"<[^<>]*?/(@[A-Za-z0-9_\-]+)", RegexOptions.Compiled);
        private static readonly Regex ValuePattern = new Regex(@"<\$([^>]+)>", RegexOptions.Compiled);
        private static readonly HashSet<string> KnownTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "IF", "SAY", "ACT", "ELSESAY", "ELSEACT", "INCLUDE", "INSERT",
        };

        private static readonly HashSet<string> GotoKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "GOTO", "GROUPGOTO", "ROLLDIE", "ROLLYUT",
        };

        private static readonly HashSet<string> DelayedGotoKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "TIMERECALL", "TIMERECALLGROUP", "DELAYGOTO",
        };

        private static readonly HashSet<string> SpellActions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "GIVESKILL", "REMOVESKILL", "HEROGIVESKILL",
        };

        private static readonly HashSet<string> MapActions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "MOVE", "INSTANCEMOVE",
        };

        public string[] Lines = new string[0];
        public readonly List<ScriptBlock> Blocks = new List<ScriptBlock>();
        public readonly List<ScriptPageInfo> Pages = new List<ScriptPageInfo>();
        public readonly HashSet<string> PageKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public static ScriptProject Parse(string text)
        {
            var project = new ScriptProject();
            project.Lines = text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');

            ScriptBlock current = null;
            ScriptPageInfo page = null;

            for (int i = 0; i < project.Lines.Length; i++)
            {
                string trimmed = project.Lines[i].Trim().TrimStart('\uFEFF');

                if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
                {
                    if (current != null) current.EndLine = i;

                    current = null;
                    page = new ScriptPageInfo { Key = trimmed, Line = i + 1 };
                    project.Pages.Add(page);
                    project.PageKeys.Add(Normalize(trimmed));
                    continue;
                }

                if (!trimmed.StartsWith("#")) continue;

                string tag = trimmed.Substring(1).Split(' ', '\t')[0].ToUpperInvariant();
                ScriptBlockKind kind;

                switch (tag)
                {
                    case "IF":
                        kind = ScriptBlockKind.Check;
                        break;
                    case "SAY":
                        kind = ScriptBlockKind.Say;
                        break;
                    case "ACT":
                        kind = ScriptBlockKind.Act;
                        break;
                    case "ELSESAY":
                        kind = ScriptBlockKind.ElseSay;
                        break;
                    case "ELSEACT":
                        kind = ScriptBlockKind.ElseAct;
                        break;
                    default:
                        continue;
                }

                if (current != null) current.EndLine = i;

                current = new ScriptBlock
                {
                    Kind = kind,
                    StartLine = i + 1,
                    EndLine = project.Lines.Length,
                    Tag = "#" + tag,
                    PageKey = page == null ? string.Empty : page.Key,
                };

                project.Blocks.Add(current);
                if (page != null) page.Blocks.Add(current);
            }

            return project;
        }

        /// <summary>Block that owns the given 1-based line, or the page when the line sits before any tag.</summary>
        public ScriptBlockKind ContextAt(int line, out string pageKey)
        {
            pageKey = string.Empty;
            ScriptBlockKind kind = ScriptBlockKind.Page;

            foreach (ScriptBlock block in Blocks)
            {
                if (block.StartLine > line) break;

                kind = block.Kind;
                pageKey = block.PageKey;
            }

            return kind;
        }

        public static string Normalize(string pageKey)
        {
            string key = pageKey.Trim().Trim('[', ']');

            int paren = key.IndexOf('(');
            if (paren > 0) key = key.Substring(0, paren) + ")";

            return key.ToUpperInvariant();
        }

        /// <summary>Rule checks that do not need the engine: unknown keywords, arity, links and values.</summary>
        public List<ScriptIssue> Check(ScriptRules rules = null)
        {
            var issues = new List<ScriptIssue>();
            var seenPages = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (ScriptPageInfo page in Pages)
            {
                if (!seenPages.Add(Normalize(page.Key)))
                {
                    issues.Add(new ScriptIssue { Line = page.Line, Message = "页面重复定义：" + page.Key });
                }
            }

            foreach (ScriptBlock block in Blocks)
            {
                for (int i = block.StartLine; i < block.EndLine; i++)
                {
                    string raw = Lines[i - 1];
                    string trimmed = raw.Trim();

                    if (trimmed.Length == 0) continue;

                    if (trimmed.StartsWith(";") || trimmed.StartsWith("//")) continue;

                    if (trimmed.StartsWith("#"))
                    {
                        string tag = trimmed.Substring(1).Split(' ', '\t')[0].ToUpperInvariant();

                        if (!KnownTags.Contains(tag))
                        {
                            issues.Add(new ScriptIssue
                            {
                                Line = i,
                                IsError = true,
                                Message = "引擎不认识的指令：#" + tag + "（加载时会抛异常）",
                            });
                        }

                        continue;
                    }

                    CheckKeywords(block, i, trimmed, rules, issues);
                    CheckLinks(i, trimmed, issues);
                    CheckValues(i, trimmed, issues);
                }
            }

            return issues;
        }

        private void CheckKeywords(ScriptBlock block, int line, string trimmed, ScriptRules rules, List<ScriptIssue> issues)
        {
            if (block.Kind != ScriptBlockKind.Check &&
                block.Kind != ScriptBlockKind.Act &&
                block.Kind != ScriptBlockKind.ElseAct) return;

            string[] parts = trimmed.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 0) return;

            if (parts[0].StartsWith(";") || parts[0].StartsWith("//")) return;

            ScriptKeyword keyword;

            bool found = block.Kind == ScriptBlockKind.Check
                ? ScriptKeywordRegistry.TryGetCheck(parts[0], out keyword)
                : ScriptKeywordRegistry.TryGetAction(parts[0], out keyword);

            if (!found)
            {
                issues.Add(new ScriptIssue
                {
                    Line = line,
                    IsError = true,
                    Message = (block.Kind == ScriptBlockKind.Check ? "未知条件：" : "未知动作：") + parts[0],
                });

                return;
            }

            if (parts.Length < keyword.MinParts)
            {
                issues.Add(new ScriptIssue
                {
                    Line = line,
                    Message = keyword.Name + " 需要 " + (keyword.MinParts - 1) + " 个参数，当前 " + (parts.Length - 1) + " 个",
                });
            }

            if (rules == null) return;

            if (SpellActions.Contains(keyword.Name) && parts.Length > 1 && rules.SpellExists != null && !rules.SpellExists(parts[1]))
            {
                issues.Add(new ScriptIssue { Line = line, Message = "引擎不认识这个技能：" + parts[1] });
            }

            if (MapActions.Contains(keyword.Name) && parts.Length > 1 && rules.MapExists != null && !rules.MapExists(parts[1]))
            {
                issues.Add(new ScriptIssue { Line = line, Message = "找不到这个地图：" + parts[1] });
            }
        }

        private void CheckLinks(int line, string trimmed, List<ScriptIssue> issues)
        {
            foreach (Match match in ButtonPattern.Matches(trimmed))
            {
                string target = match.Groups[1].Value;

                if (string.Equals(target, "@Exit", StringComparison.OrdinalIgnoreCase)) continue;

                if (!PageKeys.Contains(Normalize(target)))
                {
                    issues.Add(new ScriptIssue { Line = line, Message = "按钮指向不存在的页面：" + target });
                }
            }

            string[] parts = trimmed.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 0) return;

            string targetKey = null;

            if (GotoKeywords.Contains(parts[0]) && parts.Length > 1)
                targetKey = parts[1];
            else if (DelayedGotoKeywords.Contains(parts[0]) && parts.Length > 2)
                targetKey = parts[2];

            if (targetKey == null || targetKey.StartsWith("#")) return;

            if (!PageKeys.Contains(Normalize(targetKey)))
            {
                issues.Add(new ScriptIssue { Line = line, Message = parts[0] + " 指向不存在的页面：" + targetKey });
            }
        }

        private void CheckValues(int line, string trimmed, List<ScriptIssue> issues)
        {
            if (!trimmed.Contains("<$")) return;

            foreach (Match match in ValuePattern.Matches(trimmed))
            {
                string name = match.Groups[1].Value.ToUpperInvariant();
                int paren = name.IndexOf('(');

                if (paren >= 0) name = name.Substring(0, paren) + "()";

                ScriptKeyword keyword;

                if (ScriptKeywordRegistry.TryGetVariable(name, out keyword)) continue;
                if (ScriptKeywordRegistry.TryGetMonsterVariable(name, out keyword)) continue;

                issues.Add(new ScriptIssue { Line = line, Message = "未知变量：<$" + match.Groups[1].Value + ">" });
            }
        }
    }
}
