using Server.MirEnvir;
using Shared;

namespace Server
{
    public partial class PetInheritanceForm : Form
    {
        public Envir Envir => SMain.Envir;

        public PetInheritanceForm()
        {
            InitializeComponent();
            UiText.Apply(this);

            ApplyLimits();

            LoadSettings();
        }

        private void ApplyLimits()
        {
            List<NumericUpDown> boxes = new List<NumericUpDown>
            {
                nudTaoistMinDamage, nudTaoistMaxDamage,
                nudTaoistMinDefence, nudTaoistMaxDefence,
                nudTaoistMinMagicDefence, nudTaoistMaxMagicDefence,
                nudTaoistHealth,
                nudWizardMinDamage, nudWizardMaxDamage,
                nudWizardMinDefence, nudWizardMaxDefence,
                nudWizardMinMagicDefence, nudWizardMaxMagicDefence,
                nudWizardHealth
            };

            foreach (NumericUpDown box in boxes)
            {
                box.Minimum = 0;
                box.Maximum = Settings.PetInheritanceMaxPercent;
            }

            nudTaoistHealth.Maximum = Settings.PetInheritanceMaxHealthPerPoint;
            nudWizardHealth.Maximum = Settings.PetInheritanceMaxHealthPerPoint;
        }

        private void LoadSettings()
        {
            PetInheritanceSettings taoist = Settings.TaoistPetInheritance ?? new PetInheritanceSettings { OwnerStat = Stat.MaxSC };
            PetInheritanceSettings wizard = Settings.WizardPetInheritance ?? new PetInheritanceSettings { OwnerStat = Stat.MaxMC };

            chkTaoistEnabled.Checked = taoist.Enabled;
            nudTaoistMinDamage.Value = Clamp(taoist.MinDamagePercent, nudTaoistMinDamage.Maximum);
            nudTaoistMaxDamage.Value = Clamp(taoist.MaxDamagePercent, nudTaoistMaxDamage.Maximum);
            nudTaoistMinDefence.Value = Clamp(taoist.MinDefencePercent, nudTaoistMinDefence.Maximum);
            nudTaoistMaxDefence.Value = Clamp(taoist.MaxDefencePercent, nudTaoistMaxDefence.Maximum);
            nudTaoistMinMagicDefence.Value = Clamp(taoist.MinMagicDefencePercent, nudTaoistMinMagicDefence.Maximum);
            nudTaoistMaxMagicDefence.Value = Clamp(taoist.MaxMagicDefencePercent, nudTaoistMaxMagicDefence.Maximum);
            nudTaoistHealth.Value = Clamp(taoist.HealthPerPoint, nudTaoistHealth.Maximum);

            chkWizardEnabled.Checked = wizard.Enabled;
            nudWizardMinDamage.Value = Clamp(wizard.MinDamagePercent, nudWizardMinDamage.Maximum);
            nudWizardMaxDamage.Value = Clamp(wizard.MaxDamagePercent, nudWizardMaxDamage.Maximum);
            nudWizardMinDefence.Value = Clamp(wizard.MinDefencePercent, nudWizardMinDefence.Maximum);
            nudWizardMaxDefence.Value = Clamp(wizard.MaxDefencePercent, nudWizardMaxDefence.Maximum);
            nudWizardMinMagicDefence.Value = Clamp(wizard.MinMagicDefencePercent, nudWizardMinMagicDefence.Maximum);
            nudWizardMaxMagicDefence.Value = Clamp(wizard.MaxMagicDefencePercent, nudWizardMaxMagicDefence.Maximum);
            nudWizardHealth.Value = Clamp(wizard.HealthPerPoint, nudWizardHealth.Maximum);
        }

        private static decimal Clamp(int value, decimal maximum)
        {
            if (value < 0) return 0;
            if (value > maximum) return maximum;

            return value;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            Settings.TaoistPetInheritance = new PetInheritanceSettings
            {
                OwnerStat = Stat.MaxSC,
                Enabled = chkTaoistEnabled.Checked,
                MinDamagePercent = (int)nudTaoistMinDamage.Value,
                MaxDamagePercent = (int)nudTaoistMaxDamage.Value,
                MinDefencePercent = (int)nudTaoistMinDefence.Value,
                MaxDefencePercent = (int)nudTaoistMaxDefence.Value,
                MinMagicDefencePercent = (int)nudTaoistMinMagicDefence.Value,
                MaxMagicDefencePercent = (int)nudTaoistMaxMagicDefence.Value,
                HealthPerPoint = (int)nudTaoistHealth.Value
            };

            Settings.WizardPetInheritance = new PetInheritanceSettings
            {
                OwnerStat = Stat.MaxMC,
                Enabled = chkWizardEnabled.Checked,
                MinDamagePercent = (int)nudWizardMinDamage.Value,
                MaxDamagePercent = (int)nudWizardMaxDamage.Value,
                MinDefencePercent = (int)nudWizardMinDefence.Value,
                MaxDefencePercent = (int)nudWizardMaxDefence.Value,
                MinMagicDefencePercent = (int)nudWizardMinMagicDefence.Value,
                MaxMagicDefencePercent = (int)nudWizardMaxMagicDefence.Value,
                HealthPerPoint = (int)nudWizardHealth.Value
            };

            Settings.SavePetInheritance();

            // The world loop rebuilds the servant stats, so the editor never edits live objects.
            Envir?.RequiresPetInheritanceUpdate();

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
