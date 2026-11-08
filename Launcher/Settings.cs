using Client.Bootstrap;
namespace Launcher;
internal static class Settings
{
    private static InIReader UserReader;
    private static string Language = "English";
    public static bool PatchNeedsLogin;
    public static string PatchLogin = "", PatchPassword = "";
    // Without an operator configuration, never contact a public update site.
    public static string P_Host = "", P_PatchFileName = "PList.gz", P_ServerName = "", P_BrowserAddress = "";
    public static string P_Client => AppContext.BaseDirectory;
    public static bool P_AutoStart, UseTestConfig, FPSCap = true, TopMost;
    public static int Resolution = 1024, ResolutionHeight = 768;
    public static DisplayMode Mode = DisplayMode.Windowed;
    public static string Host = "127.0.0.1";
    public static int Port = 7000;
    public static PatchSource Snapshot() => new(P_Host, P_PatchFileName, PatchNeedsLogin, PatchLogin, PatchPassword);
    public static HttpClient HttpClient() => Snapshot().CreateClient();
    public static void ReloadPublished()
    {
        var reader = new InIReader(Path.GetFullPath("Launcher.ini"));
        PatchNeedsLogin = reader.ReadBoolean("Launcher", "NeedLogin", false, false);
        PatchLogin = reader.ReadString("Launcher", "Login", "", false);
        PatchPassword = reader.ReadString("Launcher", "Password", "", false);
        P_Host = reader.ReadString("Launcher", "Host", "", false);
        P_PatchFileName = reader.ReadString("Launcher", "PatchFile", "PList.gz", false);
        P_ServerName = reader.ReadString("Launcher", "ServerName", "", false);
        P_BrowserAddress = reader.ReadString("Launcher", "Browser", "", false);
        Host = reader.ReadString("Network", "IPAddress", "127.0.0.1", false);
        Port = reader.ReadInt32("Network", "Port", 7000, false);
        if (P_Host.StartsWith("www.")) P_Host = "http://" + P_Host;
        if (P_Host.Length > 0 && !P_Host.EndsWith("/")) P_Host += "/";
        new LaunchOptions { Host = Host, Port = Port }.Arguments();
    }
    public static void Load()
    {
        ReloadPublished();
        bool migrate = !File.Exists("Launcher.User.ini");
        string source = migrate ? (File.Exists("Launcher.ini") ? "Launcher.ini" : File.Exists("Mir2Config.ini") ? "Mir2Config.ini" : "Mir2Test.ini") : "Launcher.User.ini";
        var reader = new InIReader(Path.GetFullPath(source));
        var legacyGame = new InIReader(Path.GetFullPath(File.Exists("Mir2Config.ini") ? "Mir2Config.ini" : "Mir2Test.ini"));
        UseTestConfig = reader.ReadBoolean("Launcher", "TestConfig", migrate && legacyGame.ReadBoolean("Launcher", "TestConfig", false, false), false);
        var game = new InIReader(Path.GetFullPath(UseTestConfig ? "Mir2Test.ini" : "Mir2Config.ini"));
        // An operator-only INI has no personal keys. Migrate per key, not per file:
        // old Launcher personal values win, missing ones fall back to game settings.
        P_AutoStart = reader.ReadBoolean("Launcher", "AutoStart", migrate && game.ReadBoolean("Launcher", "AutoStart", false, false), false);
        Resolution = reader.ReadInt32("Graphics", "Resolution", migrate ? game.ReadInt32("Graphics", "Resolution", 1024, false) : 1024, false);
        ResolutionHeight = reader.ReadInt32("Graphics", "ResolutionHeight", migrate ? game.ReadInt32("Graphics", "ResolutionHeight", 768, false) : 768, false);
        int gameMode = game.ReadInt32("Graphics", "DisplayMode", game.ReadBoolean("Graphics", "FullScreen", false, false) ? 2 : 0, false);
        int fallbackMode = migrate ? gameMode : 0;
        string oldFullScreen = reader.ReadString("Graphics", "FullScreen", "", false);
        if (bool.TryParse(oldFullScreen, out bool fullScreen)) fallbackMode = fullScreen ? 2 : 0;
        Mode = (DisplayMode)reader.ReadInt32("Graphics", "DisplayMode", fallbackMode, false);
        FPSCap = reader.ReadBoolean("Graphics", "FPSCap", migrate ? game.ReadBoolean("Graphics", "FPSCap", true, false) : true, false);
        TopMost = reader.ReadBoolean("Graphics", "AlwaysOnTop", migrate && game.ReadBoolean("Graphics", "AlwaysOnTop", false, false), false);
        Language = reader.ReadString("Game", "Language", game.ReadString("Game", "Language", "English", false), false);
        // Use the same immutable pack and optional developer capture policy as the game.
        GameLanguage.LoadClientLanguage(Path.Combine("Localization", Language + ".json"));
        Options.Arguments();
        UserReader = new InIReader(Path.GetFullPath("Launcher.User.ini"));
        if (migrate) Save();
    }
    public static LaunchOptions Options => new() { Host = Host, Port = Port, Resolution = $"{Resolution}x{ResolutionHeight}", DisplayMode = Mode switch { DisplayMode.FullScreen => "fullscreen", DisplayMode.Maximized => "maximized", _ => "windowed" }, FpsCap = FPSCap, Topmost = TopMost, TestConfig = UseTestConfig };
    public static void Save()
    {
        UserReader.Write("Launcher", "AutoStart", P_AutoStart); UserReader.Write("Launcher", "TestConfig", UseTestConfig);
        UserReader.Write("Graphics", "Resolution", Resolution); UserReader.Write("Graphics", "ResolutionHeight", ResolutionHeight); UserReader.Write("Graphics", "DisplayMode", (int)Mode); UserReader.Write("Graphics", "FPSCap", FPSCap); UserReader.Write("Graphics", "AlwaysOnTop", TopMost);
        UserReader.Write("Game", "Language", Language);
    }
}
internal enum DisplayMode { Windowed, Maximized, FullScreen }
