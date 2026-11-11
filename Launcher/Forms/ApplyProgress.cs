namespace Launcher;

// Progress window shown by the external update helper. A launcher update has to run from
// a copy outside the installation, so the main window closes first; without this window
// the backup/copy phase of a large map update looks like the launcher crashed.
internal static class ApplyProgress
{
    public static void Run(string root, int parentId)
    {
        LoadLanguage(root);
        using var form = new ApplyForm();
        Exception failure = null;
        var worker = new Thread(() =>
        {
            try { DeferredUpdate.Apply(root, parentId, restart: false, form.Report); }
            catch (Exception ex) { failure = ex; }
            finally { form.Finish(); }
        }) { IsBackground = true, Name = "apply-update" };
        form.Shown += (_, _) => worker.Start();
        Application.Run(form);
        if (failure != null) throw new Exception(failure.Message, failure);
        // A session shutdown also closes the form; never start a new launcher then.
        if (!form.Completed) return;
        // The window is gone before the new launcher opens, so the player never sees two.
        DeferredUpdate.Restart(root);
    }
    // The helper copy carries the published packs but not the operator/user INI.
    static void LoadLanguage(string root)
    {
        try
        {
            string folder = Path.GetFullPath(root);
            string language = new InIReader(Path.Combine(folder, "Launcher.User.ini")).ReadString("Game", "Language", "", false);
            if (language.Length == 0) language = new InIReader(Path.Combine(folder, "Launcher.ini")).ReadString("Game", "Language", "English", false);
            GameLanguage.LoadClientLanguage(Path.Combine(AppContext.BaseDirectory, "Localization", language + ".json"));
        }
        catch { }
    }
}

internal sealed class ApplyForm : Form
{
    private readonly Label status = new();
    private readonly Label file = new();
    private readonly ProgressBar bar = new();
    private readonly Label hint = new();
    private bool finished;

    public bool Completed => finished;

    public ApplyForm()
    {
        Text = "Crystal Launcher";
        Font = SystemFonts.MessageBoxFont;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterScreen;
        MaximizeBox = false; MinimizeBox = false; ControlBox = false;
        BackColor = Color.FromArgb(24, 24, 24);
        ForeColor = Color.Gainsboro;
        ClientSize = new Size(430, 120);

        status.SetBounds(16, 14, 398, 20);
        status.Font = new Font(Font, FontStyle.Bold);
        file.SetBounds(16, 40, 398, 18);
        file.AutoEllipsis = true;
        file.ForeColor = Color.Silver;
        bar.SetBounds(16, 64, 398, 18);
        bar.Maximum = 1000; bar.Style = ProgressBarStyle.Continuous;
        hint.SetBounds(16, 92, 398, 16);
        hint.ForeColor = Color.Gray;
        Controls.Add(status); Controls.Add(file); Controls.Add(bar); Controls.Add(hint);
        status.Text = GameLanguage.Get("Waiting for the launcher to close...");
        hint.Text = GameLanguage.Get("Do not close this window.");
    }

    public void Report(ApplyPhase phase, int done, int total, string name) => Post(() =>
    {
        status.Text = phase switch
        {
            ApplyPhase.Applying => GameLanguage.Get("Applying update files... ({0}/{1})", done, total),
            ApplyPhase.Verifying => GameLanguage.Get("Verifying update..."),
            ApplyPhase.Restarting => GameLanguage.Get("Restarting launcher..."),
            _ => GameLanguage.Get("Waiting for the launcher to close..."),
        };
        file.Text = name ?? "";
        int value = phase is ApplyPhase.Verifying or ApplyPhase.Restarting ? bar.Maximum
            : total <= 0 ? 0 : (int)Math.Clamp((long)done * bar.Maximum / total, 0, bar.Maximum);
        if (value > bar.Value) bar.Value = value;
    });

    public void Finish()
    {
        if (!IsHandleCreated || IsDisposed) return;
        try { BeginInvoke(new Action(() => { finished = true; Close(); })); }
        catch (ObjectDisposedException) { }
        catch (InvalidOperationException) { }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (!finished && e.CloseReason == CloseReason.UserClosing) e.Cancel = true;
        base.OnFormClosing(e);
    }

    private void Post(Action action)
    {
        if (finished || IsDisposed) return;
        try { BeginInvoke(action); }
        catch (ObjectDisposedException) { }
        catch (InvalidOperationException) { }
    }
}
