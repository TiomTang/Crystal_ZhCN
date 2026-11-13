using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Server.MirObjects;

namespace Server
{
    /// <summary>
    /// Read-only list of every chat command. Rows come from CommandRegistry, so adding a command
    /// never needs a change here.
    /// </summary>
    public partial class CommandListForm : Form
    {
        private const int FilterAll = 0;
        private const int FilterPlayer = 1;
        private const int FilterGameMaster = 2;

        private int _sortColumn = -1;
        private bool _sortAscending = true;

        public CommandListForm()
        {
            InitializeComponent();
            UiText.Apply(this);

            txtSearch.PlaceholderText = Trans("ui.CommandListForm.txtSearch.placeholder", txtSearch.PlaceholderText);

            cbPermission.Items.Clear();
            cbPermission.Items.Add(Trans("ui.CommandListForm.cbPermission.all", "All commands"));
            cbPermission.Items.Add(Trans("ui.CommandListForm.cbPermission.player", "Player commands only"));
            cbPermission.Items.Add(Trans("ui.CommandListForm.cbPermission.gameMaster", "Game master commands only"));
            cbPermission.SelectedIndex = FilterAll;

            Reload();
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

            List<CommandInfo> rows = new List<CommandInfo>();

            foreach (CommandInfo command in CommandRegistry.All)
            {
                if (filter == FilterPlayer && command.Permission != CommandPermission.Player) continue;
                if (filter == FilterGameMaster && command.Permission != CommandPermission.GameMaster) continue;

                if (search.Length > 0 && !Matches(command, search)) continue;

                rows.Add(command);
            }

            Sort(rows);

            lvCommands.BeginUpdate();
            lvCommands.Items.Clear();

            foreach (CommandInfo command in rows)
            {
                ListViewItem item = new ListViewItem("@" + command.Name)
                {
                    Tag = command,
                };

                item.SubItems.Add(PermissionText(command));
                item.SubItems.Add(command.Usage);
                item.SubItems.Add(DescriptionText(command));
                item.SubItems.Add(Trans("ui.CommandCategory." + command.Category, command.Category.ToString()));

                lvCommands.Items.Add(item);
            }

            lvCommands.EndUpdate();

            int total = CommandRegistry.Count;
            int gameMaster = CommandRegistry.All.Count(x => x.Permission == CommandPermission.GameMaster);

            lblCount.Text = string.Format(
                Trans("ui.CommandListForm.lblCount", "{0} commands: {1} game master only, {2} available to players"),
                total, gameMaster, total - gameMaster);
        }

        private static bool Matches(CommandInfo command, string search)
        {
            if (command.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0) return true;
            if (command.Description.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0) return true;

            string translated = Trans("ui.Command." + command.Name, command.Description);
            return translated.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void Sort(List<CommandInfo> rows)
        {
            if (_sortColumn < 0)
            {
                rows.Sort((left, right) =>
                {
                    int byCategory = left.Category.CompareTo(right.Category);
                    return byCategory != 0 ? byCategory : string.CompareOrdinal(left.Name, right.Name);
                });

                return;
            }

            Comparison<CommandInfo> comparison = _sortColumn switch
            {
                0 => (left, right) => string.CompareOrdinal(left.Name, right.Name),
                1 => (left, right) => left.Permission.CompareTo(right.Permission),
                2 => (left, right) => string.CompareOrdinal(left.Usage, right.Usage),
                3 => (left, right) => string.CompareOrdinal(
                    Trans("ui.Command." + left.Name, left.Description),
                    Trans("ui.Command." + right.Name, right.Description)),
                _ => (left, right) => left.Category.CompareTo(right.Category),
            };

            rows.Sort(_sortAscending ? comparison : (left, right) => comparison(right, left));
        }

        private static string PermissionText(CommandInfo command)
        {
            return Trans(
                "ui.CommandPermission." + command.Permission,
                command.Permission == CommandPermission.GameMaster ? "Game master" : "Player");
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

            if (lvCommands.SelectedItems[0].Tag is not CommandInfo command) return;

            try
            {
                Clipboard.SetText(command.DisplayUsage);
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
    }
}
