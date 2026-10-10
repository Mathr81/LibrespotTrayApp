using System.Diagnostics;
using System.Runtime;
using Microsoft.Win32;

namespace LibrespotTrayApp;

internal sealed class TrayApplicationContext : ApplicationContext
{
    private enum BalloonAction
    {
        None,
        OpenLog,
        OpenLogin,
    }

    private static readonly TimeSpan ResumeDelay = TimeSpan.FromSeconds(5);

    private readonly LogFile log;
    private readonly SynchronizationContext ui;
    private readonly LibrespotService librespot;
    private readonly Icon activeIcon = LoadIcon("app.ico", SystemInformation.SmallIconSize);
    private readonly Icon inactiveIcon = LoadIcon("app-inactive.ico", SystemInformation.SmallIconSize);
    private readonly NotifyIcon trayIcon;
    private readonly ToolStripMenuItem statusItem = new() { Enabled = false };
    private readonly ToolStripMenuItem loginItem = new("Se connecter à Spotify…") { Visible = false };
    private readonly ToolStripMenuItem startupItem = new("Lancer au démarrage de Windows") { CheckOnClick = true };
    private readonly System.Windows.Forms.Timer trimTimer = new();
    private readonly System.Windows.Forms.Timer resumeTimer = new() { Interval = (int)ResumeDelay.TotalMilliseconds };
    private readonly RegisteredWaitHandle activationWait;

    private AppSettings settings;
    private SettingsForm? settingsForm;
    private BalloonAction balloonAction;
    private LibrespotState previousState;
    private bool exiting;

    public TrayApplicationContext(LogFile log, EventWaitHandle activationEvent)
    {
        this.log = log;
        ui = SynchronizationContext.Current!;

        settings = AppSettings.Load(out string? settingsWarning);
        LegacyMigration.Run(settings, log);
        TryRun(StartupRegistration.RepairIfEnabled);

        librespot = new LibrespotService(settings, log);
        librespot.StateChanged += OnLibrespotStateChanged;
        librespot.PersistentFailure += message => ShowBalloon("Librespot", message, ToolTipIcon.Warning, BalloonAction.OpenLog);

        var menu = new ContextMenuStrip();
        var settingsItem = new ToolStripMenuItem("Paramètres…", null, (_, _) => ShowSettings());
        settingsItem.Font = new Font(settingsItem.Font, FontStyle.Bold);
        loginItem.Click += (_, _) => OpenLoginPage();
        startupItem.Click += (_, _) => TryRun(() => StartupRegistration.SetEnabled(startupItem.Checked));
        menu.Items.AddRange(
        [
            statusItem,
            loginItem,
            new ToolStripSeparator(),
            settingsItem,
            new ToolStripMenuItem("Redémarrer librespot", null, async (_, _) => await librespot.RestartAsync()),
            startupItem,
            new ToolStripMenuItem("Ouvrir le journal", null, (_, _) => OpenLog()),
            new ToolStripSeparator(),
            new ToolStripMenuItem("Quitter", null, async (_, _) => await ExitAsync()),
        ]);
        menu.Opening += (_, _) => startupItem.Checked = TryGet(StartupRegistration.IsEnabled);
        menu.Closed += (_, _) => ScheduleMemoryTrim(TimeSpan.FromSeconds(5));

        trayIcon = new NotifyIcon
        {
            Icon = inactiveIcon,
            Text = "Librespot",
            ContextMenuStrip = menu,
            Visible = true,
        };
        trayIcon.DoubleClick += (_, _) => ShowSettings();
        trayIcon.BalloonTipClicked += (_, _) => OnBalloonClicked();

        trimTimer.Tick += (_, _) =>
        {
            trimTimer.Stop();
            TrimMemory();
        };
        resumeTimer.Tick += async (_, _) =>
        {
            resumeTimer.Stop();
            log.Info("Sortie de veille : redémarrage de librespot");
            await librespot.RestartAsync();
        };
        SystemEvents.PowerModeChanged += OnPowerModeChanged;

        // A second launch of the app (e.g. from the Start menu) opens the settings of this one.
        activationWait = ThreadPool.RegisterWaitForSingleObject(
            activationEvent, (_, _) => ui.Post(_ => ShowSettings(), null), null, Timeout.Infinite, executeOnlyOnce: false);

        UpdateStatus();
        librespot.Start();

        if (settingsWarning != null)
        {
            ShowBalloon("Librespot", settingsWarning, ToolTipIcon.Warning, BalloonAction.None);
        }

        // Once started, the tray app has nothing to do: give its memory back to the system.
        ScheduleMemoryTrim(TimeSpan.FromSeconds(20));
    }

    private void OnLibrespotStateChanged()
    {
        LibrespotState state = librespot.State;
        if (state == LibrespotState.WaitingForLogin && previousState != LibrespotState.WaitingForLogin)
        {
            ShowBalloon(
                "Connexion à Spotify",
                "Connectez-vous dans la page ouverte dans votre navigateur. Cliquez ici si elle ne s'est pas ouverte.",
                ToolTipIcon.Info,
                BalloonAction.OpenLogin);
        }
        else if (state == LibrespotState.Connected && previousState == LibrespotState.WaitingForLogin)
        {
            ShowBalloon("Connecté à Spotify", $"« {settings.DeviceName} » est disponible dans Spotify.", ToolTipIcon.Info, BalloonAction.None);
        }
        else if (state == LibrespotState.Failed)
        {
            ShowBalloon("Librespot", librespot.Detail ?? "Erreur", ToolTipIcon.Error, BalloonAction.OpenLog);
        }

        if (state == LibrespotState.Connected && previousState != LibrespotState.Connected)
        {
            ScheduleMemoryTrim(TimeSpan.FromSeconds(10));
        }

        previousState = state;
        UpdateStatus();
    }

    private void UpdateStatus()
    {
        string status = librespot.State switch
        {
            LibrespotState.Connected => librespot.Detail is { } account ? $"Prêt — {account}" : "Prêt",
            LibrespotState.Starting => "Connexion…",
            LibrespotState.WaitingForLogin => "Connexion à Spotify requise",
            LibrespotState.Restarting => $"Redémarrage ({librespot.Detail})",
            LibrespotState.Failed => $"Erreur : {librespot.Detail}",
            _ => "Arrêté",
        };

        statusItem.Text = status;
        loginItem.Visible = librespot.State == LibrespotState.WaitingForLogin && librespot.LoginUrl != null;
        trayIcon.Icon = librespot.State == LibrespotState.Connected ? activeIcon : inactiveIcon;

        string tooltip = $"{settings.DeviceName} — {status}";
        trayIcon.Text = tooltip.Length > 127 ? tooltip[..127] : tooltip;
    }

    private void ShowSettings()
    {
        if (exiting)
        {
            return;
        }

        if (settingsForm != null)
        {
            if (settingsForm.WindowState == FormWindowState.Minimized)
            {
                settingsForm.WindowState = FormWindowState.Normal;
            }

            settingsForm.Activate();
            return;
        }

        var form = new SettingsForm(settings.Clone(), TryGet(StartupRegistration.IsEnabled), LoadIcon("app.ico", Size.Empty));
        form.LogOutRequested += async () =>
        {
            log.Info("Déconnexion du compte Spotify demandée");
            await librespot.LogOutAsync();
        };
        form.FormClosed += async (_, _) =>
        {
            settingsForm = null;
            AppSettings edited = form.Settings;
            bool saved = form.DialogResult == DialogResult.OK;
            bool startWithWindows = form.StartWithWindows;
            form.Icon?.Dispose();
            form.Dispose();
            if (saved)
            {
                await ApplySettingsAsync(edited, startWithWindows);
            }

            ScheduleMemoryTrim(TimeSpan.FromSeconds(5));
        };
        settingsForm = form;
        form.Show();
    }

    private async Task ApplySettingsAsync(AppSettings edited, bool startWithWindows)
    {
        TryRun(() => StartupRegistration.SetEnabled(startWithWindows));

        bool restart = settings.RequiresRestart(edited);
        settings = edited;
        try
        {
            settings.Save();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            MessageBox.Show($"Impossible d'enregistrer les paramètres :\n{ex.Message}", "Librespot", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        UpdateStatus();
        if (restart)
        {
            log.Info("Paramètres modifiés : redémarrage de librespot");
            await librespot.RestartAsync(settings);
        }
    }

    private void OnPowerModeChanged(object? sender, PowerModeChangedEventArgs e)
    {
        if (e.Mode != PowerModes.Resume)
        {
            return;
        }

        ui.Post(_ =>
        {
            if (settings.RestartAfterResume && !exiting)
            {
                // Give the network a few seconds to come back first.
                resumeTimer.Stop();
                resumeTimer.Start();
            }
        }, null);
    }

    private void ShowBalloon(string title, string text, ToolTipIcon icon, BalloonAction action)
    {
        balloonAction = action;
        trayIcon.ShowBalloonTip(10000, title, text, icon);
    }

    private void OnBalloonClicked()
    {
        switch (balloonAction)
        {
            case BalloonAction.OpenLog:
                OpenLog();
                break;
            case BalloonAction.OpenLogin:
                OpenLoginPage();
                break;
        }
    }

    private void OpenLoginPage()
    {
        if (librespot.LoginUrl is { } url)
        {
            OpenWithShell(url);
        }
    }

    private static void OpenLog()
    {
        if (File.Exists(AppPaths.LogFile))
        {
            OpenWithShell(AppPaths.LogFile);
        }
        else
        {
            MessageBox.Show("Le journal n'existe pas encore.", "Librespot", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private static void OpenWithShell(string target)
    {
        try
        {
            Process.Start(new ProcessStartInfo(target) { UseShellExecute = true })?.Dispose();
        }
        catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or InvalidOperationException)
        {
            MessageBox.Show($"Impossible d'ouvrir {target} :\n{ex.Message}", "Librespot", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ScheduleMemoryTrim(TimeSpan delay)
    {
        trimTimer.Stop();
        trimTimer.Interval = (int)delay.TotalMilliseconds;
        trimTimer.Start();
    }

    /// <summary>
    /// Returns unused memory to Windows. The tray app then only uses a few MB of RAM until the
    /// user interacts with it again.
    /// </summary>
    private void TrimMemory()
    {
        if (settingsForm != null || trayIcon.ContextMenuStrip?.Visible == true)
        {
            return;
        }

        GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive, blocking: true, compacting: true);
        NativeMethods.TrimWorkingSet();
    }

    private async Task ExitAsync()
    {
        if (exiting)
        {
            return;
        }

        exiting = true;
        log.Info("Fermeture demandée");
        settingsForm?.Close();
        trayIcon.Visible = false;
        await librespot.StopAsync();
        ExitThread();
    }

    private void TryRun(Action action)
    {
        try
        {
            action();
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or IOException or System.Security.SecurityException)
        {
            log.Info($"Erreur : {ex.Message}");
        }
    }

    private bool TryGet(Func<bool> getter)
    {
        try
        {
            return getter();
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or IOException or System.Security.SecurityException)
        {
            log.Info($"Erreur : {ex.Message}");
            return false;
        }
    }

    /// <summary>Loads an embedded icon; <see cref="Size.Empty"/> keeps every size (for windows).</summary>
    private static Icon LoadIcon(string name, Size size)
    {
        using Stream stream = typeof(TrayApplicationContext).Assembly.GetManifestResourceStream(name)
            ?? throw new InvalidOperationException($"Ressource manquante : {name}");
        return size.IsEmpty ? new Icon(stream) : new Icon(stream, size);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            SystemEvents.PowerModeChanged -= OnPowerModeChanged;
            activationWait.Unregister(null);
            trimTimer.Dispose();
            resumeTimer.Dispose();
            librespot.Dispose();
            trayIcon.Visible = false;
            trayIcon.ContextMenuStrip?.Dispose();
            trayIcon.Dispose();
            activeIcon.Dispose();
            inactiveIcon.Dispose();
        }

        base.Dispose(disposing);
    }
}
