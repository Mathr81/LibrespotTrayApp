using Microsoft.Win32;

namespace LibrespotTrayApp;

/// <summary>"Run at Windows startup" entry (HKCU\...\Run, also written by the installer).</summary>
internal static class StartupRegistration
{
    private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";

    private static string Command => $"\"{Environment.ProcessPath ?? Application.ExecutablePath}\"";

    public static bool IsEnabled()
    {
        using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RunKey);
        return key?.GetValue(AppPaths.AppName) is string;
    }

    public static void SetEnabled(bool enabled)
    {
        using RegistryKey key = Registry.CurrentUser.CreateSubKey(RunKey);
        if (enabled)
        {
            key.SetValue(AppPaths.AppName, Command);
        }
        else
        {
            key.DeleteValue(AppPaths.AppName, throwOnMissingValue: false);
        }
    }

    /// <summary>Keeps an existing entry pointing to this executable (e.g. after a reinstall elsewhere).</summary>
    public static void RepairIfEnabled()
    {
        using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RunKey, writable: true);
        if (key?.GetValue(AppPaths.AppName) is string value && !string.Equals(value, Command, StringComparison.OrdinalIgnoreCase))
        {
            key.SetValue(AppPaths.AppName, Command);
        }
    }
}
