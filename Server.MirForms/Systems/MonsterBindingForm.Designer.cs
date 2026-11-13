namespace Server
{
    partial class MonsterBindingForm
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
            this.lvBindings = new System.Windows.Forms.ListView();
            this.colGroup = new System.Windows.Forms.ColumnHeader();
            this.colKey = new System.Windows.Forms.ColumnHeader();
            this.colUsedBy = new System.Windows.Forms.ColumnHeader();
            this.colValue = new System.Windows.Forms.ColumnHeader();
            this.colStatus = new System.Windows.Forms.ColumnHeader();
            this.chkOnlyUnresolved = new System.Windows.Forms.CheckBox();
            this.lblSummary = new System.Windows.Forms.Label();
            this.grpEdit = new System.Windows.Forms.GroupBox();
            this.lblMonster = new System.Windows.Forms.Label();
            this.cbMonster = new System.Windows.Forms.ComboBox();
            this.btnApply = new System.Windows.Forms.Button();
            this.lblSelection = new System.Windows.Forms.Label();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.grpEdit.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblHint
            // 
            this.lblHint.Location = new System.Drawing.Point(12, 10);
            this.lblHint.Name = "lblHint";
            this.lblHint.Size = new System.Drawing.Size(736, 34);
            this.lblHint.TabIndex = 0;
            this.lblHint.Text = "These names live in Configs/Setup.ini and must match a monster record. An entry that matches nothing makes its feature do nothing, and the server only checks them when EnforceDBChecks is enabled.";
            // 
            // lvBindings
            // 
            this.lvBindings.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colGroup,
            this.colKey,
            this.colUsedBy,
            this.colValue,
            this.colStatus});
            this.lvBindings.FullRowSelect = true;
            this.lvBindings.GridLines = true;
            this.lvBindings.HideSelection = false;
            this.lvBindings.Location = new System.Drawing.Point(12, 50);
            this.lvBindings.MultiSelect = false;
            this.lvBindings.Name = "lvBindings";
            this.lvBindings.Size = new System.Drawing.Size(736, 292);
            this.lvBindings.TabIndex = 1;
            this.lvBindings.UseCompatibleStateImageBehavior = false;
            this.lvBindings.View = System.Windows.Forms.View.Details;
            this.lvBindings.SelectedIndexChanged += new System.EventHandler(this.lvBindings_SelectedIndexChanged);
            // 
            // colGroup
            // 
            this.colGroup.Name = "colGroup";
            this.colGroup.Text = "Purpose";
            this.colGroup.Width = 130;
            // 
            // colKey
            // 
            this.colKey.Name = "colKey";
            this.colKey.Text = "Setting key";
            this.colKey.Width = 150;
            // 
            // colUsedBy
            // 
            this.colUsedBy.Name = "colUsedBy";
            this.colUsedBy.Text = "Used by";
            this.colUsedBy.Width = 120;
            // 
            // colValue
            // 
            this.colValue.Name = "colValue";
            this.colValue.Text = "Monster name";
            this.colValue.Width = 180;
            // 
            // colStatus
            // 
            this.colStatus.Name = "colStatus";
            this.colStatus.Text = "Status";
            this.colStatus.Width = 136;
            // 
            // chkOnlyUnresolved
            // 
            this.chkOnlyUnresolved.AutoSize = true;
            this.chkOnlyUnresolved.Location = new System.Drawing.Point(12, 352);
            this.chkOnlyUnresolved.Name = "chkOnlyUnresolved";
            this.chkOnlyUnresolved.Size = new System.Drawing.Size(194, 19);
            this.chkOnlyUnresolved.TabIndex = 2;
            this.chkOnlyUnresolved.Text = "Only show unresolved entries";
            this.chkOnlyUnresolved.UseVisualStyleBackColor = true;
            this.chkOnlyUnresolved.CheckedChanged += new System.EventHandler(this.chkOnlyUnresolved_CheckedChanged);
            // 
            // lblSummary
            // 
            this.lblSummary.Location = new System.Drawing.Point(588, 353);
            this.lblSummary.Name = "lblSummary";
            this.lblSummary.Size = new System.Drawing.Size(160, 15);
            this.lblSummary.TabIndex = 3;
            this.lblSummary.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // grpEdit
            // 
            this.grpEdit.Controls.Add(this.lblMonster);
            this.grpEdit.Controls.Add(this.cbMonster);
            this.grpEdit.Controls.Add(this.btnApply);
            this.grpEdit.Controls.Add(this.lblSelection);
            this.grpEdit.Location = new System.Drawing.Point(12, 378);
            this.grpEdit.Name = "grpEdit";
            this.grpEdit.Size = new System.Drawing.Size(736, 96);
            this.grpEdit.TabIndex = 4;
            this.grpEdit.TabStop = false;
            this.grpEdit.Text = "Change the monster";
            // 
            // lblSelection
            // 
            this.lblSelection.AutoSize = true;
            this.lblSelection.Location = new System.Drawing.Point(14, 22);
            this.lblSelection.Name = "lblSelection";
            this.lblSelection.Size = new System.Drawing.Size(289, 15);
            this.lblSelection.TabIndex = 0;
            // 
            // lblMonster
            // 
            this.lblMonster.AutoSize = true;
            this.lblMonster.Location = new System.Drawing.Point(14, 58);
            this.lblMonster.Name = "lblMonster";
            this.lblMonster.Size = new System.Drawing.Size(58, 15);
            this.lblMonster.TabIndex = 1;
            this.lblMonster.Text = "Monster:";
            // 
            // cbMonster
            // 
            this.cbMonster.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbMonster.Location = new System.Drawing.Point(85, 54);
            this.cbMonster.Name = "cbMonster";
            this.cbMonster.Size = new System.Drawing.Size(420, 23);
            this.cbMonster.TabIndex = 2;
            this.cbMonster.SelectedIndexChanged += new System.EventHandler(this.cbMonster_SelectedIndexChanged);
            // 
            // btnApply
            // 
            this.btnApply.Location = new System.Drawing.Point(516, 52);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(100, 27);
            this.btnApply.TabIndex = 3;
            this.btnApply.Text = "Set name";
            this.btnApply.UseVisualStyleBackColor = true;
            this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(558, 486);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(90, 28);
            this.btnSave.TabIndex = 5;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnClose
            // 
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Location = new System.Drawing.Point(658, 486);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(90, 28);
            this.btnClose.TabIndex = 6;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            // 
            // MonsterBindingForm
            // 
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(760, 526);
            this.Controls.Add(this.lblHint);
            this.Controls.Add(this.lvBindings);
            this.Controls.Add(this.chkOnlyUnresolved);
            this.Controls.Add(this.lblSummary);
            this.Controls.Add(this.grpEdit);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnClose);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MonsterBindingForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Monster Bindings";
            this.grpEdit.ResumeLayout(false);
            this.grpEdit.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblHint;
        private System.Windows.Forms.ListView lvBindings;
        private System.Windows.Forms.ColumnHeader colGroup;
        private System.Windows.Forms.ColumnHeader colKey;
        private System.Windows.Forms.ColumnHeader colUsedBy;
        private System.Windows.Forms.ColumnHeader colValue;
        private System.Windows.Forms.ColumnHeader colStatus;
        private System.Windows.Forms.CheckBox chkOnlyUnresolved;
        private System.Windows.Forms.Label lblSummary;
        private System.Windows.Forms.GroupBox grpEdit;
        private System.Windows.Forms.Label lblSelection;
        private System.Windows.Forms.Label lblMonster;
        private System.Windows.Forms.ComboBox cbMonster;
        private System.Windows.Forms.Button btnApply;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnClose;
    }
}
