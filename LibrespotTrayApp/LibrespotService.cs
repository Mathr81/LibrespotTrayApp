using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace LibrespotTrayApp;

internal enum LibrespotState
{
    Stopped,
    Starting,
    WaitingForLogin,
    Connected,
    Restarting,
    Failed,
}

/// <summary>
/// Runs librespot in the background and keeps it alive: restarts it with a backoff when it exits,
/// stops it gracefully, and tracks its login state from its output.
/// All public members must be used from the UI thread.
/// </summary>
internal sealed class LibrespotService : IDisposable
{
    private static readonly int[] RestartDelaysSeconds = [1, 2, 5, 10, 30, 60];
    private static readonly TimeSpan StableUptime = TimeSpan.FromMinutes(1);
    private const int FailuresBeforeAlert = 4;

    private readonly LogFile log;
    private readonly SynchronizationContext ui;
    private readonly nint job = NativeMethods.CreateKillOnCloseJob();
    private readonly System.Windows.Forms.Timer restartTimer = new();

    private AppSettings settings;
    private Process? process;
    private DateTime startedAt;
    private int consecutiveFailures;
    private volatile bool credentialsRejected;
    private bool failureReported;
    private bool transitioning;

    public LibrespotService(AppSettings settings, LogFile log)
    {
        this.settings = settings;
        this.log = log;
        ui = SynchronizationContext.Current ?? throw new InvalidOperationException("Must be created on the UI thread.");
        restartTimer.Tick += (_, _) =>
        {
            restartTimer.Stop();
            Start();
        };
    }

    public LibrespotState State { get; private set; } = LibrespotState.Stopped;

    /// <summary>Spotify account name once connected, error message when failed.</summary>
    public string? Detail { get; private set; }

    /// <summary>Spotify authorization page, while waiting for the user to log in.</summary>
    public string? LoginUrl { get; private set; }

    public event Action? StateChanged;

    /// <summary>Raised once when librespot keeps failing, with a message for the user.</summary>
    public event Action<string>? PersistentFailure;

    public void Start()
    {
        restartTimer.Stop();
        if (process != null)
        {
            return;
        }

        if (!File.Exists(AppPaths.LibrespotExe))
        {
            log.Info($"Introuvable : {AppPaths.LibrespotExe}");
            SetState(LibrespotState.Failed, "librespot.exe est introuvable, réinstallez l'application.");
            return;
        }

        if (credentialsRejected)
        {
            credentialsRejected = false;
            DeleteCachedCredentials();
        }

        LoginUrl = null;
        ProcessStartInfo startInfo = BuildStartInfo();
        log.Info("Démarrage : librespot " + string.Join(' ', startInfo.ArgumentList.Select(QuoteForLog)));

        var p = new Process { StartInfo = startInfo, EnableRaisingEvents = true };
        p.OutputDataReceived += (_, e) => OnOutput(p, e.Data);
        p.ErrorDataReceived += (_, e) => OnOutput(p, e.Data);
        p.Exited += (_, _) =>
        {
            WaitForLastOutput(p);
            ui.Post(_ => OnExited(p), null);
        };

        try
        {
            p.Start();
        }
        catch (Win32Exception ex)
        {
            p.Dispose();
            log.Info($"Impossible de lancer librespot : {ex.Message}");
            SetState(LibrespotState.Failed, ex.Message);
            return;
        }

        process = p;
        startedAt = DateTime.UtcNow;
        TuneProcess(p);
        p.BeginOutputReadLine();
        p.BeginErrorReadLine();
        SetState(LibrespotState.Starting);
    }

    public async Task StopAsync()
    {
        restartTimer.Stop();
        Process? p = process;
        process = null;
        if (p != null)
        {
            log.Info("Arrêt de librespot");
            await Task.Run(() => Terminate(p));
            p.Dispose();
        }

        SetState(LibrespotState.Stopped);
    }

    /// <summary>Restarts librespot, optionally with new settings.</summary>
    public async Task RestartAsync(AppSettings? newSettings = null)
    {
        if (newSettings != null)
        {
            settings = newSettings;
        }

        if (transitioning)
        {
            return;
        }

        transitioning = true;
        try
        {
            consecutiveFailures = 0;
            failureReported = false;
            await StopAsync();
            Start();
        }
        finally
        {
            transitioning = false;
        }
    }

    /// <summary>Forgets the Spotify account: the login page is shown again on the next start.</summary>
    public async Task LogOutAsync()
    {
        transitioning = true;
        try
        {
            await StopAsync();
            DeleteCachedCredentials();
            consecutiveFailures = 0;
            failureReported = false;
            Start();
        }
        finally
        {
            transitioning = false;
        }
    }

    public static bool HasCachedCredentials => File.Exists(AppPaths.CredentialsFile);

    private ProcessStartInfo BuildStartInfo()
    {
        var startInfo = new ProcessStartInfo(AppPaths.LibrespotExe)
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
            WorkingDirectory = AppPaths.DataDir,
        };

        Collection<string> args = startInfo.ArgumentList;
        args.Add("--name");
        args.Add(settings.DeviceName);
        args.Add("--device-type");
        args.Add(settings.DeviceType);
        args.Add("--bitrate");
        args.Add(settings.Bitrate);

        // 32-bit float is the native format of the Windows audio engine: no conversion and no
        // dithering in librespot, and no loss of precision when the volume is lowered.
        args.Add("--format");
        args.Add("F32");

        args.Add("--system-cache");
        args.Add(AppPaths.SessionDir);
        args.Add("--enable-oauth");
        args.Add("--oauth-port");
        args.Add(settings.OAuthPort.ToString(CultureInfo.InvariantCulture));

        if (settings.AudioCacheSizeGb > 0)
        {
            args.Add("--cache");
            args.Add(settings.AudioCacheDir);
            args.Add("--cache-size-limit");
            args.Add(settings.AudioCacheSizeGb.ToString(CultureInfo.InvariantCulture) + "G");
        }

        if (!settings.RememberVolume || !File.Exists(AppPaths.VolumeFile))
        {
            args.Add("--initial-volume");
            args.Add(settings.InitialVolume.ToString(CultureInfo.InvariantCulture));
        }

        if (settings.EnableVolumeNormalization)
        {
            args.Add("--enable-volume-normalisation");
        }

        if (settings.EnableAutoplay)
        {
            args.Add("--autoplay");
            args.Add("on");
        }

        if (!settings.EnableDiscovery)
        {
            args.Add("--disable-discovery");
        }

        if (settings.AudioDevice != null)
        {
            args.Add("--device");
            args.Add(settings.AudioDevice);
        }

        if (settings.VerboseLogging)
        {
            args.Add("--verbose");
        }

        return startInfo;
    }

    private void TuneProcess(Process p)
    {
        try
        {
            NativeMethods.AssignToJob(job, p.Handle);
            p.PriorityClass = settings.ProcessPriority;
            NativeMethods.DisablePowerThrottling(p.Handle);
        }
        catch (Exception ex) when (ex is Win32Exception or InvalidOperationException)
        {
            // The process may already have exited; OnExited takes care of it.
            log.Info($"Réglage du processus impossible : {ex.Message}");
        }
    }

    /// <summary>Runs on a thread pool thread.</summary>
    private void OnOutput(Process p, string? line)
    {
        if (line == null)
        {
            return;
        }

        log.Write(line);

        if (line.StartsWith("Browse to: ", StringComparison.Ordinal))
        {
            string url = line["Browse to: ".Length..].Trim();
            ui.Post(_ =>
            {
                if (p == process)
                {
                    LoginUrl = url;
                    SetState(LibrespotState.WaitingForLogin);
                }
            }, null);
        }
        else if (line.Contains("Authenticated as '", StringComparison.Ordinal))
        {
            int start = line.IndexOf("Authenticated as '", StringComparison.Ordinal) + "Authenticated as '".Length;
            int end = line.LastIndexOf('\'');
            string? account = end > start ? line[start..end] : null;
            ui.Post(_ =>
            {
                if (p == process)
                {
                    LoginUrl = null;
                    failureReported = false;
                    SetState(LibrespotState.Connected, account);
                }
            }, null);
        }
        else if (line.Contains("Spirc shut down unexpectedly", StringComparison.Ordinal))
        {
            ui.Post(_ =>
            {
                if (p == process)
                {
                    SetState(LibrespotState.Starting);
                }
            }, null);
        }
        else if (line.Contains("Bad credentials", StringComparison.Ordinal))
        {
            credentialsRejected = true;
        }
    }

    private void OnExited(Process p)
    {
        if (p != process)
        {
            // Stopped on purpose.
            return;
        }

        process = null;
        int exitCode = SafeExitCode(p);
        TimeSpan uptime = DateTime.UtcNow - startedAt;
        p.Dispose();
        log.Info($"librespot s'est arrêté (code {exitCode}) après {uptime:hh\\:mm\\:ss}");

        if (uptime >= StableUptime)
        {
            consecutiveFailures = 0;
        }

        consecutiveFailures++;
        int delay = RestartDelaysSeconds[Math.Min(consecutiveFailures, RestartDelaysSeconds.Length) - 1];
        if (credentialsRejected)
        {
            log.Info("Spotify a refusé les identifiants enregistrés : une nouvelle connexion est nécessaire.");
            delay = 1;
        }

        if (consecutiveFailures >= FailuresBeforeAlert && !failureReported)
        {
            failureReported = true;
            PersistentFailure?.Invoke("librespot s'arrête de façon répétée. Cliquez pour ouvrir le journal.");
        }

        SetState(LibrespotState.Restarting, $"nouvel essai dans {delay} s");
        restartTimer.Interval = delay * 1000;
        restartTimer.Start();
    }

    /// <summary>Lets the last lines (often the reason of the exit) reach the log.</summary>
    private static void WaitForLastOutput(Process p)
    {
        try
        {
            Task.Run(() => p.WaitForExit()).Wait(TimeSpan.FromSeconds(2));
        }
        catch (AggregateException)
        {
            // Disposed meanwhile: stopped on purpose.
        }
    }

    private static int SafeExitCode(Process p)
    {
        try
        {
            return p.ExitCode;
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
    }

    /// <summary>Asks librespot to shut down cleanly (it then disappears at once from Spotify), or kills it.</summary>
    private static void Terminate(Process p)
    {
        try
        {
            if (p.HasExited)
            {
                return;
            }

            if (NativeMethods.SendCtrlC(p.Id) && p.WaitForExit(3000))
            {
                return;
            }

            p.Kill(entireProcessTree: true);
            p.WaitForExit(3000);
        }
        catch (Exception ex) when (ex is InvalidOperationException or Win32Exception or NotSupportedException)
        {
            // Already gone.
        }
    }

    private void DeleteCachedCredentials()
    {
        try
        {
            File.Delete(AppPaths.CredentialsFile);
            log.Info("Identifiants Spotify supprimés");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            log.Info($"Impossible de supprimer les identifiants : {ex.Message}");
        }
    }

    private void SetState(LibrespotState state, string? detail = null)
    {
        if (State == state && Detail == detail)
        {
            return;
        }

        State = state;
        Detail = detail;
        StateChanged?.Invoke();
    }

    private static string QuoteForLog(string arg) =>
        arg.Length == 0 || arg.Any(char.IsWhiteSpace) ? $"\"{arg}\"" : arg;

    public void Dispose()
    {
        restartTimer.Dispose();
        Process? p = process;
        process = null;
        if (p != null)
        {
            Terminate(p);
            p.Dispose();
        }
    }
}
