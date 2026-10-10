using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LibrespotTrayApp;

public sealed class AppSettings
{
    private const int CurrentSchemaVersion = 2;

    public static readonly IReadOnlyList<(string Value, string Label)> DeviceTypes =
    [
        ("computer", "Ordinateur"),
        ("speaker", "Enceinte"),
        ("avr", "Ampli audio/vidéo"),
        ("tv", "Téléviseur"),
        ("gameconsole", "Console de jeu"),
        ("audiodongle", "Clé audio"),
        ("smartphone", "Smartphone"),
        ("tablet", "Tablette"),
        ("automobile", "Voiture"),
    ];

    public static readonly IReadOnlyList<(string Value, string Label)> Bitrates =
    [
        ("96", "Normale (96 kbit/s)"),
        ("160", "Haute (160 kbit/s)"),
        ("320", "Très haute (320 kbit/s)"),
    ];

    public static readonly IReadOnlyList<(ProcessPriorityClass Value, string Label)> Priorities =
    [
        (ProcessPriorityClass.BelowNormal, "Inférieure à la normale"),
        (ProcessPriorityClass.Normal, "Normale"),
        (ProcessPriorityClass.AboveNormal, "Supérieure à la normale (recommandé)"),
        (ProcessPriorityClass.High, "Haute"),
    ];

    /// <summary>0 for files written by version 1.x.</summary>
    public int SchemaVersion { get; set; }

    public string DeviceName { get; set; } = "Librespot";
    public string DeviceType { get; set; } = "computer";
    public string Bitrate { get; set; } = "320";

    /// <summary>Output device name, or null for the Windows default device.</summary>
    public string? AudioDevice { get; set; }

    public bool EnableVolumeNormalization { get; set; }

    /// <summary>Start with the volume of the previous session instead of <see cref="InitialVolume"/>.</summary>
    public bool RememberVolume { get; set; } = true;

    public int InitialVolume { get; set; } = 50;
    public bool EnableAutoplay { get; set; }

    /// <summary>Media keys (headset, keyboard) and "now playing" in the Windows media controls.</summary>
    public bool EnableMediaControls { get; set; } = true;
    public ProcessPriorityClass ProcessPriority { get; set; } = ProcessPriorityClass.AboveNormal;

    [JsonPropertyName("Cache")]
    public string AudioCacheDir { get; set; } = AppPaths.DefaultAudioCacheDir;

    /// <summary>Maximum size of the audio cache, 0 disables it.</summary>
    public int AudioCacheSizeGb { get; set; } = 2;

    /// <summary>Advertise the device on the local network (Spotify Connect zeroconf).</summary>
    public bool EnableDiscovery { get; set; } = true;

    public bool RestartAfterResume { get; set; } = true;
    public bool VerboseLogging { get; set; }
    public int OAuthPort { get; set; } = 8067;

    public AppSettings Clone() => (AppSettings)MemberwiseClone();

    /// <summary>Whether librespot must be restarted to apply <paramref name="other"/>.</summary>
    public bool RequiresRestart(AppSettings other) =>
        DeviceName != other.DeviceName
        || DeviceType != other.DeviceType
        || Bitrate != other.Bitrate
        || AudioDevice != other.AudioDevice
        || EnableVolumeNormalization != other.EnableVolumeNormalization
        || RememberVolume != other.RememberVolume
        || (!other.RememberVolume && InitialVolume != other.InitialVolume)
        || EnableAutoplay != other.EnableAutoplay
        || EnableMediaControls != other.EnableMediaControls
        || ProcessPriority != other.ProcessPriority
        || !string.Equals(AudioCacheDir, other.AudioCacheDir, StringComparison.OrdinalIgnoreCase)
        || AudioCacheSizeGb != other.AudioCacheSizeGb
        || EnableDiscovery != other.EnableDiscovery
        || VerboseLogging != other.VerboseLogging
        || OAuthPort != other.OAuthPort;

    /// <summary>Loads the settings, upgrading and repairing them if needed.</summary>
    public static AppSettings Load(out string? warning)
    {
        warning = null;
        AppSettings settings;
        try
        {
            settings = File.Exists(AppPaths.SettingsFile)
                ? JsonSerializer.Deserialize(File.ReadAllText(AppPaths.SettingsFile), SettingsJsonContext.Default.AppSettings) ?? new AppSettings { SchemaVersion = CurrentSchemaVersion }
                : new AppSettings { SchemaVersion = CurrentSchemaVersion };
        }
        catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException or NotSupportedException)
        {
            warning = $"Le fichier de configuration était illisible, les paramètres par défaut ont été restaurés.\n({ex.Message})";
            TryBackupCorruptFile();
            settings = new AppSettings { SchemaVersion = CurrentSchemaVersion };
        }

        settings.Upgrade();
        settings.Normalize();
        settings.Save();
        return settings;
    }

    public void Save()
    {
        Directory.CreateDirectory(AppPaths.DataDir);
        string tempFile = AppPaths.SettingsFile + ".tmp";
        File.WriteAllText(tempFile, JsonSerializer.Serialize(this, SettingsJsonContext.Default.AppSettings));
        File.Move(tempFile, AppPaths.SettingsFile, overwrite: true);
    }

    private void Upgrade()
    {
        if (SchemaVersion < 2)
        {
            // 1.x defaults that are not good for gaming anymore.
            if (ProcessPriority == ProcessPriorityClass.Normal)
            {
                ProcessPriority = ProcessPriorityClass.AboveNormal;
            }

            if (PathsEqual(AudioCacheDir, AppPaths.LegacyAudioCacheDir))
            {
                AudioCacheDir = AppPaths.DefaultAudioCacheDir;
            }
        }

        SchemaVersion = CurrentSchemaVersion;
    }

    private void Normalize()
    {
        DeviceName = string.IsNullOrWhiteSpace(DeviceName) ? "Librespot" : DeviceName.Trim();
        DeviceType = DeviceType?.Trim().ToLowerInvariant() ?? "";
        if (!DeviceTypes.Any(t => t.Value == DeviceType))
        {
            DeviceType = "speaker";
        }

        if (!Bitrates.Any(b => b.Value == Bitrate))
        {
            Bitrate = "320";
        }

        if (string.IsNullOrWhiteSpace(AudioDevice))
        {
            AudioDevice = null;
        }

        InitialVolume = Math.Clamp(InitialVolume, 0, 100);
        if (!Priorities.Any(p => p.Value == ProcessPriority))
        {
            ProcessPriority = ProcessPriorityClass.AboveNormal;
        }

        AudioCacheDir = string.IsNullOrWhiteSpace(AudioCacheDir) ? AppPaths.DefaultAudioCacheDir : AudioCacheDir.Trim();
        AudioCacheSizeGb = Math.Clamp(AudioCacheSizeGb, 0, 100);
        if (OAuthPort is < 1024 or > 65535)
        {
            OAuthPort = 8067;
        }
    }

    private static bool PathsEqual(string? a, string? b)
    {
        if (string.IsNullOrWhiteSpace(a) || string.IsNullOrWhiteSpace(b))
        {
            return false;
        }

        try
        {
            return string.Equals(
                Path.GetFullPath(a).TrimEnd('\\', '/'),
                Path.GetFullPath(b).TrimEnd('\\', '/'),
                StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return false;
        }
    }

    private static void TryBackupCorruptFile()
    {
        try
        {
            File.Copy(AppPaths.SettingsFile, AppPaths.SettingsFile + ".bak", overwrite: true);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
        }
    }
}

[JsonSourceGenerationOptions(WriteIndented = true, UseStringEnumConverter = true, ReadCommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true)]
[JsonSerializable(typeof(AppSettings))]
internal sealed partial class SettingsJsonContext : JsonSerializerContext;
