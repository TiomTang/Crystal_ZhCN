using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Windows.Forms;
using System.Drawing;

namespace Launcher
{
    partial class AMain
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AMain));
            ActionLabel = new System.Windows.Forms.Label();
            SpeedLabel = new System.Windows.Forms.Label();
            InterfaceTimer = new System.Windows.Forms.Timer(components);
            Movement_panel = new System.Windows.Forms.Panel();
            Name_label = new System.Windows.Forms.Label();
            pictureBox1 = new System.Windows.Forms.PictureBox();
            Close_pb = new System.Windows.Forms.PictureBox();
            Config_pb = new System.Windows.Forms.PictureBox();
            Version_label = new System.Windows.Forms.Label();
            CurrentFile_label = new System.Windows.Forms.Label();
            CurrentPercent_label = new System.Windows.Forms.Label();
            TotalPercent_label = new System.Windows.Forms.Label();
            Credit_label = new System.Windows.Forms.Label();
            ProgTotalEnd_pb = new System.Windows.Forms.PictureBox();
            ProgEnd_pb = new System.Windows.Forms.PictureBox();
            ProgressCurrent_pb = new System.Windows.Forms.PictureBox();
            TotalProg_pb = new System.Windows.Forms.PictureBox();
            Launch_pb = new System.Windows.Forms.PictureBox();
            Main_browser = new Microsoft.Web.WebView2.WinForms.WebView2();
            Movement_panel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)Close_pb).BeginInit();
            ((System.ComponentModel.ISupportInitialize)Config_pb).BeginInit();
            ((System.ComponentModel.ISupportInitialize)ProgTotalEnd_pb).BeginInit();
            ((System.ComponentModel.ISupportInitialize)ProgEnd_pb).BeginInit();
            ((System.ComponentModel.ISupportInitialize)ProgressCurrent_pb).BeginInit();
            ((System.ComponentModel.ISupportInitialize)TotalProg_pb).BeginInit();
            ((System.ComponentModel.ISupportInitialize)Launch_pb).BeginInit();
            ((System.ComponentModel.ISupportInitialize)Main_browser).BeginInit();
            SuspendLayout();
            // 
            // ActionLabel
            // 
            ActionLabel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            ActionLabel.BackColor = System.Drawing.Color.Transparent;
            ActionLabel.Font = new System.Drawing.Font("Calibri", 8.25F);
            ActionLabel.ForeColor = System.Drawing.Color.Gray;
            ActionLabel.Location = new System.Drawing.Point(478, 470);
            ActionLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            ActionLabel.Name = "ActionLabel";
            ActionLabel.Size = new System.Drawing.Size(126, 21);
            ActionLabel.TabIndex = 4;
            ActionLabel.Text = "1423MB/2000MB";
            ActionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            ActionLabel.Visible = false;
            ActionLabel.Click += ActionLabel_Click;
            // 
            // SpeedLabel
            // 
            SpeedLabel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            SpeedLabel.BackColor = System.Drawing.Color.Transparent;
            SpeedLabel.Font = new System.Drawing.Font("Calibri", 8.25F);
            SpeedLabel.ForeColor = System.Drawing.Color.Gray;
            SpeedLabel.Location = new System.Drawing.Point(341, 469);
            SpeedLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            SpeedLabel.Name = "SpeedLabel";
            SpeedLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
            SpeedLabel.Size = new System.Drawing.Size(83, 18);
            SpeedLabel.TabIndex = 13;
            SpeedLabel.Text = "Speed";
            SpeedLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            SpeedLabel.Visible = false;
            // 
            // InterfaceTimer
            // 
            InterfaceTimer.Enabled = true;
            InterfaceTimer.Interval = 50;
            InterfaceTimer.Tick += InterfaceTimer_Tick;
            // 
            // Movement_panel
            // 
            Movement_panel.BackColor = System.Drawing.Color.Transparent;
            Movement_panel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            Movement_panel.Controls.Add(Name_label);
            Movement_panel.Controls.Add(pictureBox1);
            Movement_panel.Controls.Add(Close_pb);
            Movement_panel.Controls.Add(Config_pb);
            Movement_panel.Location = new System.Drawing.Point(0, 0);
            Movement_panel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Movement_panel.Name = "Movement_panel";
            Movement_panel.Size = new System.Drawing.Size(800, 45);
            Movement_panel.TabIndex = 21;
            Movement_panel.MouseClick += Movement_panel_MouseClick;
            Movement_panel.MouseDown += Movement_panel_MouseClick;
            Movement_panel.MouseMove += Movement_panel_MouseMove;
            Movement_panel.MouseUp += Movement_panel_MouseUp;
            // 
            // Name_label
            // 
            Name_label.BackColor = System.Drawing.Color.Transparent;
            Name_label.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            Name_label.ForeColor = System.Drawing.Color.White;
            Name_label.Image = ((System.Drawing.Image)resources.GetObject("Name_label.Image"));
            Name_label.Location = new System.Drawing.Point(289, 11);
            Name_label.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            Name_label.Name = "Name_label";
            Name_label.Size = new System.Drawing.Size(217, 25);
            Name_label.TabIndex = 0;
            Name_label.Text = "Crystal Mir 2";
            Name_label.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            Name_label.Visible = false;
            // 
            // pictureBox1
            // 
            pictureBox1.Image = ((System.Drawing.Image)resources.GetObject("pictureBox1.Image"));
            pictureBox1.Location = new System.Drawing.Point(358, -46);
            pictureBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new System.Drawing.Size(217, 23);
            pictureBox1.TabIndex = 33;
            pictureBox1.TabStop = false;
            // 
            // Close_pb
            // 
            Close_pb.BackColor = System.Drawing.Color.Transparent;
            Close_pb.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            Close_pb.Image = ((System.Drawing.Image)resources.GetObject("Close_pb.Image"));
            Close_pb.Location = new System.Drawing.Point(768, 14);
            Close_pb.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Close_pb.Name = "Close_pb";
            Close_pb.Size = new System.Drawing.Size(19, 19);
            Close_pb.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            Close_pb.TabIndex = 20;
            Close_pb.TabStop = false;
            Close_pb.Click += Close_pb_Click;
            Close_pb.MouseDown += Close_pb_MouseDown;
            Close_pb.MouseEnter += Close_pb_MouseEnter;
            Close_pb.MouseLeave += Close_pb_MouseLeave;
            Close_pb.MouseUp += Close_pb_MouseUp;
            // 
            // Config_pb
            // 
            Config_pb.BackColor = System.Drawing.Color.Transparent;
            Config_pb.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            Config_pb.Image = ((System.Drawing.Image)resources.GetObject("Config_pb.Image"));
            Config_pb.Location = new System.Drawing.Point(744, 14);
            Config_pb.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Config_pb.Name = "Config_pb";
            Config_pb.Size = new System.Drawing.Size(19, 19);
            Config_pb.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            Config_pb.TabIndex = 32;
            Config_pb.TabStop = false;
            Config_pb.Click += Config_pb_Click;
            Config_pb.MouseDown += Config_pb_MouseDown;
            Config_pb.MouseEnter += Config_pb_MouseEnter;
            Config_pb.MouseLeave += Config_pb_MouseLeave;
            Config_pb.MouseUp += Config_pb_MouseUp;
            // 
            // Version_label
            // 
            Version_label.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            Version_label.BackColor = System.Drawing.Color.Transparent;
            Version_label.Font = new System.Drawing.Font("Calibri", 8.25F);
            Version_label.ForeColor = System.Drawing.Color.Gray;
            Version_label.Location = new System.Drawing.Point(648, 530);
            Version_label.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            Version_label.Name = "Version_label";
            Version_label.Size = new System.Drawing.Size(143, 15);
            Version_label.TabIndex = 31;
            Version_label.Text = "Version 1.0.0.0";
            Version_label.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // CurrentFile_label
            // 
            CurrentFile_label.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            CurrentFile_label.BackColor = System.Drawing.Color.Transparent;
            CurrentFile_label.Font = new System.Drawing.Font("Calibri", 8.25F);
            CurrentFile_label.ForeColor = System.Drawing.Color.Gray;
            CurrentFile_label.Location = new System.Drawing.Point(62, 466);
            CurrentFile_label.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            CurrentFile_label.Name = "CurrentFile_label";
            CurrentFile_label.Size = new System.Drawing.Size(422, 20);
            CurrentFile_label.TabIndex = 27;
            CurrentFile_label.Text = "Checking Files.";
            CurrentFile_label.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            CurrentFile_label.Visible = false;
            // 
            // CurrentPercent_label
            // 
            CurrentPercent_label.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            CurrentPercent_label.BackColor = System.Drawing.Color.Transparent;
            CurrentPercent_label.Font = new System.Drawing.Font("Calibri", 8.25F);
            CurrentPercent_label.ForeColor = System.Drawing.Color.Gray;
            CurrentPercent_label.Location = new System.Drawing.Point(619, 488);
            CurrentPercent_label.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            CurrentPercent_label.Name = "CurrentPercent_label";
            CurrentPercent_label.Size = new System.Drawing.Size(41, 23);
            CurrentPercent_label.TabIndex = 28;
            CurrentPercent_label.Text = "100%";
            CurrentPercent_label.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            CurrentPercent_label.Visible = false;
            // 
            // TotalPercent_label
            // 
            TotalPercent_label.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            TotalPercent_label.BackColor = System.Drawing.Color.Transparent;
            TotalPercent_label.Font = new System.Drawing.Font("Calibri", 8.25F);
            TotalPercent_label.ForeColor = System.Drawing.Color.Gray;
            TotalPercent_label.Location = new System.Drawing.Point(619, 509);
            TotalPercent_label.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            TotalPercent_label.Name = "TotalPercent_label";
            TotalPercent_label.Size = new System.Drawing.Size(41, 23);
            TotalPercent_label.TabIndex = 29;
            TotalPercent_label.Text = "100%";
            TotalPercent_label.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            TotalPercent_label.Visible = false;
            // 
            // Credit_label
            // 
            Credit_label.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            Credit_label.AutoSize = true;
            Credit_label.BackColor = System.Drawing.Color.Transparent;
            Credit_label.Font = new System.Drawing.Font("Calibri", 8.25F);
            Credit_label.ForeColor = System.Drawing.Color.Gray;
            Credit_label.Location = new System.Drawing.Point(13, 530);
            Credit_label.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            Credit_label.Name = "Credit_label";
            Credit_label.Size = new System.Drawing.Size(114, 13);
            Credit_label.TabIndex = 30;
            Credit_label.Text = "Powered by Crystal M2";
            Credit_label.Click += Credit_label_Click;
            // 
            // ProgTotalEnd_pb
            // 
            ProgTotalEnd_pb.Anchor = System.Windows.Forms.AnchorStyles.None;
            ProgTotalEnd_pb.BackColor = System.Drawing.Color.Transparent;
            ProgTotalEnd_pb.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            ProgTotalEnd_pb.Image = ((System.Drawing.Image)resources.GetObject("ProgTotalEnd_pb.Image"));
            ProgTotalEnd_pb.Location = new System.Drawing.Point(613, 507);
            ProgTotalEnd_pb.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ProgTotalEnd_pb.Name = "ProgTotalEnd_pb";
            ProgTotalEnd_pb.Size = new System.Drawing.Size(5, 17);
            ProgTotalEnd_pb.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            ProgTotalEnd_pb.TabIndex = 26;
            ProgTotalEnd_pb.TabStop = false;
            // 
            // ProgEnd_pb
            // 
            ProgEnd_pb.Anchor = System.Windows.Forms.AnchorStyles.None;
            ProgEnd_pb.BackColor = System.Drawing.Color.Transparent;
            ProgEnd_pb.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            ProgEnd_pb.Image = ((System.Drawing.Image)resources.GetObject("ProgEnd_pb.Image"));
            ProgEnd_pb.Location = new System.Drawing.Point(613, 490);
            ProgEnd_pb.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ProgEnd_pb.Name = "ProgEnd_pb";
            ProgEnd_pb.Size = new System.Drawing.Size(5, 17);
            ProgEnd_pb.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            ProgEnd_pb.TabIndex = 25;
            ProgEnd_pb.TabStop = false;
            // 
            // ProgressCurrent_pb
            // 
            ProgressCurrent_pb.Anchor = System.Windows.Forms.AnchorStyles.None;
            ProgressCurrent_pb.BackColor = System.Drawing.Color.Transparent;
            ProgressCurrent_pb.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            ProgressCurrent_pb.Image = ((System.Drawing.Image)resources.GetObject("ProgressCurrent_pb.Image"));
            ProgressCurrent_pb.Location = new System.Drawing.Point(60, 490);
            ProgressCurrent_pb.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ProgressCurrent_pb.Name = "ProgressCurrent_pb";
            ProgressCurrent_pb.Size = new System.Drawing.Size(557, 17);
            ProgressCurrent_pb.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            ProgressCurrent_pb.TabIndex = 23;
            ProgressCurrent_pb.TabStop = false;
            ProgressCurrent_pb.SizeChanged += ProgressCurrent_pb_SizeChanged;
            // 
            // TotalProg_pb
            // 
            TotalProg_pb.Anchor = System.Windows.Forms.AnchorStyles.None;
            TotalProg_pb.BackColor = System.Drawing.Color.Transparent;
            TotalProg_pb.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            TotalProg_pb.Image = ((System.Drawing.Image)resources.GetObject("TotalProg_pb.Image"));
            TotalProg_pb.Location = new System.Drawing.Point(60, 508);
            TotalProg_pb.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            TotalProg_pb.Name = "TotalProg_pb";
            TotalProg_pb.Size = new System.Drawing.Size(558, 16);
            TotalProg_pb.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            TotalProg_pb.TabIndex = 22;
            TotalProg_pb.TabStop = false;
            TotalProg_pb.SizeChanged += TotalProg_pb_SizeChanged;
            // 
            // Launch_pb
            // 
            Launch_pb.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            Launch_pb.BackColor = System.Drawing.Color.Transparent;
            Launch_pb.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            Launch_pb.Cursor = System.Windows.Forms.Cursors.Hand;
            Launch_pb.Image = ((System.Drawing.Image)resources.GetObject("Launch_pb.Image"));
            Launch_pb.Location = new System.Drawing.Point(658, 472);
            Launch_pb.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Launch_pb.Name = "Launch_pb";
            Launch_pb.Size = new System.Drawing.Size(115, 53);
            Launch_pb.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            Launch_pb.TabIndex = 19;
            Launch_pb.TabStop = false;
            Launch_pb.Click += Launch_pb_Click;
            Launch_pb.MouseDown += Launch_pb_MouseDown;
            Launch_pb.MouseEnter += Launch_pb_MouseEnter;
            Launch_pb.MouseLeave += Launch_pb_MouseLeave;
            Launch_pb.MouseUp += Launch_pb_MouseUp;
            // 
            // Main_browser
            // 
            Main_browser.AllowExternalDrop = true;
            Main_browser.CausesValidation = false;
            Main_browser.CreationProperties = null;
            Main_browser.DefaultBackgroundColor = System.Drawing.Color.White;
            Main_browser.Location = new System.Drawing.Point(11, 53);
            Main_browser.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Main_browser.MaximumSize = new System.Drawing.Size(782, 403);
            Main_browser.Name = "Main_browser";
            Main_browser.Size = new System.Drawing.Size(782, 403);
            Main_browser.TabIndex = 32;
            Main_browser.Visible = false;
            Main_browser.ZoomFactor = 1D;
            // 
            // AMain
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            BackColor = System.Drawing.SystemColors.ActiveCaption;
            BackgroundImage = ((System.Drawing.Image)resources.GetObject("$this.BackgroundImage"));
            BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            ClientSize = new System.Drawing.Size(800, 550);
            Controls.Add(Main_browser);
            Controls.Add(SpeedLabel);
            Controls.Add(Credit_label);
            Controls.Add(Version_label);
            Controls.Add(TotalPercent_label);
            Controls.Add(CurrentPercent_label);
            Controls.Add(CurrentFile_label);
            Controls.Add(ProgTotalEnd_pb);
            Controls.Add(ProgEnd_pb);
            Controls.Add(ProgressCurrent_pb);
            Controls.Add(TotalProg_pb);
            Controls.Add(Launch_pb);
            Controls.Add(ActionLabel);
            Controls.Add(Movement_panel);
            DoubleBuffered = true;
            ForeColor = System.Drawing.SystemColors.ControlText;
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            Icon = ((System.Drawing.Icon)resources.GetObject("$this.Icon"));
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Launcher";
            TransparencyKey = System.Drawing.Color.Black;
            FormClosed += AMain_FormClosed;
            Load += AMain_Load;
            Click += AMain_Click;
            Movement_panel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ((System.ComponentModel.ISupportInitialize)Close_pb).EndInit();
            ((System.ComponentModel.ISupportInitialize)Config_pb).EndInit();
            ((System.ComponentModel.ISupportInitialize)ProgTotalEnd_pb).EndInit();
            ((System.ComponentModel.ISupportInitialize)ProgEnd_pb).EndInit();
            ((System.ComponentModel.ISupportInitialize)ProgressCurrent_pb).EndInit();
            ((System.ComponentModel.ISupportInitialize)TotalProg_pb).EndInit();
            ((System.ComponentModel.ISupportInitialize)Launch_pb).EndInit();
            ((System.ComponentModel.ISupportInitialize)Main_browser).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private System.Windows.Forms.Label ActionLabel;
        private System.Windows.Forms.Label SpeedLabel;
        public System.Windows.Forms.Timer InterfaceTimer;
        private System.Windows.Forms.PictureBox Launch_pb;
        private System.Windows.Forms.PictureBox Close_pb;
        private System.Windows.Forms.Panel Movement_panel;
        private System.Windows.Forms.PictureBox TotalProg_pb;
        private System.Windows.Forms.PictureBox ProgressCurrent_pb;
        private System.Windows.Forms.Label Name_label;
        private System.Windows.Forms.PictureBox ProgEnd_pb;
        private System.Windows.Forms.PictureBox ProgTotalEnd_pb;
        private System.Windows.Forms.Label CurrentFile_label;
        private System.Windows.Forms.Label CurrentPercent_label;
        private System.Windows.Forms.Label TotalPercent_label;
        private System.Windows.Forms.Label Credit_label;
        private System.Windows.Forms.Label Version_label;
        private System.Windows.Forms.PictureBox Config_pb;
        private System.Windows.Forms.PictureBox pictureBox1;
        private Microsoft.Web.WebView2.WinForms.WebView2 Main_browser;
    }
}

