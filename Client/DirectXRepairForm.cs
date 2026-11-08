namespace Client
{
    internal sealed class DirectXRepairForm : Form
    {
        private readonly Label _status;
        private readonly ProgressBar _progress;
        private readonly Button _repair;
        private readonly Button _exit;
        private readonly CancellationTokenSource _cancellation = new CancellationTokenSource();
        private bool _busy;
        private bool _canCancel;

        internal DirectXRepairForm()
        {
            Text = GameLanguage.Get("Missing game runtime");
            ClientSize = new Size(570, 270);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            AutoScaleMode = AutoScaleMode.Font;

            _status = new Label
            {
                Text = GameLanguage.Get("The DirectX 9 extension runtime (D3DX9) is missing. DirectX 11/12 alone does not include it. Repair will download Microsoft's official runtime and open its setup wizard. Please confirm the installation and administrator prompt. The runtime cannot be uninstalled."),
                Location = new Point(18, 18), Size = new Size(534, 140)
            };
            _progress = new ProgressBar { Location = new Point(18, 168), Size = new Size(534, 20) };
            _repair = new Button
            {
                Text = GameLanguage.Get("Repair and recheck"),
                Location = new Point(18, 210), Size = new Size(180, 36)
            };
            var manual = new Button
            {
                Text = GameLanguage.Get("Official download"),
                Location = new Point(208, 210), Size = new Size(180, 36)
            };
            _exit = new Button
            {
                Text = GameLanguage.Get("Exit"), Location = new Point(398, 210), Size = new Size(154, 36)
            };
            _repair.Click += Repair_Click;
            manual.Click += (_, _) => StartupRecovery.Open(StartupRecovery.DownloadPage, this);
            _exit.Click += (_, _) => Close();
            Controls.AddRange(new Control[] { _status, _progress, _repair, manual, _exit });
            FormClosing += (_, e) =>
            {
                if (!_busy) return;
                e.Cancel = true;
                if (_canCancel)
                {
                    _cancellation.Cancel();
                    _exit.Enabled = false;
                }
            };
        }

        private async void Repair_Click(object sender, EventArgs e)
        {
            if (_busy) return;
            _busy = true;
            _canCancel = true;
            _repair.Enabled = false;
            bool repaired = false;
            string directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Crystal", "RuntimeRepair", Guid.NewGuid().ToString("N"));
            try
            {
                Directory.CreateDirectory(directory);
                string package = Path.Combine(directory, "directx_Jun2010_redist.exe");
                _status.Text = GameLanguage.Get("Downloading the official DirectX runtime. You can cancel by closing this window.");
                using var timeout = CancellationTokenSource.CreateLinkedTokenSource(_cancellation.Token);
                timeout.CancelAfter(TimeSpan.FromMinutes(10));
                await DirectXRuntimeInstaller.DownloadAsync(package, new Progress<int>(value =>
                {
                    if (!IsDisposed && !Disposing) _progress.Value = value;
                }), timeout.Token);
                _cancellation.Token.ThrowIfCancellationRequested();
                _canCancel = false;
                _exit.Enabled = false;
                _status.Text = GameLanguage.Get("Verifying and extracting the Microsoft runtime...");
                await Task.Run(() => DirectXRuntimeInstaller.VerifyPackage(package));
                await DirectXRuntimeInstaller.ExtractAsync(package, directory);
                _status.Text = GameLanguage.Get("Please complete the Microsoft DirectX setup wizard and administrator prompt. This window will recheck the runtime when setup exits.");
                await DirectXRuntimeInstaller.InstallAsync(directory);
                _status.Text = GameLanguage.Get("Checking DirectX in a separate process...");
                await StartupRecovery.VerifyDirectXAsync();
                MessageBox.Show(this, GameLanguage.Get("DirectX is available. The client will now restart."), Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                repaired = true;
            }
            catch (OperationCanceledException)
            {
                _status.Text = GameLanguage.Get("Runtime download was cancelled or timed out. No installation was started.");
            }
            catch (Exception ex)
            {
                StartupRecovery.Log(ex);
                _status.Text = GameLanguage.Get("Repair did not complete. Use the official download button to install DirectX manually, then restart the client. See Error.txt for details.")
                    + Environment.NewLine + ex.Message;
            }
            finally
            {
                _busy = false;
                _exit.Enabled = true;
                try { if (Directory.Exists(directory)) Directory.Delete(directory, true); }
                catch (Exception ex) { StartupRecovery.Log(ex); }
                if (repaired) { DialogResult = DialogResult.OK; Close(); }
                else if (_cancellation.IsCancellationRequested) Close();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _cancellation.Dispose();
            base.Dispose(disposing);
        }
    }
}
