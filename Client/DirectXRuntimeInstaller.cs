using System.Diagnostics;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Client
{
    internal static class DirectXRuntimeInstaller
    {
        internal const string PackageUrl = "https://download.microsoft.com/download/8/4/a/84a35bf1-dafe-4ae8-82af-ad2ae20b6b14/directx_Jun2010_redist.exe";
        internal const string PackageHash = "053F76DCBB28802E23341B6A787E3B0791C0FA5C8D4D011B1044172DBF89C73B";
        private const long MaximumPackageSize = 110 * 1024 * 1024;

        internal static async Task DownloadAsync(string destination, IProgress<int> progress, CancellationToken cancellation)
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromMinutes(10) };
            using var response = await client.GetAsync(PackageUrl, HttpCompletionOption.ResponseHeadersRead, cancellation);
            response.EnsureSuccessStatusCode();
            Uri finalUri = response.RequestMessage.RequestUri;
            if (finalUri.Scheme != Uri.UriSchemeHttps || !string.Equals(finalUri.Host, "download.microsoft.com", StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException(GameLanguage.Get("The runtime download was redirected to an untrusted location."));
            long? length = response.Content.Headers.ContentLength;
            if (length > MaximumPackageSize)
                throw new InvalidDataException(GameLanguage.Get("The runtime download is larger than expected."));
            await using Stream source = await response.Content.ReadAsStreamAsync(cancellation);
            await using var file = new FileStream(destination, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, true);
            byte[] buffer = new byte[81920];
            long received = 0;
            int read;
            while ((read = await source.ReadAsync(buffer, cancellation)) > 0)
            {
                received += read;
                if (received > MaximumPackageSize)
                    throw new InvalidDataException(GameLanguage.Get("The runtime download is larger than expected."));
                await file.WriteAsync(buffer.AsMemory(0, read), cancellation);
                progress.Report(length > 0 ? (int)(received * 100 / length.Value) : 0);
            }
        }

        internal static void VerifyPackage(string path)
        {
            using var stream = File.OpenRead(path);
            string hash = Convert.ToHexString(SHA256.HashData(stream));
            if (!string.Equals(hash, PackageHash, StringComparison.Ordinal))
                throw new InvalidDataException(GameLanguage.Get("The downloaded runtime failed its integrity check. It will not be executed."));
            VerifyMicrosoftSignature(path);
        }

        internal static void VerifyMicrosoftSignature(string path)
        {
            var file = new TrustFile { Size = (uint)Marshal.SizeOf<TrustFile>(), Path = path };
            IntPtr pointer = Marshal.AllocHGlobal(Marshal.SizeOf<TrustFile>());
            try
            {
                Marshal.StructureToPtr(file, pointer, false);
                var data = new TrustData
                {
                    Size = (uint)Marshal.SizeOf<TrustData>(),
                    UIChoice = 2,
                    UnionChoice = 1,
                    File = pointer,
                    StateAction = 1,
                    ProviderFlags = 0x1000 | 0x10 // Cache-only trust lookup; no online revocation check.
                };
                Guid action = new Guid("00AAC56B-CD44-11D0-8CC2-00C04FC295EE");
                int result;
                try { result = WinVerifyTrust(new IntPtr(-1), ref action, ref data); }
                finally
                {
                    data.StateAction = 2;
                    WinVerifyTrust(new IntPtr(-1), ref action, ref data);
                }
                if (result != 0)
                    throw new InvalidDataException(GameLanguage.Get("The Microsoft runtime signature could not be verified. The installer will not be executed.") + $" (0x{result:X8})");
                using var certificate = new X509Certificate2(X509Certificate.CreateFromSignedFile(path));
                if (certificate.GetNameInfo(X509NameType.SimpleName, false) != "Microsoft Corporation")
                    throw new InvalidDataException(GameLanguage.Get("The runtime installer is not signed by Microsoft Corporation."));
            }
            finally
            {
                Marshal.DestroyStructure<TrustFile>(pointer);
                Marshal.FreeHGlobal(pointer);
            }
        }

        internal static async Task ExtractAsync(string package, string directory)
        {
            var start = new ProcessStartInfo(package) { UseShellExecute = false, CreateNoWindow = true };
            start.ArgumentList.Add("/Q");
            start.ArgumentList.Add("/C");
            start.ArgumentList.Add("/T:" + directory);
            using var process = Process.Start(start) ?? throw new InvalidOperationException(GameLanguage.Get("The runtime extractor could not be started."));
            using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(2));
            try { await process.WaitForExitAsync(timeout.Token); }
            catch (OperationCanceledException)
            {
                process.Kill(true);
                await process.WaitForExitAsync();
                throw new TimeoutException(GameLanguage.Get("Runtime extraction timed out."));
            }
            if (process.ExitCode != 0)
                throw new InvalidOperationException(GameLanguage.Get("Runtime extraction failed with exit code {0}.", process.ExitCode));
        }

        internal static async Task InstallAsync(string directory)
        {
            string setup = Path.Combine(directory, "DXSETUP.exe");
            VerifyMicrosoftSignature(setup);
            using var process = Process.Start(new ProcessStartInfo(setup)
            {
                WorkingDirectory = directory,
                UseShellExecute = true,
                Verb = "runas"
                // No /silent: the player must see and confirm Microsoft's setup UI.
            }) ?? throw new InvalidOperationException(GameLanguage.Get("The runtime installer could not be started."));
            await process.WaitForExitAsync();
            if (process.ExitCode != 0)
                throw new InvalidOperationException(GameLanguage.Get("DirectX setup did not complete successfully (exit code {0}).", process.ExitCode));
        }

        [DllImport("wintrust.dll", ExactSpelling = true)]
        private static extern int WinVerifyTrust(IntPtr window, ref Guid action, ref TrustData data);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct TrustFile
        {
            public uint Size;
            [MarshalAs(UnmanagedType.LPWStr)] public string Path;
            public IntPtr Handle;
            public IntPtr KnownSubject;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct TrustData
        {
            public uint Size;
            public IntPtr PolicyCallback;
            public IntPtr SIPClient;
            public uint UIChoice;
            public uint RevocationChecks;
            public uint UnionChoice;
            public IntPtr File;
            public uint StateAction;
            public IntPtr StateData;
            public IntPtr URLReference;
            public uint ProviderFlags;
            public uint UIContext;
            public IntPtr SignatureSettings;
        }
    }
}
