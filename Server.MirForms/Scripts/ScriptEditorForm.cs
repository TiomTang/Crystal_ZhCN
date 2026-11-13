using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Server.MirEnvir;
using Server.MirObjects;

namespace Server.MirForms.Scripts
{
    /// <summary>
    /// NPC script editor: file tree, structure outline, a keyword palette filtered by the block the
    /// caret sits in, and validation that runs both the rule checks and the engine parser.
    /// </summary>
    public sealed class ScriptEditorForm : Form
    {
        private readonly TreeView _files = new TreeView();
        private readonly RichTextBox _editor = new RichTextBox();
        private readonly TabControl _side = new TabControl();
        private readonly ListBox _palette = new ListBox();
        private readonly TextBox _paletteFilter = new TextBox();
        private readonly TreeView _structure = new TreeView();
        private readonly ListView _problems = new ListView();
        private readonly ToolStripStatusLabel _statusText = new ToolStripStatusLabel();
        private readonly ToolStripStatusLabel _statusIssues = new ToolStripStatusLabel();

        private readonly List<ScriptKeyword> _paletteSource = new List<ScriptKeyword>();

        private string _path;
        private bool _dirty;
        private bool _hasBom = true;
        private string _newLine = "\r\n";
        private Encoding _encoding = new UTF8Encoding(false);

        public ScriptEditorForm()
        {
            Text = GameLanguage.Get("Script Editor");
            StartPosition = FormStartPosition.CenterParent;
            Width = 1200;
            Height = 780;
            MinimumSize = new Size(900, 600);

            BuildUi();
            LoadFileTree();
        }

        private void BuildUi()
        {
            var toolbar = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden };
            toolbar.Items.Add(Button(GameLanguage.Get("Open"), (s, e) => OpenSelectedFile()));
            toolbar.Items.Add(Button(GameLanguage.Get("Save"), (s, e) => SaveFile()));
            toolbar.Items.Add(new ToolStripSeparator());
            toolbar.Items.Add(Button(GameLanguage.Get("Check"), (s, e) => CheckBuffer(true)));
            toolbar.Items.Add(Button(GameLanguage.Get("New Page"), (s, e) => InsertPageTemplate()));

            var status = new StatusStrip();
            _statusText.Spring = true;
            _statusText.TextAlign = ContentAlignment.MiddleLeft;
            status.Items.Add(_statusText);
            status.Items.Add(_statusIssues);

            var root = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
            };

            var body = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                FixedPanel = FixedPanel.Panel2,
            };

            _files.Dock = DockStyle.Fill;
            _files.HideSelection = false;
            _files.DoubleClick += (s, e) => OpenSelectedFile();
            root.Panel1.Controls.Add(_files);

            _editor.Dock = DockStyle.Fill;
            _editor.Font = new Font("Consolas", 10F);
            _editor.WordWrap = false;
            _editor.AcceptsTab = true;
            _editor.HideSelection = false;
            _editor.ScrollBars = RichTextBoxScrollBars.Both;
            _editor.DetectUrls = false;
            _editor.TextChanged += (s, e) => { _dirty = true; RefreshPalette(); };
            _editor.SelectionChanged += (s, e) => { RefreshPalette(); UpdateStatus(); };
            body.Panel1.Controls.Add(_editor);

            var palettePage = new TabPage(GameLanguage.Get("Commands"));
            _paletteFilter.Dock = DockStyle.Top;
            _paletteFilter.PlaceholderText = GameLanguage.Get("Filter");
            _paletteFilter.TextChanged += (s, e) => FillPalette();
            _palette.Dock = DockStyle.Fill;
            _palette.IntegralHeight = false;
            _palette.MouseClick += (s, e) =>
            {
                int index = _palette.IndexFromPoint(e.Location);

                if (index < 0) return;

                _palette.SelectedIndex = index;
                InsertKeyword(_palette.Items[index] as PaletteEntry);
            };
            palettePage.Controls.Add(_palette);
            palettePage.Controls.Add(_paletteFilter);

            var structurePage = new TabPage(GameLanguage.Get("Structure"));
            _structure.Dock = DockStyle.Fill;
            _structure.HideSelection = false;
            _structure.DoubleClick += (s, e) => JumpToStructureNode();
            structurePage.Controls.Add(_structure);

            var problemPage = new TabPage(GameLanguage.Get("Problems"));
            _problems.Dock = DockStyle.Fill;
            _problems.View = View.Details;
            _problems.FullRowSelect = true;
            _problems.Columns.Add(GameLanguage.Get("Line"), 60);
            _problems.Columns.Add(GameLanguage.Get("Message"), 420);
            _problems.DoubleClick += (s, e) => JumpToProblem();
            problemPage.Controls.Add(_problems);

            _side.Dock = DockStyle.Fill;
            _side.TabPages.Add(palettePage);
            _side.TabPages.Add(structurePage);
            _side.TabPages.Add(problemPage);
            body.Panel2.Controls.Add(_side);

            root.Panel2.Controls.Add(body);

            Controls.Add(root);
            Controls.Add(toolbar);
            Controls.Add(status);

            Shown += (s, e) =>
            {
                root.Panel1MinSize = 140;
                root.SplitterDistance = 220;

                body.Panel2MinSize = 320;
                body.SplitterDistance = Math.Max(body.Width - 340, 200);
            };
        }

        private static ToolStripButton Button(string text, EventHandler handler)
        {
            var button = new ToolStripButton(text);
            button.Click += handler;
            return button;
        }

        // ---------------------------------------------------------------- files

        private void LoadFileTree()
        {
            _files.Nodes.Clear();

            string root = Settings.NPCPath;

            if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root))
            {
                _statusText.Text = GameLanguage.Get("NPC script folder is not configured.");
                return;
            }

            var rootNode = new TreeNode(Path.GetFileName(root.TrimEnd(Path.DirectorySeparatorChar))) { Tag = root };
            _files.Nodes.Add(rootNode);
            FillDirectory(rootNode, root, 0);
            rootNode.Expand();
        }

        private void FillDirectory(TreeNode node, string path, int depth)
        {
            string[] files;

            try
            {
                files = Directory.GetFiles(path, "*.txt");
            }
            catch (Exception)
            {
                return;
            }

            foreach (string file in files.OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
            {
                node.Nodes.Add(new TreeNode(Path.GetFileName(file)) { Tag = file });
            }

            if (depth >= 6) return;

            foreach (string directory in Directory.GetDirectories(path).OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
            {
                var child = new TreeNode(Path.GetFileName(directory)) { Tag = directory };
                node.Nodes.Add(child);
                FillDirectory(child, directory, depth + 1);
            }
        }

        private void OpenSelectedFile()
        {
            var node = _files.SelectedNode;

            if (node == null || !(node.Tag is string path) || !File.Exists(path)) return;

            if (!ConfirmDiscard()) return;

            byte[] bytes = File.ReadAllBytes(path);
            _hasBom = bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF;

            int offset = _hasBom ? 3 : 0;
            string text;
            _encoding = new UTF8Encoding(false);

            try
            {
                text = new UTF8Encoding(false, true).GetString(bytes, offset, bytes.Length - offset);
            }
            catch (DecoderFallbackException)
            {
                _encoding = Encoding.GetEncoding(936);
                text = _encoding.GetString(bytes, offset, bytes.Length - offset);
            }

            _newLine = text.Contains("\r\n") ? "\r\n" : "\n";

            _path = path;
            _editor.Text = text.Replace("\r\n", "\n").Replace("\n", "\r\n");
            _editor.SelectionStart = 0;
            _dirty = false;

            _statusText.Text = path;
            RefreshStructure();
            CheckBuffer(false);
            UpdateTitle();
        }

        private void SaveFile()
        {
            if (_path == null) return;

            string text = _editor.Text.Replace("\r\n", "\n");

            if (_newLine != "\n") text = text.Replace("\n", _newLine);

            File.WriteAllText(_path, text, _hasBom ? new UTF8Encoding(true) : _encoding);
            _dirty = false;
            UpdateTitle();
            _statusIssues.Text = GameLanguage.Get("Saved");
        }

        private bool ConfirmDiscard()
        {
            if (!_dirty) return true;

            DialogResult result = MessageBox.Show(this,
                GameLanguage.Get("The script has unsaved changes. Discard them?"),
                GameLanguage.Get("Script Editor"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            return result == DialogResult.Yes;
        }

        private void UpdateTitle()
        {
            Text = (_path == null ? GameLanguage.Get("Script Editor") : Path.GetFileName(_path)) +
                   (_dirty ? " *" : string.Empty) + " - " + GameLanguage.Get("Script Editor");
        }

        // ---------------------------------------------------------------- palette

        private void RefreshPalette()
        {
            int line = _editor.GetLineFromCharIndex(_editor.SelectionStart) + 1;
            ScriptProject project = ScriptProject.Parse(_editor.Text.Replace("\r\n", "\n"));
            ScriptBlockKind kind = project.ContextAt(line, out _);

            _paletteSource.Clear();

            switch (kind)
            {
                case ScriptBlockKind.Check:
                    _paletteSource.AddRange(ScriptKeywordRegistry.Checks);
                    break;
                case ScriptBlockKind.Act:
                case ScriptBlockKind.ElseAct:
                    _paletteSource.AddRange(ScriptKeywordRegistry.Actions);
                    break;
                case ScriptBlockKind.Say:
                case ScriptBlockKind.ElseSay:
                    _paletteSource.AddRange(ScriptKeywordRegistry.Variables);
                    break;
            }

            FillPalette();
        }

        private void FillPalette()
        {
            string filter = _paletteFilter.Text.Trim();

            _palette.BeginUpdate();
            _palette.Items.Clear();

            foreach (ScriptKeyword keyword in _paletteSource)
            {
                var entry = new PaletteEntry(keyword);

                if (filter.Length > 0 &&
                    entry.ToString().IndexOf(filter, StringComparison.OrdinalIgnoreCase) < 0 &&
                    keyword.Name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) < 0) continue;

                _palette.Items.Add(entry);
            }

            _palette.EndUpdate();

            if (_paletteSource.Count == 0)
            {
                _palette.Items.Add(new PaletteEntry(null));
            }
        }

        private void InsertKeyword(PaletteEntry entry)
        {
            if (entry == null || entry.Keyword == null) return;

            ScriptKeyword keyword = entry.Keyword;
            string usage = keyword.Kind == ScriptKeywordKind.Variable
                ? "<$" + keyword.Name + ">"
                : keyword.Name + (keyword.Usage.Length > 0 ? " " + keyword.Usage : string.Empty);

            int start = _editor.SelectionStart;
            _editor.SelectedText = usage;
            _editor.SelectionStart = start + (keyword.Kind == ScriptKeywordKind.Variable ? 0 : keyword.Name.Length + 1);
            _editor.SelectionLength = keyword.Kind == ScriptKeywordKind.Variable ? 0 : keyword.Usage.Length;
            _editor.Focus();
        }

        private void InsertPageTemplate()
        {
            _editor.SelectedText = "[@NewPage]\r\n#SAY\r\n\r\n\r\n#ACT\r\n\r\n";
            _editor.Focus();
        }

        // ---------------------------------------------------------------- structure and problems

        private void RefreshStructure()
        {
            _structure.BeginUpdate();
            _structure.Nodes.Clear();

            ScriptProject project = ScriptProject.Parse(_editor.Text.Replace("\r\n", "\n"));
            var nodes = new Dictionary<string, TreeNode>(StringComparer.OrdinalIgnoreCase);

            foreach (ScriptPageInfo page in project.Pages)
            {
                var node = new TreeNode(page.Key + "    " + page.Line) { Tag = page.Line };
                nodes[page.Key] = node;
                _structure.Nodes.Add(node);

                foreach (ScriptBlock block in page.Blocks)
                {
                    node.Nodes.Add(new TreeNode(block.Tag + "    " + block.StartLine) { Tag = block.StartLine });
                }
            }

            _structure.EndUpdate();
        }

        private void JumpToStructureNode()
        {
            if (_structure.SelectedNode?.Tag is int line) JumpToLine(line);
        }

        private void JumpToProblem()
        {
            if (_problems.SelectedItems.Count == 0) return;

            if (_problems.SelectedItems[0].Tag is int line) JumpToLine(line);
        }

        private void JumpToLine(int line)
        {
            if (line < 1) line = 1;
            if (line > _editor.Lines.Length) line = _editor.Lines.Length;

            int index = _editor.GetFirstCharIndexFromLine(line - 1);

            if (index < 0) return;

            _editor.SelectionStart = index;
            _editor.SelectionLength = _editor.Lines[line - 1].Length;
            _editor.ScrollToCaret();
            _editor.Focus();
        }

        private void CheckBuffer(bool report)
        {
            ScriptProject project = ScriptProject.Parse(_editor.Text.Replace("\r\n", "\n"));
            List<ScriptIssue> issues = project.Check();

            string engineError = EngineCheck();

            if (engineError != null)
            {
                issues.Insert(0, new ScriptIssue { Line = 0, IsError = true, Message = engineError });
            }

            _problems.BeginUpdate();
            _problems.Items.Clear();

            foreach (ScriptIssue issue in issues.OrderBy(x => x.Line))
            {
                var item = new ListViewItem(issue.Line > 0 ? issue.Line.ToString() : "-");
                item.SubItems.Add((issue.IsError ? "" : "") + issue.Message);
                item.ForeColor = issue.IsError ? Color.Firebrick : Color.DimGray;
                item.Tag = issue.Line;
                _problems.Items.Add(item);
            }

            _problems.EndUpdate();

            int errors = issues.Count(x => x.IsError);
            _statusIssues.Text = issues.Count == 0
                ? GameLanguage.Get("No problems found")
                : string.Format(GameLanguage.Get("{0} error(s), {1} warning(s)"), errors, issues.Count - errors);

            RefreshStructure();

            if (report && issues.Count == 0)
            {
                MessageBox.Show(this, GameLanguage.Get("No problems found"), GameLanguage.Get("Script Editor"),
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            if (report) _side.SelectedIndex = 2;
        }

        /// <summary>
        /// Runs the buffer through the real parser: the engine throws on tags it does not know, which
        /// no rule check can predict. The temporary file sits in the NPC folder because the loader
        /// resolves scripts relative to it, and it is deleted straight after parsing.
        /// </summary>
        private string EngineCheck()
        {
            if (string.IsNullOrWhiteSpace(Settings.NPCPath) || !Directory.Exists(Settings.NPCPath)) return null;

            string path = Path.Combine(Settings.NPCPath, "_editor_check.txt");

            try
            {
                File.WriteAllText(path, _editor.Text, new UTF8Encoding(false));

                NPCScript script = NPCScript.GetOrAdd(0u, "_editor_check", NPCScriptType.Normal);
                script.LoadInfo();

                return null;
            }
            catch (Exception exception)
            {
                return GameLanguage.Get("Engine rejected the script:") + " " + exception.Message;
            }
            finally
            {
                try
                {
                    if (File.Exists(path)) File.Delete(path);
                }
                catch (IOException)
                {
                }
            }
        }

        private void UpdateStatus()
        {
            int index = _editor.GetLineFromCharIndex(_editor.SelectionStart);
            int column = _editor.SelectionStart - _editor.GetFirstCharIndexFromLine(index);

            if (_path != null)
                _statusText.Text = _path + "   " + (index + 1) + ":" + (column + 1);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (!ConfirmDiscard()) e.Cancel = true;
            base.OnFormClosing(e);
        }

        private sealed class PaletteEntry
        {
            public readonly ScriptKeyword Keyword;

            public PaletteEntry(ScriptKeyword keyword)
            {
                Keyword = keyword;
            }

            public override string ToString()
            {
                if (Keyword == null) return GameLanguage.Get("No keyword here");

                string description = GameLanguage.Get(Keyword.Description);

                return Keyword.Name + "   " + description;
            }
        }
    }
}
