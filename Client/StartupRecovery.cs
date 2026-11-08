using System.ComponentModel;
using System.Diagnostics;
using SlimDX.Direct3D9;

namespace Client
{
    internal static class StartupRecovery
    {
        internal const string ProbeArgument = "--check-directx-runtime";
        internal const string DownloadPage = "https://www.microsoft.com/en-us/download/details.aspx?id=8109";

        internal static bool IsMissingDirectX(Exception exception)
        {
            for (Exception current = exception; current != null; current = current.InnerException)
            {
                if (current is Direct3DX9NotFoundException) return true;
            }
            return false;
        }

        internal static void CheckDirectX()
        {
            using var direct3D = new Direct3D();
        }

        internal static bool EnsureDirectX()
        {
            try
            {
                CheckDirectX();
                return true;
            }
            catch (Exception ex) when (IsMissingDirectX(ex))
            {
                Log(ex);
                using var dialog = new DirectXRepairForm();
                if (dialog.ShowDialog() == DialogResult.OK)
                    Application.Restart();
                return false;
            }
        }

        internal static ProcessStartInfo ClientProcess(string argument)
        {
            var start = new ProcessStartInfo(Environment.ProcessPath) { UseShellExecute = false };
            // Also support framework-dependent launches through dotnet.exe.
            if (string.Equals(Path.GetFileNameWithoutExtension(Environment.ProcessPath), "dotnet", StringComparison.OrdinalIgnoreCase))
                start.ArgumentList.Add(typeof(Program).Assembly.Location);
            start.ArgumentList.Add(argument);
            start.WorkingDirectory = Environment.CurrentDirectory;
            return start;
        }

        internal static async Task VerifyDirectXAsync()
        {
            var start = ClientProcess(ProbeArgument);
            start.CreateNoWindow = true;
            start.RedirectStandardError = true;
            using var process = Process.Start(start) ?? throw new InvalidOperationException(GameLanguage.Get("The DirectX check could not be started."));
            Task<string> errorTask = process.StandardError.ReadToEndAsync();
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            try
            {
                await process.WaitForExitAsync(timeout.Token);
            }
            catch (OperationCanceledException)
            {
                process.Kill(true);
                await process.WaitForExitAsync();
                throw new TimeoutException(GameLanguage.Get("The DirectX check timed out. Please restart the client manually."));
            }
            string error = await errorTask;
            if (process.ExitCode != 0)
                throw new InvalidOperationException(GameLanguage.Get("DirectX is still unavailable after setup. Please restart Windows or install the runtime manually.") + Environment.NewLine + error);
        }

        internal static void Log(Exception exception)
        {
            try { CMain.SaveError(exception.ToString()); }
            catch (Exception logError) { Debug.WriteLine(logError); }
        }

        internal static void ShowFatal(Exception exception, IWin32Window owner = null)
        {
            Log(exception);
            string message = GameLanguage.Get("The game could not start. Please check the error below and the Error.txt log.")
                + Environment.NewLine + Environment.NewLine + exception.GetBaseException().Message
                + Environment.NewLine + Environment.NewLine + Path.GetFullPath("Error.txt");
            MessageBox.Show(owner, message, GameLanguage.Get("Unable to start game"), MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        internal static void Open(string target, IWin32Window owner)
        {
            try { Process.Start(new ProcessStartInfo(target) { UseShellExecute = true }); }
            catch (Exception ex) when (ex is Win32Exception || ex is InvalidOperationException)
            {
                Log(ex);
                MessageBox.Show(owner, target + Environment.NewLine + ex.Message, GameLanguage.Get("Could not open link"));
            }
        }
    }
}
