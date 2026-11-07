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
            AccountLogin_txt = new TextBox();
            AccountPass_txt = new TextBox();
            ID_l = new Label();
            Password_l = new Label();
            label4 = new Label();
            AutoStart_pb = new PictureBox();
            AutoStart_label = new Label();
            OnTop_label = new Label();
            OnTop_pb = new PictureBox();
            FPScap_label = new Label();
            FPScap_pb = new PictureBox();
            CleanFiles_pb = new PictureBox();
            pictureBox6 = new PictureBox();
            label9 = new Label();
            label10 = new Label();
            label11 = new Label();
            label12 = new Label();
            Resolution_cb = new ComboBox();
            DisplayMode_cb = new ComboBox();
            ((System.ComponentModel.ISupportInitialize)AutoStart_pb).BeginInit();
            ((System.ComponentModel.ISupportInitialize)OnTop_pb).BeginInit();
            ((System.ComponentModel.ISupportInitialize)FPScap_pb).BeginInit();
            ((System.ComponentModel.ISupportInitialize)CleanFiles_pb).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox6).BeginInit();
            SuspendLayout();
            // 
            // AccountLogin_txt
            // 
            AccountLogin_txt.BackColor = Color.FromArgb(64, 64, 64);
            AccountLogin_txt.BorderStyle = BorderStyle.None;
            AccountLogin_txt.Cursor = Cursors.IBeam;
            AccountLogin_txt.Font = new Font("Calibri", 9F);
            AccountLogin_txt.ForeColor = Color.White;
            AccountLogin_txt.Location = new Point(32, 208);
            AccountLogin_txt.Margin = new Padding(4, 3, 4, 3);
            AccountLogin_txt.Name = "AccountLogin_txt";
            AccountLogin_txt.Size = new Size(124, 15);
            AccountLogin_txt.TabIndex = 0;
            AccountLogin_txt.Tag = "Testing";
            AccountLogin_txt.Click += AccountLogin_txt_Click;
            AccountLogin_txt.TextChanged += AccountLogin_txt_TextChanged;
            AccountLogin_txt.Leave += AccountLogin_txt_TextChanged;
            // 
            // AccountPass_txt
            // 
            AccountPass_txt.BackColor = Color.FromArgb(64, 64, 64);
            AccountPass_txt.BorderStyle = BorderStyle.None;
            AccountPass_txt.Font = new Font("Calibri", 9F);
            AccountPass_txt.ForeColor = Color.White;
            AccountPass_txt.Location = new Point(32, 239);
            AccountPass_txt.Margin = new Padding(4, 3, 4, 3);
            AccountPass_txt.Name = "AccountPass_txt";
            AccountPass_txt.PasswordChar = '*';
            AccountPass_txt.Size = new Size(124, 15);
            AccountPass_txt.TabIndex = 1;
            AccountPass_txt.Click += AccountPass_txt_Click;
            AccountPass_txt.TextChanged += AccountPass_txt_TextChanged;
            AccountPass_txt.Leave += AccountPass_txt_TextChanged;
            // 
            // ID_l
            // 
            ID_l.BackColor = Color.Transparent;
            ID_l.Cursor = Cursors.IBeam;
            ID_l.Enabled = false;
            ID_l.Font = new Font("Calibri", 9.75F, FontStyle.Italic);
            ID_l.ForeColor = Color.DarkGray;
            ID_l.Location = new Point(32, 207);
            ID_l.Margin = new Padding(4, 0, 4, 0);
            ID_l.Name = "ID_l";
            ID_l.Size = new Size(124, 19);
            ID_l.TabIndex = 8;
            ID_l.Text = "Username";
            ID_l.Click += AccountLogin_txt_Click;
            // 
            // Password_l
            // 
            Password_l.BackColor = Color.Transparent;
            Password_l.Cursor = Cursors.IBeam;
            Password_l.Enabled = false;
            Password_l.Font = new Font("Calibri Light", 9.75F, FontStyle.Italic);
            Password_l.ForeColor = Color.DarkGray;
            Password_l.Location = new Point(32, 239);
            Password_l.Margin = new Padding(4, 0, 4, 0);
            Password_l.Name = "Password_l";
            Password_l.Size = new Size(124, 19);
            Password_l.TabIndex = 9;
            Password_l.Text = "Password";
            Password_l.Click += AccountPass_txt_Click;
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
            OnTop_pb.Image = Client.Resources.Images.Config_Check_Off1;
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
            FPScap_pb.Image = Client.Resources.Images.Config_Check_Off1;
            FPScap_pb.Location = new Point(148, 66);
            FPScap_pb.Margin = new Padding(4, 3, 4, 3);
            FPScap_pb.Name = "FPScap_pb";
            FPScap_pb.Size = new Size(12, 14);
            FPScap_pb.TabIndex = 17;
            FPScap_pb.TabStop = false;
            FPScap_pb.Click += FPScap_pb_Click;
            // 
            // CleanFiles_pb
            // 
            CleanFiles_pb.BackgroundImageLayout = ImageLayout.Center;
            CleanFiles_pb.Image = Client.Resources.Images.CheckF_Base2;
            CleanFiles_pb.Location = new Point(26, 288);
            CleanFiles_pb.Margin = new Padding(4, 3, 4, 3);
            CleanFiles_pb.Name = "CleanFiles_pb";
            CleanFiles_pb.Size = new Size(67, 26);
            CleanFiles_pb.TabIndex = 19;
            CleanFiles_pb.TabStop = false;
            CleanFiles_pb.Click += CleanFiles_pb_Click;
            CleanFiles_pb.MouseDown += CleanFiles_pb_MouseDown;
            CleanFiles_pb.MouseEnter += CleanFiles_pb_MouseEnter;
            CleanFiles_pb.MouseLeave += CleanFiles_pb_MouseLeave;
            CleanFiles_pb.MouseUp += CleanFiles_pb_MouseUp;
            // 
            // pictureBox6
            // 
            pictureBox6.BackColor = Color.Transparent;
            pictureBox6.Image = Client.Resources.Images.textboxes;
            pictureBox6.Location = new Point(26, 200);
            pictureBox6.Margin = new Padding(4, 3, 4, 3);
            pictureBox6.Name = "pictureBox6";
            pictureBox6.Size = new Size(137, 65);
            pictureBox6.TabIndex = 20;
            pictureBox6.TabStop = false;
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
            // label11
            // 
            label11.AutoSize = true;
            label11.BackColor = Color.Transparent;
            label11.Font = new Font("Calibri", 9F);
            label11.ForeColor = Color.White;
            label11.Location = new Point(23, 157);
            label11.Margin = new Padding(4, 0, 4, 0);
            label11.Name = "label11";
            label11.Size = new Size(39, 14);
            label11.TabIndex = 23;
            label11.Text = "Game";
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.BackColor = Color.Transparent;
            label12.Font = new Font("Calibri", 8.25F);
            label12.ForeColor = Color.Gray;
            label12.Location = new Point(23, 182);
            label12.Margin = new Padding(4, 0, 4, 0);
            label12.Name = "label12";
            label12.Size = new Size(80, 13);
            label12.TabIndex = 24;
            label12.Text = "Account details";
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
            BackgroundImage = Client.Resources.Images.Config_Base1;
            BackgroundImageLayout = ImageLayout.Center;
            ClientSize = new Size(257, 357);
            Controls.Add(label12);
            Controls.Add(Resolution_cb);
            Controls.Add(DisplayMode_cb);
            Controls.Add(label11);
            Controls.Add(label10);
            Controls.Add(label9);
            Controls.Add(CleanFiles_pb);
            Controls.Add(FPScap_label);
            Controls.Add(FPScap_pb);
            Controls.Add(OnTop_label);
            Controls.Add(OnTop_pb);
            Controls.Add(AutoStart_label);
            Controls.Add(AutoStart_pb);
            Controls.Add(label4);
            Controls.Add(Password_l);
            Controls.Add(ID_l);
            Controls.Add(AccountPass_txt);
            Controls.Add(AccountLogin_txt);
            Controls.Add(pictureBox6);
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
            ((System.ComponentModel.ISupportInitialize)CleanFiles_pb).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox6).EndInit();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox AccountLogin_txt;
        private System.Windows.Forms.TextBox AccountPass_txt;
        private System.Windows.Forms.Label ID_l;
        private System.Windows.Forms.Label Password_l;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.PictureBox AutoStart_pb;
        private System.Windows.Forms.Label AutoStart_label;
        private System.Windows.Forms.Label OnTop_label;
        private System.Windows.Forms.PictureBox OnTop_pb;
        private System.Windows.Forms.Label FPScap_label;
        private System.Windows.Forms.PictureBox FPScap_pb;
        private System.Windows.Forms.PictureBox CleanFiles_pb;
        private System.Windows.Forms.PictureBox pictureBox6;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.ComboBox Resolution_cb;
        private System.Windows.Forms.ComboBox DisplayMode_cb;
    }
}