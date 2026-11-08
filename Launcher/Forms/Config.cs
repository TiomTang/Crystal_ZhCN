using Client.Bootstrap;
namespace Launcher;
public partial class Config : Form
{
    private bool syncing;
    private TextBox host;
    private NumericUpDown port;
    private CheckBox testConfig;
    public Config() { InitializeComponent(); }
    private void Config_Load(object sender, EventArgs e)
    {
        AMain.LocalizeControls(this);
        // Cover the old decorative account fields; credentials now exist only in the game.
        var server = new Panel { Location = new Point(16, 150), Size = new Size(225, 174), BackColor = Color.FromArgb(7, 10, 13) };
        server.Controls.Add(new Label { Text = GameLanguage.Get("Server"), ForeColor = Color.White, Location = new Point(7, 4), AutoSize = true });
        server.Controls.Add(new Label { Text = GameLanguage.Get("Host"), ForeColor = Color.Gray, Location = new Point(7, 28), AutoSize = true });
        host = new TextBox { ReadOnly = true, TabStop = false, Text = Settings.Host, Location = new Point(7, 47), Width = 200 };
        server.Controls.Add(host);
        server.Controls.Add(new Label { Text = GameLanguage.Get("Port"), ForeColor = Color.Gray, Location = new Point(7, 79), AutoSize = true });
        port = new NumericUpDown { Enabled = false, Minimum = 1, Maximum = 65535, Value = Settings.Port, Location = new Point(66, 77), Width = 141 };
        server.Controls.Add(port);
        testConfig = new CheckBox { Text = GameLanguage.Get("Test configuration (-tc)"), Checked = Settings.UseTestConfig, ForeColor = Color.Gray, Location = new Point(7, 111), AutoSize = true };
        server.Controls.Add(testConfig);
        Controls.Add(server); server.BringToFront();
        Resolution_cb.Items.AddRange(ResolutionOptions.All.Select(x => (object)x.Label).ToArray());
        DisplayMode_cb.Items.AddRange(new object[] { GameLanguage.Get("Fullscreen"), GameLanguage.Get("Borderless window"), GameLanguage.Get("Windowed") });
    }
    private void Config_VisibleChanged(object sender, EventArgs e)
    {
        if (!Visible)
        {
            if (host != null)
            {
                Settings.UseTestConfig = testConfig.Checked;
            }
            Settings.Save(); return;
        }
        if (host != null) { host.Text = Settings.Host; port.Value = Settings.Port; }
        syncing = true;
        Resolution_cb.SelectedIndex = ResolutionOptions.IndexOf(Settings.Resolution, Settings.ResolutionHeight);
        DisplayMode_cb.SelectedIndex = Settings.Mode == DisplayMode.FullScreen ? 0 : Settings.Mode == DisplayMode.Maximized ? 1 : 2;
        syncing = false; RefreshChecks();
    }
    private void RefreshChecks()
    {
        FPScap_pb.Image = Settings.FPSCap ? Resources.Images.Config_Check_On : Resources.Images.Config_Check_Off1;
        OnTop_pb.Image = Settings.TopMost ? Resources.Images.Config_Check_On : Resources.Images.Config_Check_Off1;
        AutoStart_pb.Image = Settings.P_AutoStart ? Resources.Images.Config_Check_On : Resources.Images.Config_Check_Off1;
    }
    private void Resolution_cb_SelectedIndexChanged(object sender, EventArgs e) { if (syncing || Resolution_cb.SelectedIndex < 0) return; var x = ResolutionOptions.All[Resolution_cb.SelectedIndex]; Settings.Resolution = x.Width; Settings.ResolutionHeight = x.Height; }
    private void DisplayMode_cb_SelectedIndexChanged(object sender, EventArgs e) { if (!syncing) Settings.Mode = DisplayMode_cb.SelectedIndex == 0 ? DisplayMode.FullScreen : DisplayMode_cb.SelectedIndex == 1 ? DisplayMode.Maximized : DisplayMode.Windowed; }
    private void FPScap_pb_Click(object sender, EventArgs e) { Settings.FPSCap = !Settings.FPSCap; RefreshChecks(); }
    private void OnTop_pb_Click(object sender, EventArgs e) { Settings.TopMost = !Settings.TopMost; RefreshChecks(); }
    private void AutoStart_pb_Click(object sender, EventArgs e) { Settings.P_AutoStart = !Settings.P_AutoStart; RefreshChecks(); }
    private void Config_Click(object sender, EventArgs e) { ActiveControl = label4; }
}
