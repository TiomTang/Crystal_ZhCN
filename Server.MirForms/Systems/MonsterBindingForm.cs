using Server.MirDatabase;
using Server.MirEnvir;

namespace Server
{
    /// <summary>
    /// Editor for the [Game] monster name references.
    /// <para>
    /// Every value is a monster record name. A name that matches nothing makes the feature that
    /// reads it do nothing without any log line, and the server only validates these names when
    /// EnforceDBChecks is enabled, so the list resolves each entry against the database and shows
    /// the result before anything is saved.
    /// </para>
    /// </summary>
    public partial class MonsterBindingForm : Form
    {
        private readonly Dictionary<string, string> _values = new(StringComparer.Ordinal);
        private readonly Dictionary<string, string> _pending = new(StringComparer.Ordinal);
        private readonly List<MonsterInfo> _monsters;

        public MonsterBindingForm() : this(null)
        {
        }

        /// <param name="monsters">
        /// Monster records used to resolve the names. Left null the form reads the editor world.
        /// </param>
        public MonsterBindingForm(IEnumerable<MonsterInfo> monsters)
        {
            InitializeComponent();
            UiText.Apply(this);

            IEnumerable<MonsterInfo> source = monsters ?? SMain.EditEnvir?.MonsterInfoList;

            _monsters = (source ?? Enumerable.Empty<MonsterInfo>())
                .Where(x => x != null)
                .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();

            foreach (MonsterInfo monster in _monsters)
                cbMonster.Items.Add(monster.Name);

            foreach ((string section, string key, string value) in Settings.GetMonsterNameSettings())
                _values[key] = value;

            Rebuild();
        }

        private void Rebuild()
        {
            string selected = SelectedKey();
            bool onlyUnresolved = chkOnlyUnresolved.Checked;
            int unresolved = 0;

            lvBindings.BeginUpdate();
            lvBindings.Items.Clear();

            foreach (MonsterBindingCatalog.Definition definition in MonsterBindingCatalog.All)
            {
                string value = ValueOf(definition.Key);
                MonsterInfo monster = Resolve(value);

                if (monster == null) unresolved++;
                if (onlyUnresolved && monster != null) continue;

                ListViewItem item = new ListViewItem(GameLanguage.Get(definition.Group));
                item.SubItems.Add(definition.Key);
                item.SubItems.Add(definition.UsedBy);
                item.SubItems.Add(value ?? string.Empty);
                item.SubItems.Add(monster == null
                    ? GameLanguage.Get("Not found")
                    : GameLanguage.Get("index {0}", monster.Index));

                if (monster == null) item.ForeColor = Color.Red;

                item.Tag = definition;

                lvBindings.Items.Add(item);
            }

            lvBindings.EndUpdate();

            lblSummary.Text = GameLanguage.Get("Unresolved: {0} of {1}", unresolved, MonsterBindingCatalog.All.Count);

            if (selected != null) SelectKey(selected);
            else UpdateSelection();
        }

        private string ValueOf(string key)
        {
            if (_pending.TryGetValue(key, out string pending)) return pending;

            return _values.TryGetValue(key, out string value) ? value : null;
        }

        /// <summary>
        /// Mirrors Envir.GetMonsterInfo(name): case insensitive, ordinary spaces ignored.
        /// </summary>
        private MonsterInfo Resolve(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;

            MonsterInfo exact = _monsters.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
            if (exact != null) return exact;

            string compact = name.Replace(" ", string.Empty);

            return _monsters.FirstOrDefault(x => string.Equals(x.Name.Replace(" ", string.Empty), compact, StringComparison.OrdinalIgnoreCase));
        }

        private MonsterBindingCatalog.Definition SelectedDefinition()
        {
            if (lvBindings.SelectedItems.Count == 0) return null;

            return lvBindings.SelectedItems[0].Tag as MonsterBindingCatalog.Definition;
        }

        private string SelectedKey()
        {
            return SelectedDefinition()?.Key;
        }

        private void SelectKey(string key)
        {
            foreach (ListViewItem item in lvBindings.Items)
            {
                if (!string.Equals((item.Tag as MonsterBindingCatalog.Definition)?.Key, key, StringComparison.Ordinal)) continue;

                item.Selected = true;
                item.Focused = true;
                item.EnsureVisible();
                return;
            }

            UpdateSelection();
        }

        private void UpdateSelection()
        {
            MonsterBindingCatalog.Definition definition = SelectedDefinition();

            if (definition == null)
            {
                lblSelection.Text = GameLanguage.Get("Select a row to change the monster it points at.");
                cbMonster.SelectedIndex = -1;
                cbMonster.Enabled = false;
                btnApply.Enabled = false;
                return;
            }

            lblSelection.Text = GameLanguage.Get(definition.Group) + "   /   " + definition.Key + "   [" + definition.UsedBy + "]";

            string value = ValueOf(definition.Key);

            cbMonster.Enabled = _monsters.Count > 0;
            cbMonster.SelectedIndex = string.IsNullOrEmpty(value) ? -1 : cbMonster.FindStringExact(value);
            btnApply.Enabled = cbMonster.SelectedIndex >= 0;
        }

        private void lvBindings_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateSelection();
        }

        private void cbMonster_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnApply.Enabled = SelectedDefinition() != null && cbMonster.SelectedIndex >= 0;
        }

        private void chkOnlyUnresolved_CheckedChanged(object sender, EventArgs e)
        {
            Rebuild();
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            MonsterBindingCatalog.Definition definition = SelectedDefinition();
            if (definition == null || cbMonster.SelectedIndex < 0) return;

            string name = cbMonster.SelectedItem as string;
            if (string.IsNullOrEmpty(name)) return;

            _values.TryGetValue(definition.Key, out string current);

            if (string.Equals(name, current, StringComparison.Ordinal)) _pending.Remove(definition.Key);
            else _pending[definition.Key] = name;

            Rebuild();
            SelectKey(definition.Key);
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (_pending.Count > 0)
            {
                Settings.SetMonsterNameSettings(_pending.Select(x => (MonsterBindingCatalog.Section, x.Key, x.Value)));
                Settings.SaveMonsterNameSettings();

                foreach (var pair in _pending) _values[pair.Key] = pair.Value;

                _pending.Clear();
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
