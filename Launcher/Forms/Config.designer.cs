namespace Launcher
{
    partial class Config
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
            label4 = new Label();
            AutoStart_pb = new PictureBox();
            AutoStart_label = new Label();
            OnTop_label = new Label();
            OnTop_pb = new PictureBox();
            FPScap_label = new Label();
            FPScap_pb = new PictureBox();
            label9 = new Label();
            label10 = new Label();
            Resolution_cb = new ComboBox();
            DisplayMode_cb = new ComboBox();
            ((System.ComponentModel.ISupportInitialize)AutoStart_pb).BeginInit();
            ((System.ComponentModel.ISupportInitialize)OnTop_pb).BeginInit();
            ((System.ComponentModel.ISupportInitialize)FPScap_pb).BeginInit();
            SuspendLayout();
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.BackColor = Color.Transparent;
            label4.Location = new Point(261, 35);
            label4.Margin = new Padding(4, 0, 4, 0);
            label4.Name = "label4";
            label4.Size = new Size(0, 17);
            label4.TabIndex = 10;
            // 
            // AutoStart_pb
            // 
            AutoStart_pb.BackgroundImageLayout = ImageLayout.Center;
            AutoStart_pb.Location = new Point(148, 116);
            AutoStart_pb.Margin = new Padding(4, 3, 4, 3);
            AutoStart_pb.Name = "AutoStart_pb";
            AutoStart_pb.Size = new Size(12, 14);
            AutoStart_pb.TabIndex = 11;
            AutoStart_pb.TabStop = false;
            AutoStart_pb.Click += AutoStart_pb_Click;
            // 
            // AutoStart_label
            // 
            AutoStart_label.AutoSize = true;
            AutoStart_label.BackColor = Color.Transparent;
            AutoStart_label.Font = new Font("Calibri", 8.25F);
            AutoStart_label.ForeColor = Color.Gray;
            AutoStart_label.Location = new Point(165, 116);
            AutoStart_label.Margin = new Padding(4, 0, 4, 0);
            AutoStart_label.Name = "AutoStart_label";
            AutoStart_label.Size = new Size(54, 13);
            AutoStart_label.TabIndex = 12;
            AutoStart_label.Text = "Auto start";
            AutoStart_label.Click += AutoStart_pb_Click;
            // 
            // OnTop_label
            // 
            OnTop_label.AutoSize = true;
            OnTop_label.BackColor = Color.Transparent;
            OnTop_label.Font = new Font("Calibri", 8.25F);
            OnTop_label.ForeColor = Color.Gray;
            OnTop_label.Location = new Point(165, 91);
            OnTop_label.Margin = new Padding(4, 0, 4, 0);
            OnTop_label.Name = "OnTop_label";
            OnTop_label.Size = new Size(72, 13);
            OnTop_label.TabIndex = 16;
            OnTop_label.Text = "Always on top";
            OnTop_label.Click += OnTop_pb_Click;
            // 
            // OnTop_pb
            // 
            OnTop_pb.BackgroundImageLayout = ImageLayout.Center;
            OnTop_pb.Image = Launcher.Resources.Images.Config_Check_Off1;
            OnTop_pb.Location = new Point(148, 91);
            OnTop_pb.Margin = new Padding(4, 3, 4, 3);
            OnTop_pb.Name = "OnTop_pb";
            OnTop_pb.Size = new Size(12, 14);
            OnTop_pb.TabIndex = 15;
            OnTop_pb.TabStop = false;
            OnTop_pb.Click += OnTop_pb_Click;
            // 
            // FPScap_label
            // 
            FPScap_label.AutoSize = true;
            FPScap_label.BackColor = Color.Transparent;
            FPScap_label.Font = new Font("Calibri", 8.25F);
            FPScap_label.ForeColor = Color.Gray;
            FPScap_label.Location = new Point(165, 66);
            FPScap_label.Margin = new Padding(4, 0, 4, 0);
            FPScap_label.Name = "FPScap_label";
            FPScap_label.Size = new Size(42, 13);
            FPScap_label.TabIndex = 18;
            FPScap_label.Text = "FPS cap";
            FPScap_label.Click += FPScap_pb_Click;
            // 
            // FPScap_pb
            // 
            FPScap_pb.BackgroundImageLayout = ImageLayout.Center;
            FPScap_pb.Image = Launcher.Resources.Images.Config_Check_Off1;
            FPScap_pb.Location = new Point(148, 66);
            FPScap_pb.Margin = new Padding(4, 3, 4, 3);
            FPScap_pb.Name = "FPScap_pb";
            FPScap_pb.Size = new Size(12, 14);
            FPScap_pb.TabIndex = 17;
            FPScap_pb.TabStop = false;
            FPScap_pb.Click += FPScap_pb_Click;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.BackColor = Color.Transparent;
            label9.Font = new Font("Calibri", 9F);
            label9.ForeColor = Color.White;
            label9.Location = new Point(23, 48);
            label9.Margin = new Padding(4, 0, 4, 0);
            label9.Name = "label9";
            label9.Size = new Size(55, 14);
            label9.TabIndex = 21;
            label9.Text = "Graphics";
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.BackColor = Color.Transparent;
            label10.Font = new Font("Calibri", 9F);
            label10.ForeColor = Color.White;
            label10.Location = new Point(23, 101);
            label10.Margin = new Padding(4, 0, 4, 0);
            label10.Name = "label10";
            label10.Size = new Size(66, 14);
            label10.TabIndex = 22;
            label10.Text = "Resolution";
            // 
            // Resolution_cb
            // 
            Resolution_cb.DropDownStyle = ComboBoxStyle.DropDownList;
            Resolution_cb.Font = new Font("Calibri", 8.25F);
            Resolution_cb.FormattingEnabled = true;
            Resolution_cb.Location = new Point(23, 119);
            Resolution_cb.Name = "Resolution_cb";
            Resolution_cb.Size = new Size(112, 21);
            Resolution_cb.TabIndex = 30;
            Resolution_cb.SelectedIndexChanged += Resolution_cb_SelectedIndexChanged;
            // 
            // DisplayMode_cb
            // 
            DisplayMode_cb.DropDownStyle = ComboBoxStyle.DropDownList;
            DisplayMode_cb.Font = new Font("Calibri", 8.25F);
            DisplayMode_cb.FormattingEnabled = true;
            DisplayMode_cb.Location = new Point(23, 66);
            DisplayMode_cb.Name = "DisplayMode_cb";
            DisplayMode_cb.Size = new Size(114, 21);
            DisplayMode_cb.TabIndex = 31;
            DisplayMode_cb.SelectedIndexChanged += DisplayMode_cb_SelectedIndexChanged;
            // 
            // Config
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Black;
            BackgroundImage = Launcher.Resources.Images.Config_Base1;
            BackgroundImageLayout = ImageLayout.Center;
            ClientSize = new Size(257, 357);
            Controls.Add(Resolution_cb);
            Controls.Add(DisplayMode_cb);
            Controls.Add(label10);
            Controls.Add(label9);
            Controls.Add(FPScap_label);
            Controls.Add(FPScap_pb);
            Controls.Add(OnTop_label);
            Controls.Add(OnTop_pb);
            Controls.Add(AutoStart_label);
            Controls.Add(AutoStart_pb);
            Controls.Add(label4);
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.None;
            Margin = new Padding(4, 3, 4, 3);
            Name = "Config";
            Opacity = 0.94D;
            ShowIcon = false;
            ShowInTaskbar = false;
            SizeGripStyle = SizeGripStyle.Hide;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Config";
            TransparencyKey = Color.Black;
            Load += Config_Load;
            VisibleChanged += Config_VisibleChanged;
            Click += Config_Click;
            ((System.ComponentModel.ISupportInitialize)AutoStart_pb).EndInit();
            ((System.ComponentModel.ISupportInitialize)OnTop_pb).EndInit();
            ((System.ComponentModel.ISupportInitialize)FPScap_pb).EndInit();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.PictureBox AutoStart_pb;
        private System.Windows.Forms.Label AutoStart_label;
        private System.Windows.Forms.Label OnTop_label;
        private System.Windows.Forms.PictureBox OnTop_pb;
        private System.Windows.Forms.Label FPScap_label;
        private System.Windows.Forms.PictureBox FPScap_pb;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.ComboBox Resolution_cb;
        private System.Windows.Forms.ComboBox DisplayMode_cb;
    }
}