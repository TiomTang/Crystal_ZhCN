using System.Diagnostics;
using Client.Bootstrap;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using Client.Resolution;

namespace Client
{
    internal static class Program
    {
        public static CMain Form;

        public static bool Restart;

        [STAThread]
        private static void Main(string[] args)
        {
            Directory.SetCurrentDirectory(AppContext.BaseDirectory);
            LaunchOptions options;
            try { options = LaunchOptions.Parse(args); }
            catch (ArgumentException ex) { MessageBox.Show(ex.Message, "Invalid launch arguments"); Environment.ExitCode = 2; return; }
            try
            {
                using var installation = new InstallationLease(AppContext.BaseDirectory, game: true);

            Settings.UseTestConfig = options.TestConfig;
#if DEBUG
            Settings.UseTestConfig = true;
#endif


                if (RuntimePolicyHelper.LegacyV2RuntimeEnabledSuccessfully == true) { }

                Packet.IsServer = false;
                Settings.Load();
                Settings.ApplyLaunchOptions(options);

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                CheckResolutionSetting();

                Application.Run(Form = new CMain());

                Settings.Save();

                if (Restart)
                {
                    Application.Restart();
                }
            }
            catch (Exception ex)
            {
                CMain.SaveError(ex.ToString());
            }
        }

        public static class RuntimePolicyHelper
        {
            public static bool LegacyV2RuntimeEnabledSuccessfully { get; private set; }

            static RuntimePolicyHelper()
            {
                //ICLRRuntimeInfo clrRuntimeInfo =
                //    (ICLRRuntimeInfo)RuntimeEnvironment.GetRuntimeInterfaceAsObject(
                //        Guid.Empty,
                //        typeof(ICLRRuntimeInfo).GUID);

                //try
                //{
                //    clrRuntimeInfo.BindAsLegacyV2Runtime();
                //    LegacyV2RuntimeEnabledSuccessfully = true;
                //}
                //catch (COMException)
                //{
                //    // This occurs with an HRESULT meaning 
                //    // "A different runtime was already bound to the legacy CLR version 2 activation policy."
                //    LegacyV2RuntimeEnabledSuccessfully = false;
                //}
            }

            [ComImport]
            [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
            [Guid("BD39D1D2-BA2F-486A-89B0-B4B0CB466891")]
            private interface ICLRRuntimeInfo
            {
                void xGetVersionString();
                void xGetRuntimeDirectory();
                void xIsLoaded();
                void xIsLoadable();
                void xLoadErrorString();
                void xLoadLibrary();
                void xGetProcAddress();
                void xGetInterface();
                void xSetDefaultStartupFlags();
                void xGetDefaultStartupFlags();

                [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
                void BindAsLegacyV2Runtime();
            }
        }

        public static void CheckResolutionSetting()
        {
            var parsedOK = DisplayResolutions.GetDisplayResolutions();
            if (!parsedOK)
            {
                MessageBox.Show(GameLanguage.Get("Could not get display resolutions"), GameLanguage.Get("Get Display Resolution Issue"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
            }

            if (!DisplayResolutions.IsSupported(Settings.Resolution))
            {
                MessageBox.Show(GameLanguage.Get("Client does not support {0}. Setting Resolution to 1024x768.", Settings.Resolution),
                    GameLanguage.Get("Invalid Client Resolution"),
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);

                Settings.Resolution = (int)eSupportedResolution.w1024h768;
                Settings.Save();
            }
        }

    }
}