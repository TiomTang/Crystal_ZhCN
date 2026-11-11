namespace Server
{
    partial class PetInheritanceForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblHint = new System.Windows.Forms.Label();
            this.grpTaoist = new System.Windows.Forms.GroupBox();
            this.lblTaoistHealth = new System.Windows.Forms.Label();
            this.nudTaoistHealth = new System.Windows.Forms.NumericUpDown();
            this.lblTaoistMagicDefence = new System.Windows.Forms.Label();
            this.nudTaoistMinMagicDefence = new System.Windows.Forms.NumericUpDown();
            this.nudTaoistMaxMagicDefence = new System.Windows.Forms.NumericUpDown();
            this.lblTaoistDefence = new System.Windows.Forms.Label();
            this.nudTaoistMinDefence = new System.Windows.Forms.NumericUpDown();
            this.nudTaoistMaxDefence = new System.Windows.Forms.NumericUpDown();
            this.lblTaoistDamage = new System.Windows.Forms.Label();
            this.nudTaoistMinDamage = new System.Windows.Forms.NumericUpDown();
            this.nudTaoistMaxDamage = new System.Windows.Forms.NumericUpDown();
            this.lblTaoistSource = new System.Windows.Forms.Label();
            this.chkTaoistEnabled = new System.Windows.Forms.CheckBox();
            this.grpWizard = new System.Windows.Forms.GroupBox();
            this.lblWizardHealth = new System.Windows.Forms.Label();
            this.nudWizardHealth = new System.Windows.Forms.NumericUpDown();
            this.lblWizardMagicDefence = new System.Windows.Forms.Label();
            this.nudWizardMinMagicDefence = new System.Windows.Forms.NumericUpDown();
            this.nudWizardMaxMagicDefence = new System.Windows.Forms.NumericUpDown();
            this.lblWizardDefence = new System.Windows.Forms.Label();
            this.nudWizardMinDefence = new System.Windows.Forms.NumericUpDown();
            this.nudWizardMaxDefence = new System.Windows.Forms.NumericUpDown();
            this.lblWizardDamage = new System.Windows.Forms.Label();
            this.nudWizardMinDamage = new System.Windows.Forms.NumericUpDown();
            this.nudWizardMaxDamage = new System.Windows.Forms.NumericUpDown();
            this.lblWizardSource = new System.Windows.Forms.Label();
            this.chkWizardEnabled = new System.Windows.Forms.CheckBox();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.grpTaoist.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudTaoistHealth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudTaoistMinMagicDefence)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudTaoistMaxMagicDefence)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudTaoistMinDefence)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudTaoistMaxDefence)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudTaoistMinDamage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudTaoistMaxDamage)).BeginInit();
            this.grpWizard.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudWizardHealth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudWizardMinMagicDefence)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudWizardMaxMagicDefence)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudWizardMinDefence)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudWizardMaxDefence)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudWizardMinDamage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudWizardMaxDamage)).BeginInit();
            this.SuspendLayout();
            // 
            // lblHint
            // 
            this.lblHint.Location = new System.Drawing.Point(12, 9);
            this.lblHint.Name = "lblHint";
            this.lblHint.Size = new System.Drawing.Size(500, 32);
            this.lblHint.TabIndex = 0;
            this.lblHint.Text = "Inherited values are added on top of the servant template and pet level. Percentages use the owner core attribute, health is added per attribute point.";
            // 
            // grpTaoist
            // 
            this.grpTaoist.Controls.Add(this.lblTaoistHealth);
            this.grpTaoist.Controls.Add(this.nudTaoistHealth);
            this.grpTaoist.Controls.Add(this.lblTaoistMagicDefence);
            this.grpTaoist.Controls.Add(this.nudTaoistMinMagicDefence);
            this.grpTaoist.Controls.Add(this.nudTaoistMaxMagicDefence);
            this.grpTaoist.Controls.Add(this.lblTaoistDefence);
            this.grpTaoist.Controls.Add(this.nudTaoistMinDefence);
            this.grpTaoist.Controls.Add(this.nudTaoistMaxDefence);
            this.grpTaoist.Controls.Add(this.lblTaoistDamage);
            this.grpTaoist.Controls.Add(this.nudTaoistMinDamage);
            this.grpTaoist.Controls.Add(this.nudTaoistMaxDamage);
            this.grpTaoist.Controls.Add(this.lblTaoistSource);
            this.grpTaoist.Controls.Add(this.chkTaoistEnabled);
            this.grpTaoist.Location = new System.Drawing.Point(12, 44);
            this.grpTaoist.Name = "grpTaoist";
            this.grpTaoist.Size = new System.Drawing.Size(500, 150);
            this.grpTaoist.TabIndex = 1;
            this.grpTaoist.TabStop = false;
            this.grpTaoist.Text = "Taoist summons (skeleton, shinsu, holy deva)";
            // 
            // chkTaoistEnabled
            // 
            this.chkTaoistEnabled.AutoSize = true;
            this.chkTaoistEnabled.Location = new System.Drawing.Point(14, 22);
            this.chkTaoistEnabled.Name = "chkTaoistEnabled";
            this.chkTaoistEnabled.Size = new System.Drawing.Size(114, 17);
            this.chkTaoistEnabled.TabIndex = 0;
            this.chkTaoistEnabled.Text = "Enable inheritance";
            this.chkTaoistEnabled.UseVisualStyleBackColor = true;
            // 
            // lblTaoistSource
            // 
            this.lblTaoistSource.AutoSize = true;
            this.lblTaoistSource.Location = new System.Drawing.Point(180, 23);
            this.lblTaoistSource.Name = "lblTaoistSource";
            this.lblTaoistSource.Size = new System.Drawing.Size(188, 13);
            this.lblTaoistSource.TabIndex = 1;
            this.lblTaoistSource.Text = "Owner attribute source: max Taoism (SC)";
            // 
            // lblTaoistDamage
            // 
            this.lblTaoistDamage.AutoSize = true;
            this.lblTaoistDamage.Location = new System.Drawing.Point(14, 52);
            this.lblTaoistDamage.Name = "lblTaoistDamage";
            this.lblTaoistDamage.Size = new System.Drawing.Size(124, 13);
            this.lblTaoistDamage.TabIndex = 2;
            this.lblTaoistDamage.Text = "Attack min / max percentage";
            // 
            // nudTaoistMinDamage
            // 
            this.nudTaoistMinDamage.Location = new System.Drawing.Point(183, 50);
            this.nudTaoistMinDamage.Name = "nudTaoistMinDamage";
            this.nudTaoistMinDamage.Size = new System.Drawing.Size(70, 20);
            this.nudTaoistMinDamage.TabIndex = 3;
            // 
            // nudTaoistMaxDamage
            // 
            this.nudTaoistMaxDamage.Location = new System.Drawing.Point(263, 50);
            this.nudTaoistMaxDamage.Name = "nudTaoistMaxDamage";
            this.nudTaoistMaxDamage.Size = new System.Drawing.Size(70, 20);
            this.nudTaoistMaxDamage.TabIndex = 4;
            // 
            // lblTaoistDefence
            // 
            this.lblTaoistDefence.AutoSize = true;
            this.lblTaoistDefence.Location = new System.Drawing.Point(14, 78);
            this.lblTaoistDefence.Name = "lblTaoistDefence";
            this.lblTaoistDefence.Size = new System.Drawing.Size(148, 13);
            this.lblTaoistDefence.TabIndex = 5;
            this.lblTaoistDefence.Text = "Defence min / max percentage";
            // 
            // nudTaoistMinDefence
            // 
            this.nudTaoistMinDefence.Location = new System.Drawing.Point(183, 76);
            this.nudTaoistMinDefence.Name = "nudTaoistMinDefence";
            this.nudTaoistMinDefence.Size = new System.Drawing.Size(70, 20);
            this.nudTaoistMinDefence.TabIndex = 6;
            // 
            // nudTaoistMaxDefence
            // 
            this.nudTaoistMaxDefence.Location = new System.Drawing.Point(263, 76);
            this.nudTaoistMaxDefence.Name = "nudTaoistMaxDefence";
            this.nudTaoistMaxDefence.Size = new System.Drawing.Size(70, 20);
            this.nudTaoistMaxDefence.TabIndex = 7;
            // 
            // lblTaoistMagicDefence
            // 
            this.lblTaoistMagicDefence.AutoSize = true;
            this.lblTaoistMagicDefence.Location = new System.Drawing.Point(14, 104);
            this.lblTaoistMagicDefence.Name = "lblTaoistMagicDefence";
            this.lblTaoistMagicDefence.Size = new System.Drawing.Size(168, 13);
            this.lblTaoistMagicDefence.TabIndex = 8;
            this.lblTaoistMagicDefence.Text = "Magic defence min / max percentage";
            // 
            // nudTaoistMinMagicDefence
            // 
            this.nudTaoistMinMagicDefence.Location = new System.Drawing.Point(183, 102);
            this.nudTaoistMinMagicDefence.Name = "nudTaoistMinMagicDefence";
            this.nudTaoistMinMagicDefence.Size = new System.Drawing.Size(70, 20);
            this.nudTaoistMinMagicDefence.TabIndex = 9;
            // 
            // nudTaoistMaxMagicDefence
            // 
            this.nudTaoistMaxMagicDefence.Location = new System.Drawing.Point(263, 102);
            this.nudTaoistMaxMagicDefence.Name = "nudTaoistMaxMagicDefence";
            this.nudTaoistMaxMagicDefence.Size = new System.Drawing.Size(70, 20);
            this.nudTaoistMaxMagicDefence.TabIndex = 10;
            // 
            // lblTaoistHealth
            // 
            this.lblTaoistHealth.AutoSize = true;
            this.lblTaoistHealth.Location = new System.Drawing.Point(14, 126);
            this.lblTaoistHealth.Name = "lblTaoistHealth";
            this.lblTaoistHealth.Size = new System.Drawing.Size(133, 13);
            this.lblTaoistHealth.TabIndex = 11;
            this.lblTaoistHealth.Text = "Health per attribute point";
            // 
            // nudTaoistHealth
            // 
            this.nudTaoistHealth.Location = new System.Drawing.Point(183, 124);
            this.nudTaoistHealth.Name = "nudTaoistHealth";
            this.nudTaoistHealth.Size = new System.Drawing.Size(70, 20);
            this.nudTaoistHealth.TabIndex = 12;
            // 
            // grpWizard
            // 
            this.grpWizard.Controls.Add(this.lblWizardHealth);
            this.grpWizard.Controls.Add(this.nudWizardHealth);
            this.grpWizard.Controls.Add(this.lblWizardMagicDefence);
            this.grpWizard.Controls.Add(this.nudWizardMinMagicDefence);
            this.grpWizard.Controls.Add(this.nudWizardMaxMagicDefence);
            this.grpWizard.Controls.Add(this.lblWizardDefence);
            this.grpWizard.Controls.Add(this.nudWizardMinDefence);
            this.grpWizard.Controls.Add(this.nudWizardMaxDefence);
            this.grpWizard.Controls.Add(this.lblWizardDamage);
            this.grpWizard.Controls.Add(this.nudWizardMinDamage);
            this.grpWizard.Controls.Add(this.nudWizardMaxDamage);
            this.grpWizard.Controls.Add(this.lblWizardSource);
            this.grpWizard.Controls.Add(this.chkWizardEnabled);
            this.grpWizard.Location = new System.Drawing.Point(12, 202);
            this.grpWizard.Name = "grpWizard";
            this.grpWizard.Size = new System.Drawing.Size(500, 150);
            this.grpWizard.TabIndex = 2;
            this.grpWizard.TabStop = false;
            this.grpWizard.Text = "Wizard tamed pets";
            // 
            // chkWizardEnabled
            // 
            this.chkWizardEnabled.AutoSize = true;
            this.chkWizardEnabled.Location = new System.Drawing.Point(14, 22);
            this.chkWizardEnabled.Name = "chkWizardEnabled";
            this.chkWizardEnabled.Size = new System.Drawing.Size(114, 17);
            this.chkWizardEnabled.TabIndex = 0;
            this.chkWizardEnabled.Text = "Enable inheritance";
            this.chkWizardEnabled.UseVisualStyleBackColor = true;
            // 
            // lblWizardSource
            // 
            this.lblWizardSource.AutoSize = true;
            this.lblWizardSource.Location = new System.Drawing.Point(180, 23);
            this.lblWizardSource.Name = "lblWizardSource";
            this.lblWizardSource.Size = new System.Drawing.Size(183, 13);
            this.lblWizardSource.TabIndex = 1;
            this.lblWizardSource.Text = "Owner attribute source: max Magic (MC)";
            // 
            // lblWizardDamage
            // 
            this.lblWizardDamage.AutoSize = true;
            this.lblWizardDamage.Location = new System.Drawing.Point(14, 52);
            this.lblWizardDamage.Name = "lblWizardDamage";
            this.lblWizardDamage.Size = new System.Drawing.Size(124, 13);
            this.lblWizardDamage.TabIndex = 2;
            this.lblWizardDamage.Text = "Attack min / max percentage";
            // 
            // nudWizardMinDamage
            // 
            this.nudWizardMinDamage.Location = new System.Drawing.Point(183, 50);
            this.nudWizardMinDamage.Name = "nudWizardMinDamage";
            this.nudWizardMinDamage.Size = new System.Drawing.Size(70, 20);
            this.nudWizardMinDamage.TabIndex = 3;
            // 
            // nudWizardMaxDamage
            // 
            this.nudWizardMaxDamage.Location = new System.Drawing.Point(263, 50);
            this.nudWizardMaxDamage.Name = "nudWizardMaxDamage";
            this.nudWizardMaxDamage.Size = new System.Drawing.Size(70, 20);
            this.nudWizardMaxDamage.TabIndex = 4;
            // 
            // lblWizardDefence
            // 
            this.lblWizardDefence.AutoSize = true;
            this.lblWizardDefence.Location = new System.Drawing.Point(14, 78);
            this.lblWizardDefence.Name = "lblWizardDefence";
            this.lblWizardDefence.Size = new System.Drawing.Size(148, 13);
            this.lblWizardDefence.TabIndex = 5;
            this.lblWizardDefence.Text = "Defence min / max percentage";
            // 
            // nudWizardMinDefence
            // 
            this.nudWizardMinDefence.Location = new System.Drawing.Point(183, 76);
            this.nudWizardMinDefence.Name = "nudWizardMinDefence";
            this.nudWizardMinDefence.Size = new System.Drawing.Size(70, 20);
            this.nudWizardMinDefence.TabIndex = 6;
            // 
            // nudWizardMaxDefence
            // 
            this.nudWizardMaxDefence.Location = new System.Drawing.Point(263, 76);
            this.nudWizardMaxDefence.Name = "nudWizardMaxDefence";
            this.nudWizardMaxDefence.Size = new System.Drawing.Size(70, 20);
            this.nudWizardMaxDefence.TabIndex = 7;
            // 
            // lblWizardMagicDefence
            // 
            this.lblWizardMagicDefence.AutoSize = true;
            this.lblWizardMagicDefence.Location = new System.Drawing.Point(14, 104);
            this.lblWizardMagicDefence.Name = "lblWizardMagicDefence";
            this.lblWizardMagicDefence.Size = new System.Drawing.Size(168, 13);
            this.lblWizardMagicDefence.TabIndex = 8;
            this.lblWizardMagicDefence.Text = "Magic defence min / max percentage";
            // 
            // nudWizardMinMagicDefence
            // 
            this.nudWizardMinMagicDefence.Location = new System.Drawing.Point(183, 102);
            this.nudWizardMinMagicDefence.Name = "nudWizardMinMagicDefence";
            this.nudWizardMinMagicDefence.Size = new System.Drawing.Size(70, 20);
            this.nudWizardMinMagicDefence.TabIndex = 9;
            // 
            // nudWizardMaxMagicDefence
            // 
            this.nudWizardMaxMagicDefence.Location = new System.Drawing.Point(263, 102);
            this.nudWizardMaxMagicDefence.Name = "nudWizardMaxMagicDefence";
            this.nudWizardMaxMagicDefence.Size = new System.Drawing.Size(70, 20);
            this.nudWizardMaxMagicDefence.TabIndex = 10;
            // 
            // lblWizardHealth
            // 
            this.lblWizardHealth.AutoSize = true;
            this.lblWizardHealth.Location = new System.Drawing.Point(14, 126);
            this.lblWizardHealth.Name = "lblWizardHealth";
            this.lblWizardHealth.Size = new System.Drawing.Size(133, 13);
            this.lblWizardHealth.TabIndex = 11;
            this.lblWizardHealth.Text = "Health per attribute point";
            // 
            // nudWizardHealth
            // 
            this.nudWizardHealth.Location = new System.Drawing.Point(183, 124);
            this.nudWizardHealth.Name = "nudWizardHealth";
            this.nudWizardHealth.Size = new System.Drawing.Size(70, 20);
            this.nudWizardHealth.TabIndex = 12;
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(336, 362);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(100, 26);
            this.btnSave.TabIndex = 3;
            this.btnSave.Text = "Save and apply";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(442, 362);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(70, 26);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "Close";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // PetInheritanceForm
            // 
            this.AcceptButton = this.btnSave;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(524, 400);
            this.Controls.Add(this.lblHint);
            this.Controls.Add(this.grpTaoist);
            this.Controls.Add(this.grpWizard);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PetInheritanceForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Pet Inheritance";
            this.grpTaoist.ResumeLayout(false);
            this.grpTaoist.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudTaoistHealth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudTaoistMinMagicDefence)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudTaoistMaxMagicDefence)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudTaoistMinDefence)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudTaoistMaxDefence)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudTaoistMinDamage)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudTaoistMaxDamage)).EndInit();
            this.grpWizard.ResumeLayout(false);
            this.grpWizard.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudWizardHealth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudWizardMinMagicDefence)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudWizardMaxMagicDefence)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudWizardMinDefence)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudWizardMaxDefence)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudWizardMinDamage)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudWizardMaxDamage)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Label lblHint;
        private System.Windows.Forms.GroupBox grpTaoist;
        private System.Windows.Forms.CheckBox chkTaoistEnabled;
        private System.Windows.Forms.Label lblTaoistSource;
        private System.Windows.Forms.Label lblTaoistDamage;
        private System.Windows.Forms.NumericUpDown nudTaoistMinDamage;
        private System.Windows.Forms.NumericUpDown nudTaoistMaxDamage;
        private System.Windows.Forms.Label lblTaoistDefence;
        private System.Windows.Forms.NumericUpDown nudTaoistMinDefence;
        private System.Windows.Forms.NumericUpDown nudTaoistMaxDefence;
        private System.Windows.Forms.Label lblTaoistMagicDefence;
        private System.Windows.Forms.NumericUpDown nudTaoistMinMagicDefence;
        private System.Windows.Forms.NumericUpDown nudTaoistMaxMagicDefence;
        private System.Windows.Forms.Label lblTaoistHealth;
        private System.Windows.Forms.NumericUpDown nudTaoistHealth;
        private System.Windows.Forms.GroupBox grpWizard;
        private System.Windows.Forms.CheckBox chkWizardEnabled;
        private System.Windows.Forms.Label lblWizardSource;
        private System.Windows.Forms.Label lblWizardDamage;
        private System.Windows.Forms.NumericUpDown nudWizardMinDamage;
        private System.Windows.Forms.NumericUpDown nudWizardMaxDamage;
        private System.Windows.Forms.Label lblWizardDefence;
        private System.Windows.Forms.NumericUpDown nudWizardMinDefence;
        private System.Windows.Forms.NumericUpDown nudWizardMaxDefence;
        private System.Windows.Forms.Label lblWizardMagicDefence;
        private System.Windows.Forms.NumericUpDown nudWizardMinMagicDefence;
        private System.Windows.Forms.NumericUpDown nudWizardMaxMagicDefence;
        private System.Windows.Forms.Label lblWizardHealth;
        private System.Windows.Forms.NumericUpDown nudWizardHealth;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
    }
}
