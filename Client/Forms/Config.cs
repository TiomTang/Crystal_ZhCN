using Client;
using System.Resources;
using System.Reflection;
using Client.Resolution;

namespace Launcher
{

    public partial class Config : Form
    {
        // 填充下拉框时给 SelectedIndex 赋值会触发事件，先用标志跳过写回。
        private bool _syncingChoices;

        public Config()
        {
            InitializeComponent();
        }

        private void Config_Load(object sender, EventArgs e)
        {
            this.label10.Text = GameLanguage.Get("Resolution") ;
            this.AutoStart_label.Text = GameLanguage.Get("Auto start");
            this.ID_l.Text = GameLanguage.Get("Username");
            this.Password_l.Text = GameLanguage.Get("Password");

            FillResolutionChoices();
            FillDisplayModeChoices();
        }

        private void FillResolutionChoices()
        {
            _syncingChoices = true;

            Resolution_cb.Items.Clear();

            foreach (ResolutionOptions.Option option in ResolutionOptions.All)
                Resolution_cb.Items.Add(option.Label);

            Resolution_cb.SelectedIndex = ResolutionOptions.IndexOf(Settings.Resolution, Settings.ResolutionHeight);

            _syncingChoices = false;
        }

        private void FillDisplayModeChoices()
        {
            _syncingChoices = true;

            DisplayMode_cb.Items.Clear();
            DisplayMode_cb.Items.Add(GameLanguage.Get("Fullscreen"));
            DisplayMode_cb.Items.Add(GameLanguage.Get("Borderless window"));
            DisplayMode_cb.Items.Add(GameLanguage.Get("Windowed"));

            DisplayMode_cb.SelectedIndex = Settings.Mode switch
            {
                DisplayMode.FullScreen => 0,
                DisplayMode.Maximized => 1,
                _ => 2,
            };

            _syncingChoices = false;
        }

        private void Resolution_cb_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_syncingChoices || Resolution_cb.SelectedIndex < 0)
                return;

            ResolutionOptions.Option option = ResolutionOptions.All[Resolution_cb.SelectedIndex];

            Settings.Resolution = option.Width;
            Settings.ResolutionHeight = option.Height;
        }

        private void DisplayMode_cb_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_syncingChoices || DisplayMode_cb.SelectedIndex < 0)
                return;

            Settings.Mode = DisplayMode_cb.SelectedIndex switch
            {
                0 => DisplayMode.FullScreen,
                1 => DisplayMode.Maximized,
                _ => DisplayMode.Windowed,
            };
        }

        private void Config_VisibleChanged(object sender, EventArgs e)
        {
            if (Visible)
            {
                AccountLogin_txt.Text = Settings.AccountID;
                AccountPass_txt.Text = Settings.Password;

                FillResolutionChoices();
                FillDisplayModeChoices();

                FPScap_pb.Image = Settings.FPSCap
                    ? Client.Resources.Images.Config_Check_On
                    : Client.Resources.Images.Config_Check_Off1;

                OnTop_pb.Image = Settings.TopMost
                    ? Client.Resources.Images.Config_Check_On
                    : Client.Resources.Images.Config_Check_Off1;

                AutoStart_pb.Image = Settings.P_AutoStart
                    ? Client.Resources.Images.Config_Check_On
                    : Client.Resources.Images.Config_Check_Off1;

                this.ActiveControl = label4;
            }
            else
            {             
                Settings.AccountID = AccountLogin_txt.Text;
                Settings.Password = AccountPass_txt.Text;
                Settings.Save();
            }
        }

        private void AccountLogin_txt_TextChanged(object sender, EventArgs e)
        {
            if (AccountLogin_txt.Text == string.Empty) ID_l.Visible = true;
            else ID_l.Visible = false;
        }

        private void AccountPass_txt_TextChanged(object sender, EventArgs e)
        {
            if (AccountPass_txt.Text == string.Empty) Password_l.Visible = true;
            else Password_l.Visible = false;
        }

        private void AccountLogin_txt_Click(object sender, EventArgs e)
        {
            ID_l.Visible = false;
            AccountLogin_txt.Focus();
        }

        private void AccountPass_txt_Click(object sender, EventArgs e)
        {
            Password_l.Visible = false;
            AccountPass_txt.Focus();
        }

        private void Config_Click(object sender, EventArgs e)
        {
            this.ActiveControl = label4;
        }

        private void FPScap_pb_Click(object sender, EventArgs e)
        {
            Settings.FPSCap = !Settings.FPSCap;

            FPScap_pb.Image = Settings.FPSCap
                    ? Client.Resources.Images.Config_Check_On
                    : Client.Resources.Images.Config_Check_Off1;
        }

        private void OnTop_pb_Click(object sender, EventArgs e)
        {
            Settings.TopMost = !Settings.TopMost;

            OnTop_pb.Image = Settings.TopMost
                    ? Client.Resources.Images.Config_Check_On
                    : Client.Resources.Images.Config_Check_Off1;
        }

        private void AutoStart_pb_Click(object sender, EventArgs e)
        {
            Settings.P_AutoStart = !Settings.P_AutoStart;

            AutoStart_pb.Image = Settings.P_AutoStart
                    ? Client.Resources.Images.Config_Check_On
                    : Client.Resources.Images.Config_Check_Off1;
        }

        private void CleanFiles_pb_MouseDown(object sender, MouseEventArgs e)
        {
            CleanFiles_pb.Image = Client.Resources.Images.CheckF_Pressed;
        }

        private void CleanFiles_pb_MouseUp(object sender, MouseEventArgs e)
        {
            CleanFiles_pb.Image = Client.Resources.Images.CheckF_Base2;
        }

        private void CleanFiles_pb_MouseEnter(object sender, EventArgs e)
        {
            CleanFiles_pb.Image = Client.Resources.Images.CheckF_Hover;
        }

        private void CleanFiles_pb_MouseLeave(object sender, EventArgs e)
        {
            CleanFiles_pb.Image = Client.Resources.Images.CheckF_Base2;
        }

        private void CleanFiles_pb_Click(object sender, EventArgs e)
        {
            if (!Program.PForm.Launch_pb.Enabled) return;

            Program.PForm.Completed = false;
            Program.PForm.InterfaceTimer.Enabled = true;
            Program.PForm.CleanFiles = true;
            Program.PForm._workThread = new Thread(Program.PForm.Start) { IsBackground = true };
            Program.PForm._workThread.Start();
        }
    }
}
