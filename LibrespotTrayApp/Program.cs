using System.Reflection;

namespace LibrespotTrayApp;

internal static class Program
{
    private const string SingleInstanceMutexName = "LibrespotTrayApp.SingleInstance";
    private const string ActivationEventName = "LibrespotTrayApp.Activate";

    /// <summary>Helper mode used to stop librespot gracefully, see <see cref="LibrespotService"/>.</summary>
    public const string SendCtrlCArgument = "--send-ctrl-c";

    [STAThread]
    private static int Main(string[] args)
    {
        if (args is [SendCtrlCArgument, string pid] && int.TryParse(pid, out int processId))
        {
            return NativeMethods.SendCtrlC(processId) ? 0 : 1;
        }

        using var mutex = new Mutex(initiallyOwned: true, SingleInstanceMutexName, out bool firstInstance);
        if (!firstInstance)
        {
            // Already running: ask that instance to show its settings.
            if (EventWaitHandle.TryOpenExisting(ActivationEventName, out EventWaitHandle? activation))
            {
                using (activation)
                {
                    activation.Set();
                }
            }

            return 0;
        }

        using var activationEvent = new EventWaitHandle(false, EventResetMode.AutoReset, ActivationEventName);

        // The tray app only waits for clicks: never let it compete with a game for the CPU or RAM.
        NativeMethods.EnableEfficiencyModeForCurrentProcess();
        NativeMethods.SetAppUserModelId(AppPaths.AppUserModelId);

        ApplicationConfiguration.Initialize();
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += (_, e) => ReportError(e.Exception);
        AppDomain.CurrentDomain.UnhandledException += (_, e) => ReportError(e.ExceptionObject as Exception);

        try
        {
            using var log = new LogFile();
            log.Info($"LibrespotTrayApp {Version} démarré");
            using var context = new TrayApplicationContext(log, activationEvent);
            Application.Run(context);
            log.Info("LibrespotTrayApp fermé");
            return 0;
        }
        catch (Exception ex)
        {
            ReportError(ex);
            return 1;
        }
    }

    public static string Version =>
        typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion.Split('+')[0]
        ?? "?";

    private static void ReportError(Exception? exception)
    {
        string details = exception?.ToString() ?? "Erreur inconnue";
        try
        {
            Directory.CreateDirectory(AppPaths.DataDir);
            File.AppendAllText(AppPaths.ErrorLogFile, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {details}{Environment.NewLine}{Environment.NewLine}");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
        }

        MessageBox.Show(
            $"Une erreur inattendue est survenue :\n{exception?.Message}\n\nDétails : {AppPaths.ErrorLogFile}",
            "Librespot",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error);
    }
}
