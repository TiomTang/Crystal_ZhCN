using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Server.MirObjects;

namespace Server
{
    /// <summary>
    /// Read-only list of every chat command. Registry rows come from CommandRegistry and script rows
    /// from the default NPC script, so adding a command never needs a change here.
    /// </summary>
    public partial class CommandListForm : Form
    {
        private const int FilterAll = 0;
        private const int FilterPlayer = 1;
        private const int FilterGameMaster = 2;
        private const int FilterScript = 3;

        private readonly List<ScriptCommandCatalog.Entry> _scriptCommands;

        private int _sortColumn = -1;
        private bool _sortAscending = true;

        public CommandListForm()
        {
            InitializeComponent();
            UiText.Apply(this);

            txtSearch.PlaceholderText = Trans("ui.CommandListForm.txtSearch.placeholder", txtSearch.PlaceholderText);

            // Load before touching the filter combo: setting SelectedIndex raises Filter_Changed,
            // which reloads the list and needs the script rows already in place.
            _scriptCommands = SafeLoadScriptCommands();

            cbPermission.Items.Clear();
            cbPermission.Items.Add(Trans("ui.CommandListForm.cbPermission.all", "All commands"));
            cbPermission.Items.Add(Trans("ui.CommandListForm.cbPermission.player", "Player commands only"));
            cbPermission.Items.Add(Trans("ui.CommandListForm.cbPermission.gameMaster", "Game master commands only"));
            cbPermission.Items.Add(Trans("ui.CommandListForm.cbPermission.script", "Script commands only"));
            cbPermission.SelectedIndex = FilterAll;

            Reload();
        }

        /// <summary>
        /// The catalog touches the file system; a broken or missing default script must not stop the
        /// window from opening, it just leaves the registry rows.
        /// </summary>
        private static List<ScriptCommandCatalog.Entry> SafeLoadScriptCommands()
        {
            try
            {
                return ScriptCommandCatalog.Load();
            }
            catch (Exception)
            {
                return new List<ScriptCommandCatalog.Entry>();
            }
        }

        private static string Trans(string key, string source)
        {
            return GameLanguage.ServerStore.Translate(key, source);
        }

        private void Filter_Changed(object sender, EventArgs e)
        {
            Reload();
        }

        private void Reload()
        {
            string search = txtSearch.Text.Trim();
            int filter = cbPermission.SelectedIndex < 0 ? FilterAll : cbPermission.SelectedIndex;

            List<Row> rows = new List<Row>();

            if (filter != FilterScript)
            {
                foreach (CommandInfo command in CommandRegistry.All)
                {
                    if (filter == FilterPlayer && command.Permission != CommandPermission.Player) continue;
                    if (filter == FilterGameMaster && command.Permission != CommandPermission.GameMaster) continue;

                    Row row = RegistryRow(command);
                    if (search.Length > 0 && !Matches(row, search)) continue;

                    rows.Add(row);
                }
            }

            if (filter == FilterAll || filter == FilterScript)
            {
                foreach (ScriptCommandCatalog.Entry entry in _scriptCommands)
                {
                    Row row = ScriptRow(entry);
                    if (search.Length > 0 && !Matches(row, search)) continue;

                    rows.Add(row);
                }
            }

            Sort(rows);

            lvCommands.BeginUpdate();
            lvCommands.Items.Clear();

            foreach (Row row in rows)
            {
                ListViewItem item = new ListViewItem(row.Display)
                {
                    Tag = row,
                };

                item.SubItems.Add(row.Permission);
                item.SubItems.Add(row.Usage);
                item.SubItems.Add(row.Description);
                item.SubItems.Add(row.Category);

                lvCommands.Items.Add(item);
            }

            lvCommands.EndUpdate();

            int total = CommandRegistry.Count + _scriptCommands.Count;
            int gameMaster = CommandRegistry.All.Count(x => x.Permission == CommandPermission.GameMaster);

            lblCount.Text = string.Format(
                Trans("ui.CommandListForm.lblCount", "{0} commands: {1} game master only, {2} available to players, {3} script commands"),
                total, gameMaster, CommandRegistry.Count - gameMaster, _scriptCommands.Count);
        }

        private Row RegistryRow(CommandInfo command)
        {
            return new Row
            {
                Display = "@" + command.Name,
                Name = command.Name,
                Permission = Trans(
                    "ui.CommandPermission." + command.Permission,
                    command.Permission == CommandPermission.GameMaster ? "Game master" : "Player"),
                PermissionLevel = (int)command.Permission,
                Usage = command.Usage,
                Description = DescriptionText(command),
                Category = Trans("ui.CommandCategory." + command.Category, command.Category.ToString()),
                CategoryKey = command.Category.ToString(),
                Copy = command.DisplayUsage,
            };
        }

        private Row ScriptRow(ScriptCommandCatalog.Entry entry)
        {
            return new Row
            {
                Display = "@" + entry.Name,
                Name = entry.Name,
                Permission = Trans("ui.CommandListForm.permission.script", "Script"),
                PermissionLevel = 3,
                Usage = "\u2014",
                Description = ScriptDescription(entry),
                Category = Trans("ui.CommandListForm.category.script", "Script"),
                CategoryKey = "Script",
                Copy = "@" + entry.Name,
            };
        }

        /// <summary>
        /// A script command carries no permission level of its own: the page it runs decides, with
        /// ISADMIN or an item check. Say so instead of guessing a level.
        /// </summary>
        private static string ScriptDescription(ScriptCommandCatalog.Entry entry)
        {
            if (entry.Line <= 0)
            {
                return Trans(
                    "ui.CommandListForm.scriptDescriptionRuntime",
                    "Script command registered at runtime; the script decides what it does and who may use it.");
            }

            return string.Format(
                Trans("ui.CommandListForm.scriptDescription", "Script command at {0} line {1}"),
                entry.SourceFile, entry.Line);
        }

        private static bool Matches(Row row, string search)
        {
            if (row.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0) return true;
            if (row.Description.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0) return true;

            return false;
        }

        private void Sort(List<Row> rows)
        {
            if (_sortColumn < 0)
            {
                rows.Sort((left, right) =>
                {
                    int byCategory = string.CompareOrdinal(left.CategoryKey, right.CategoryKey);
                    return byCategory != 0 ? byCategory : string.CompareOrdinal(left.Name, right.Name);
                });

                return;
            }

            Comparison<Row> comparison = _sortColumn switch
            {
                0 => (left, right) => string.CompareOrdinal(left.Name, right.Name),
                1 => (left, right) => left.PermissionLevel.CompareTo(right.PermissionLevel),
                2 => (left, right) => string.CompareOrdinal(left.Usage, right.Usage),
                3 => (left, right) => string.CompareOrdinal(left.Description, right.Description),
                _ => (left, right) => string.CompareOrdinal(left.CategoryKey, right.CategoryKey),
            };

            rows.Sort(_sortAscending ? comparison : (left, right) => comparison(right, left));
        }

        /// <summary>
        /// The description carries the extra ways a player can reach a GM command, so the
        /// permission column stays a plain level and the row does not need a fourth marker.
        /// </summary>
        private static string DescriptionText(CommandInfo command)
        {
            string text = Trans("ui.Command." + command.Name, command.Description);

            if (command.Permission == CommandPermission.GameMaster && command.TestServerAllowsPlayers)
            {
                text += Trans("ui.CommandListForm.testServerSuffix", " (players on test servers)");
            }

            if (!string.IsNullOrEmpty(command.SpecialItem))
            {
                text += string.Format(Trans("ui.CommandListForm.specialItemSuffix", " (players holding {0})"), command.SpecialItem);
            }

            return text;
        }

        private void lvCommands_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (_sortColumn == e.Column)
            {
                _sortAscending = !_sortAscending;
            }
            else
            {
                _sortColumn = e.Column;
                _sortAscending = true;
            }

            Reload();
        }

        private void lvCommands_DoubleClick(object sender, EventArgs e)
        {
            CopySelected();
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            CopySelected();
        }

        private void CopySelected()
        {
            if (lvCommands.SelectedItems.Count == 0) return;

            if (lvCommands.SelectedItems[0].Tag is not Row row) return;

            try
            {
                Clipboard.SetText(row.Copy);
            }
            catch (Exception)
            {
                // The clipboard can be locked by another process; ignore and keep the window usable.
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>One visible line, either a registry command or a script command.</summary>
        private sealed class Row
        {
            public string Display;
            public string Name;
            public string Permission;
            public int PermissionLevel;
            public string Usage;
            public string Description;
            public string Category;
            public string CategoryKey;
            public string Copy;
        }
    }
}
