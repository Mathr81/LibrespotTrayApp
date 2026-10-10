namespace LibrespotTrayApp;

internal static class AppPaths
{
    public const string AppName = "LibrespotTrayApp";

    /// <summary>%LocalAppData%\LibrespotTrayApp: settings, logs and caches (never roamed).</summary>
    public static readonly string DataDir =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppName);

    public static readonly string SettingsFile = Path.Combine(DataDir, "config.json");
    public static readonly string LogFile = Path.Combine(DataDir, "librespot.log");
    public static readonly string PreviousLogFile = Path.Combine(DataDir, "librespot.previous.log");
    public static readonly string ErrorLogFile = Path.Combine(DataDir, "error.log");

    /// <summary>librespot "system cache": cached credentials and last volume.</summary>
    public static readonly string SessionDir = Path.Combine(DataDir, "session");
    public static readonly string CredentialsFile = Path.Combine(SessionDir, "credentials.json");
    public static readonly string VolumeFile = Path.Combine(SessionDir, "volume");

    public static readonly string DefaultAudioCacheDir = Path.Combine(DataDir, "cache");

    /// <summary>Location used by version 1.x (in the roaming profile).</summary>
    public static readonly string LegacyAudioCacheDir =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppName, "cache");

    public static readonly string LibrespotExe = Path.Combine(AppContext.BaseDirectory, "librespot.exe");
}
