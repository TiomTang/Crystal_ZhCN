namespace Server
{
    partial class CommandListForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.lblPermission = new System.Windows.Forms.Label();
            this.cbPermission = new System.Windows.Forms.ComboBox();
            this.lblCount = new System.Windows.Forms.Label();
            this.lvCommands = new System.Windows.Forms.ListView();
            this.colCommand = new System.Windows.Forms.ColumnHeader();
            this.colPermission = new System.Windows.Forms.ColumnHeader();
            this.colUsage = new System.Windows.Forms.ColumnHeader();
            this.colDescription = new System.Windows.Forms.ColumnHeader();
            this.colCategory = new System.Windows.Forms.ColumnHeader();
            this.lblHint = new System.Windows.Forms.Label();
            this.btnCopy = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // txtSearch
            // 
            this.txtSearch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSearch.Location = new System.Drawing.Point(12, 12);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.PlaceholderText = "Search a command or its description";
            this.txtSearch.Size = new System.Drawing.Size(420, 23);
            this.txtSearch.TabIndex = 0;
            this.txtSearch.TextChanged += new System.EventHandler(this.Filter_Changed);
            // 
            // lblPermission
            // 
            this.lblPermission.AutoSize = true;
            this.lblPermission.Location = new System.Drawing.Point(444, 16);
            this.lblPermission.Name = "lblPermission";
            this.lblPermission.Size = new System.Drawing.Size(66, 15);
            this.lblPermission.TabIndex = 1;
            this.lblPermission.Text = "Permission:";
            // 
            // cbPermission
            // 
            this.cbPermission.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cbPermission.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbPermission.Location = new System.Drawing.Point(516, 12);
            this.cbPermission.Name = "cbPermission";
            this.cbPermission.Size = new System.Drawing.Size(276, 23);
            this.cbPermission.TabIndex = 2;
            this.cbPermission.SelectedIndexChanged += new System.EventHandler(this.Filter_Changed);
            // 
            // lblCount
            // 
            this.lblCount.AutoSize = true;
            this.lblCount.Location = new System.Drawing.Point(12, 44);
            this.lblCount.Name = "lblCount";
            this.lblCount.Size = new System.Drawing.Size(0, 15);
            this.lblCount.TabIndex = 3;
            // 
            // lvCommands
            // 
            this.lvCommands.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lvCommands.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colCommand,
            this.colPermission,
            this.colUsage,
            this.colDescription,
            this.colCategory});
            this.lvCommands.FullRowSelect = true;
            this.lvCommands.GridLines = true;
            this.lvCommands.HideSelection = false;
            this.lvCommands.Location = new System.Drawing.Point(12, 66);
            this.lvCommands.MultiSelect = false;
            this.lvCommands.Name = "lvCommands";
            this.lvCommands.Size = new System.Drawing.Size(780, 402);
            this.lvCommands.TabIndex = 4;
            this.lvCommands.UseCompatibleStateImageBehavior = false;
            this.lvCommands.View = System.Windows.Forms.View.Details;
            this.lvCommands.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.lvCommands_ColumnClick);
            this.lvCommands.DoubleClick += new System.EventHandler(this.lvCommands_DoubleClick);
            // 
            // colCommand
            // 
            this.colCommand.Text = "Command";
            this.colCommand.Width = 170;
            // 
            // colPermission
            // 
            this.colPermission.Text = "Permission";
            this.colPermission.Width = 70;
            // 
            // colUsage
            // 
            this.colUsage.Text = "Usage";
            this.colUsage.Width = 190;
            // 
            // colDescription
            // 
            this.colDescription.Text = "Description";
            this.colDescription.Width = 270;
            // 
            // colCategory
            // 
            this.colCategory.Text = "Category";
            this.colCategory.Width = 80;
            // 
            // lblHint
            // 
            this.lblHint.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblHint.AutoSize = true;
            this.lblHint.ForeColor = System.Drawing.SystemColors.GrayText;
            this.lblHint.Location = new System.Drawing.Point(12, 478);
            this.lblHint.Name = "lblHint";
            this.lblHint.Size = new System.Drawing.Size(0, 15);
            this.lblHint.TabIndex = 5;
            this.lblHint.Text = "Double-click a command to copy it.";
            // 
            // btnCopy
            // 
            this.btnCopy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCopy.Location = new System.Drawing.Point(620, 474);
            this.btnCopy.Name = "btnCopy";
            this.btnCopy.Size = new System.Drawing.Size(80, 26);
            this.btnCopy.TabIndex = 6;
            this.btnCopy.Text = "Copy";
            this.btnCopy.UseVisualStyleBackColor = true;
            this.btnCopy.Click += new System.EventHandler(this.btnCopy_Click);
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.Location = new System.Drawing.Point(708, 474);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(80, 26);
            this.btnClose.TabIndex = 7;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // CommandListForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(804, 512);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnCopy);
            this.Controls.Add(this.lblHint);
            this.Controls.Add(this.lvCommands);
            this.Controls.Add(this.lblCount);
            this.Controls.Add(this.cbPermission);
            this.Controls.Add(this.lblPermission);
            this.Controls.Add(this.txtSearch);
            this.MinimumSize = new System.Drawing.Size(720, 420);
            this.Name = "CommandListForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Commands";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Label lblPermission;
        private System.Windows.Forms.ComboBox cbPermission;
        private System.Windows.Forms.Label lblCount;
        private System.Windows.Forms.ListView lvCommands;
        private System.Windows.Forms.ColumnHeader colCommand;
        private System.Windows.Forms.ColumnHeader colPermission;
        private System.Windows.Forms.ColumnHeader colUsage;
        private System.Windows.Forms.ColumnHeader colDescription;
        private System.Windows.Forms.ColumnHeader colCategory;
        private System.Windows.Forms.Label lblHint;
        private System.Windows.Forms.Button btnCopy;
        private System.Windows.Forms.Button btnClose;
    }
}
