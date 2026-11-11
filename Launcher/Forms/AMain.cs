using System.Diagnostics;
using System.Net;
using Client.Bootstrap;
using Microsoft.Web.WebView2.Core;
using System.Net.Http.Headers;


namespace Launcher
{
    public partial class AMain : Form
    {
        private enum UpdatePhase { Checking, Downloading, Applying }

        private int _fileCount;
        private volatile string _currentFileName = "";
        private volatile UpdatePhase _phase = UpdatePhase.Checking;
        // Written by the download thread, read by the UI timer; every counter is
        // published with Volatile so the bars cannot tear on 32-bit reads.
        private long _fileDone, _fileTotal, _doneBytes, _totalBytes;
        private int _filesDone, _filesTotal, _applyDone, _applyTotal;

        public volatile bool Completed, Checked, CleanFiles, LabelSwitch, ErrorFound;

        public List<FileInformation> OldList;


        public Thread _workThread;

        private bool dragging = false;
        private Point dragCursorPoint;
        private Point dragFormPoint;

        private Config ConfigForm = new Config();
        private PatchSource updateSource;
        private CancellationTokenSource cancellation = new();
        private bool forceRepair;
        private volatile string updateStatus = "Checking files...";
        private readonly Button repairButton = new() { Name = "RepairButton", Size = new Size(105, 21), Location = new Point(200, 524) };
        private readonly Button cancelButton = new() { Name = "CancelButton", Size = new Size(135, 21), Location = new Point(310, 524) };


        private int SkinPixels(int value) => (int)Math.Round(value * ClientSize.Width / 800f);
        // The informational version appends "+<commit>"; the label holds one short line.
        private static string BuildVersion()
        {
            string version = Application.ProductVersion;
            int commit = version.IndexOf('+');
            return commit < 0 ? version : version[..commit];
        }
        private int BarWidth(long done, long total) => total <= 0 ? SkinPixels(5) : Math.Max(SkinPixels(5), (int)((long)SkinPixels(550) * Math.Clamp(done, 0, total) / total));

        public AMain()
        {
            InitializeComponent();

            FormClosing += (_, e) => { if (_workThread != null && _workThread.IsAlive && !Completed) e.Cancel = true; };
            LocalizeControls(this);
            foreach (var button in new[] { repairButton, cancelButton })
            {
                button.BackColor = Color.FromArgb(48, 48, 48); button.ForeColor = Color.Gainsboro;
                button.UseVisualStyleBackColor = false; button.FlatStyle = FlatStyle.Flat;
                // Inherited form font is too tall for these 21px buttons and clipped the captions.
                button.Font = new Font("Calibri", 8.25F);
                button.TextAlign = ContentAlignment.MiddleCenter;
            }
            repairButton.Text = GameLanguage.Get("Repair files");
            cancelButton.Text = GameLanguage.Get("Cancel download");
            // Bottom strip follows the skin: the file field ends near x=422 and the
            // version line sits below both bars, so both labels must stay inside them.
            CurrentFile_label.Size = new Size(360, 20);
            ActionLabel.Location = new Point(424, 470); ActionLabel.Size = new Size(180, 21);
            Version_label.Size = new Size(200, 15);
            Version_label.Location = new Point(ClientSize.Width - Version_label.Width - 9, Version_label.Location.Y);
            Controls.Add(repairButton); Controls.Add(cancelButton); repairButton.BringToFront(); cancelButton.BringToFront();
            cancelButton.Click += (_, _) => cancellation.Cancel();
            repairButton.Click += (_, _) =>
            {
                if (_workThread?.IsAlive == true || DeferredUpdate.Pending(Settings.P_Client)) return;
                if (MessageBox.Show(GameLanguage.Get("Download ALL managed files again? This uses bandwidth. Personal settings and unknown files will not be deleted."), GameLanguage.Get("Repair files"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
                forceRepair = true; Completed = Checked = ErrorFound = false; ResetProgress();
                cancellation.Dispose(); cancellation = new(); Launch_pb.Enabled = false;
                updateSource = Settings.Snapshot(); InterfaceTimer.Enabled = true;
                _workThread = new Thread(Start) { IsBackground = true }; _workThread.Start();
            };
            BackColor = Color.FromArgb(1, 0, 0);
            TransparencyKey = Color.FromArgb(1, 0, 0);
            CurrentFile_label.AutoEllipsis = true;
        }

        public static void LocalizeControls(Control root)
        {
            foreach (Control control in root.Controls) { if (!string.IsNullOrWhiteSpace(control.Text)) control.Text = GameLanguage.Get(control.Text); LocalizeControls(control); }
        }
        public static void SaveError(string error) { try { File.AppendAllText("Launcher.log", error + Environment.NewLine); } catch { } }
        public void Start()
        {
            try
            {
                OldList = string.IsNullOrEmpty(updateSource.Host) ? new List<FileInformation>() : PatchEngine.ReadList(updateSource);
                var plan = PatchEngine.Check(Settings.P_Client, OldList, forceRepair);
                _fileCount = plan.Downloads.Count;
                Program.Trace("check-complete downloads=" + _fileCount);
                if (plan.Downloads.Count != 0)
                {
                    _phase = UpdatePhase.Downloading;
                    UpdateTransaction.Prepare(updateSource, Settings.P_Client, plan.Downloads, cancellation.Token, OnTransferProgress);
                    if (!DeferredUpdate.RequiresRestart(plan.Downloads))
                    {
                        _phase = UpdatePhase.Applying;
                        updateStatus = "Applying and verifying resource update...";
                        Program.Trace("resource-apply");
                        UpdateTransaction.ApplyConfirmed(Settings.P_Client, token => DeferredUpdate.Probe(Settings.P_Client, token), OnApplyProgress);
                        Program.Trace("resource-committed");
                    }
                }
                Checked = true;
            }
            catch (OperationCanceledException) { ErrorFound = true; updateStatus = "Download cancelled; installed files unchanged."; }
            catch (Exception ex) { ErrorFound = true; updateStatus = ex.Message; SaveError(ex.GetType().Name + ": Update failed; installed files unchanged or recovery required."); }
            finally { Completed = true; }
        }

        private void ResetProgress()
        {
            _currentFileName = ""; _phase = UpdatePhase.Checking;
            Volatile.Write(ref _fileDone, 0); Volatile.Write(ref _fileTotal, 0);
            Volatile.Write(ref _doneBytes, 0); Volatile.Write(ref _totalBytes, 0);
            Volatile.Write(ref _filesDone, 0); Volatile.Write(ref _filesTotal, 0);
            Volatile.Write(ref _applyDone, 0); Volatile.Write(ref _applyTotal, 0);
        }

        private void OnTransferProgress(TransferProgress p)
        {
            _currentFileName = p.Name;
            Volatile.Write(ref _fileDone, p.FileDone);
            Volatile.Write(ref _fileTotal, p.FileTotal);
            Volatile.Write(ref _doneBytes, p.Done);
            Volatile.Write(ref _totalBytes, p.Total);
            Volatile.Write(ref _filesDone, p.FilesDone);
            Volatile.Write(ref _filesTotal, p.FilesTotal);
        }

        private void OnApplyProgress(int done, int total, string name)
        {
            Volatile.Write(ref _applyDone, done);
            Volatile.Write(ref _applyTotal, total);
            _currentFileName = name ?? "";
        }

        private async void AMain_Load(object sender, EventArgs e)
        {


            Program.Trace("ui-load dpi=" + DeviceDpi + " client=" + ClientSize);
            repairButton.BringToFront(); cancelButton.BringToFront();
            Launch_pb.Enabled = false;
            ProgressCurrent_pb.Width = SkinPixels(5);
            TotalProg_pb.Width = SkinPixels(5);
            Version_label.Text = string.Format("Build: Crystal {0} {1}", Settings.UseTestConfig ? "Debug" : "Release", BuildVersion());

            if (Settings.P_ServerName != String.Empty)
            {
                Name_label.Visible = true;
                Name_label.Text = Settings.P_ServerName;
            }
            updateSource = Settings.Snapshot();
            _workThread = new Thread(Start) { IsBackground = true };
            _workThread.Start();

            if (Settings.P_BrowserAddress != "")
            {
                if (Uri.IsWellFormedUriString(Settings.P_BrowserAddress, UriKind.Absolute))
                {
                    Main_browser.NavigationCompleted += Main_browser_NavigationCompleted;
                    try
                    {
                        var envir = await CoreWebView2Environment.CreateAsync(null, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CrystalLauncher", "WebView"));
                        await Main_browser.EnsureCoreWebView2Async(envir);
                        if (!IsDisposed) Main_browser.Source = new Uri(Settings.P_BrowserAddress);
                    }
                    catch (Exception ex) { SaveError(ex.Message); }
                }
                else
                {
                    MessageBox.Show(GameLanguage.Get("Please Check Launcher BROWSER Setting is formatted correctly.\nCan be caused by missing or extra slashes and spelling mistakes.\nThis error can be ignored."), GameLanguage.Get("Bad BROWSER Format"));
                }
            }






        }

        private void Main_browser_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (Main_browser.Source.AbsolutePath != "blank") Main_browser.Visible = true;
        }

        private void Launch_pb_Click(object sender, EventArgs e)
        {
            Launch();
        }

        private void Launch()
        {
            if (!Completed || ErrorFound) return;
            if (ConfigForm.Visible) ConfigForm.Visible = false;
            Settings.Save();
            Program.Launch = true;
            Close();
        }

        private void Close_pb_Click(object sender, EventArgs e)
        {
            if (ConfigForm.Visible) ConfigForm.Visible = false;
            Close();
        }

        private void Movement_panel_MouseClick(object sender, MouseEventArgs e)
        {
            if (ConfigForm.Visible) ConfigForm.Visible = false;
            dragging = true;
            dragCursorPoint = Cursor.Position;
            dragFormPoint = this.Location;
        }

        private void Movement_panel_MouseUp(object sender, MouseEventArgs e)
        {
            dragging = false;
        }

        private void Movement_panel_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragging)
            {
                Point dif = Point.Subtract(Cursor.Position, new Size(dragCursorPoint));
                this.Location = Point.Add(dragFormPoint, new Size(dif));
            }
        }

        private void Launch_pb_MouseEnter(object sender, EventArgs e)
        {
            Launch_pb.Image = Launcher.Resources.Images.Launch_Hover;
        }

        private void Launch_pb_MouseLeave(object sender, EventArgs e)
        {
            Launch_pb.Image = Launcher.Resources.Images.Launch_Base1;
        }

        private void Close_pb_MouseEnter(object sender, EventArgs e)
        {
            Close_pb.Image = Launcher.Resources.Images.Cross_Hover;
        }

        private void Close_pb_MouseLeave(object sender, EventArgs e)
        {
            Close_pb.Image = Launcher.Resources.Images.Cross_Base;
        }

        private void Launch_pb_MouseDown(object sender, MouseEventArgs e)
        {
            Launch_pb.Image = Launcher.Resources.Images.Launch_Pressed;
        }

        private void Launch_pb_MouseUp(object sender, MouseEventArgs e)
        {
            Launch_pb.Image = Launcher.Resources.Images.Launch_Base1;
        }

        private void Close_pb_MouseDown(object sender, MouseEventArgs e)
        {
            Close_pb.Image = Launcher.Resources.Images.Cross_Pressed;
        }

        private void Close_pb_MouseUp(object sender, MouseEventArgs e)
        {
            Close_pb.Image = Launcher.Resources.Images.Cross_Base;
        }

        private void ProgressCurrent_pb_SizeChanged(object sender, EventArgs e)
        {
            ProgEnd_pb.Location = new Point((ProgressCurrent_pb.Location.X + ProgressCurrent_pb.Width), ProgressCurrent_pb.Location.Y);
            if (ProgressCurrent_pb.Width == 0) ProgEnd_pb.Visible = false;
            else ProgEnd_pb.Visible = true;
        }

        private void Config_pb_MouseDown(object sender, MouseEventArgs e)
        {
            Config_pb.Image = Launcher.Resources.Images.Config_Pressed;
        }

        private void Config_pb_MouseEnter(object sender, EventArgs e)
        {
            Config_pb.Image = Launcher.Resources.Images.Config_Hover;
        }

        private void Config_pb_MouseLeave(object sender, EventArgs e)
        {
            Config_pb.Image = Launcher.Resources.Images.Config_Base;
        }

        private void Config_pb_MouseUp(object sender, MouseEventArgs e)
        {
            Config_pb.Image = Launcher.Resources.Images.Config_Base;
        }

        private void Config_pb_Click(object sender, EventArgs e)
        {
            if (ConfigForm.Visible) ConfigForm.Hide();
            else ConfigForm.Show(Program.PForm);
            ConfigForm.Location = new Point(Location.X + Config_pb.Location.X - 183, Location.Y + 36);
        }

        private void TotalProg_pb_SizeChanged(object sender, EventArgs e)
        {
            ProgTotalEnd_pb.Location = new Point((TotalProg_pb.Location.X + TotalProg_pb.Width), TotalProg_pb.Location.Y);
            if (TotalProg_pb.Width == 0) ProgTotalEnd_pb.Visible = false;
            else ProgTotalEnd_pb.Visible = true;
        }

        private void InterfaceTimer_Tick(object sender, EventArgs e)
        {
            if (!Completed)
            {
                repairButton.Enabled = false;
                cancelButton.Enabled = _phase == UpdatePhase.Downloading && !cancellation.IsCancellationRequested;
                ActionLabel.Visible = true; CurrentFile_label.Visible = true;
                switch (_phase)
                {
                    case UpdatePhase.Downloading:
                        long filesDone = Volatile.Read(ref _filesDone), filesTotal = Volatile.Read(ref _filesTotal);
                        long doneBytes = Volatile.Read(ref _doneBytes), totalBytes = Volatile.Read(ref _totalBytes);
                        CurrentFile_label.Text = _currentFileName.Length == 0
                            ? GameLanguage.Get("{0} Files Remaining", Math.Max(0, filesTotal - filesDone))
                            : _currentFileName;
                        ActionLabel.Text = Math.Min(filesDone + 1, filesTotal) + "/" + filesTotal + "  " + ToSize(doneBytes) + "/" + ToSize(totalBytes);
                        TotalProg_pb.Width = BarWidth(doneBytes, totalBytes);
                        ProgressCurrent_pb.Width = BarWidth(Volatile.Read(ref _fileDone), Volatile.Read(ref _fileTotal));
                        break;
                    case UpdatePhase.Applying:
                        int applyDone = Volatile.Read(ref _applyDone), applyTotal = Volatile.Read(ref _applyTotal);
                        CurrentFile_label.Text = _currentFileName.Length == 0 ? updateStatus : _currentFileName;
                        ActionLabel.Text = GameLanguage.Get("Applying update {0}/{1}", applyDone, applyTotal);
                        TotalProg_pb.Width = SkinPixels(550);
                        ProgressCurrent_pb.Width = BarWidth(applyDone, applyTotal);
                        break;
                    default:
                        CurrentFile_label.Text = updateStatus; ActionLabel.Text = "";
                        TotalProg_pb.Width = ProgressCurrent_pb.Width = SkinPixels(5);
                        break;
                }
                return;
            }
            InterfaceTimer.Enabled = false;
            repairButton.Enabled = true; cancelButton.Enabled = false;
            if (!ErrorFound && DeferredUpdate.Pending(Settings.P_Client))
            {
                CurrentFile_label.Visible = true;
                CurrentFile_label.Text = GameLanguage.Get("Applying update... the launcher will restart automatically.");
                Program.Trace("self-update-restart-requested");
                CurrentFile_label.Refresh();
                // Keep the reason visible before the one required self-update restart.
                var restartTimer = new System.Windows.Forms.Timer { Interval = 900 };
                repairButton.Enabled = cancelButton.Enabled = false;
                restartTimer.Tick += (_, _) => { restartTimer.Stop(); restartTimer.Dispose(); Program.Launch = false; Close(); };
                restartTimer.Start(); return;
            }
            if (!ErrorFound)
            {
                try { Settings.ReloadPublished(); }
                catch (Exception ex)
                {
                    ErrorFound = true; Checked = false; Launch_pb.Enabled = false;
                    CurrentFile_label.Visible = true;
                    CurrentFile_label.Text = "Invalid launcher configuration. See Launcher.log.";
                    SaveError("Published launcher configuration: " + ex);
                    return;
                }
            }
            Program.Trace("ui-ready error=" + ErrorFound);
            Launch_pb.Enabled = !ErrorFound;
            CurrentFile_label.Visible = true;
            CurrentFile_label.Text = ErrorFound ? updateStatus : GameLanguage.Get("Up to date.");
            if (ErrorFound) { MessageBox.Show(GameLanguage.Get("One or more files failed to download, check Error.txt for details.").Replace("Error.txt", "Launcher.log")); return; }
            ProgressCurrent_pb.Width = TotalProg_pb.Width = SkinPixels(550);
            Name_label.Text = Settings.P_ServerName; Name_label.Visible = Settings.P_ServerName.Length != 0;
            
            if (Settings.P_AutoStart) Launch();
        }

        private void AMain_Click(object sender, EventArgs e)
        {
            if (ConfigForm.Visible) ConfigForm.Visible = false;
        }

        private void ActionLabel_Click(object sender, EventArgs e)
        {
            LabelSwitch = !LabelSwitch;
        }

        private bool _creditBrandSwitch;

        private void Credit_label_Click(object sender, EventArgs e)
        {
            _creditBrandSwitch = !_creditBrandSwitch;
            Credit_label.Text = _creditBrandSwitch ? "Designed by Breezer" : "Powered by Crystal M2";
        }

        private void AMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            Program.Trace("ui-closed reason=" + e.CloseReason + " launch=" + Program.Launch);


            Launch_pb?.Dispose();
            Close_pb?.Dispose();
        }

        private static string[] suffixes = new[] { " B", " KB", " MB", " GB", " TB", " PB" };

        private string ToSize(double number, int precision = 2)
        {
            // unit's number of bytes
            const double unit = 1024;
            // suffix counter
            int i = 0;
            // as long as we're bigger than a unit, keep going
            while (number > unit)
            {
                number /= unit;
                i++;
            }
            // apply precision and current suffix
            return Math.Round(number, precision) + suffixes[i];
        }

    }
}
