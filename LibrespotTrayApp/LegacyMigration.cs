namespace LibrespotTrayApp;

/// <summary>Brings data over from version 1.x, which stored everything in the roaming profile.</summary>
internal static class LegacyMigration
{
    public static void Run(AppSettings settings, LogFile log)
    {
        Directory.CreateDirectory(AppPaths.SessionDir);

        // 1.x kept the Spotify credentials next to the audio cache: reuse them to avoid a new login.
        foreach (string dir in new[] { AppPaths.LegacyAudioCacheDir, settings.AudioCacheDir })
        {
            CopyIfMissing(Path.Combine(dir, "credentials.json"), AppPaths.CredentialsFile, log);
            CopyIfMissing(Path.Combine(dir, "volume"), AppPaths.VolumeFile, log);
        }

        // The 1.x audio cache had no size limit and lives in the roaming profile: free that space.
        string legacyAudioFiles = Path.Combine(AppPaths.LegacyAudioCacheDir, "files");
        if (Directory.Exists(legacyAudioFiles) && !IsSameOrParent(AppPaths.LegacyAudioCacheDir, settings.AudioCacheDir))
        {
            try
            {
                Directory.Delete(legacyAudioFiles, recursive: true);
                log.Info($"Ancien cache audio supprimé : {legacyAudioFiles}");
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                log.Info($"Impossible de supprimer l'ancien cache audio : {ex.Message}");
            }
        }
    }

    private static void CopyIfMissing(string source, string destination, LogFile log)
    {
        try
        {
            if (!File.Exists(destination) && File.Exists(source))
            {
                File.Copy(source, destination);
                log.Info($"Repris de la version précédente : {Path.GetFileName(source)}");
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            log.Info($"Impossible de reprendre {source} : {ex.Message}");
        }
    }

    private static bool IsSameOrParent(string dir, string path)
    {
        try
        {
            string a = Path.GetFullPath(dir).TrimEnd('\\', '/') + Path.DirectorySeparatorChar;
            string b = Path.GetFullPath(path).TrimEnd('\\', '/') + Path.DirectorySeparatorChar;
            return b.StartsWith(a, StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return true;
        }
    }
}
