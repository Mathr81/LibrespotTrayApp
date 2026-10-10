using System.Runtime.InteropServices;

namespace LibrespotTrayApp;

/// <summary>Win32 APIs used to keep librespot glitch-free and the tray app itself as light as possible.</summary>
internal static unsafe partial class NativeMethods
{
    private const int ProcessMemoryPriority = 0;
    private const int ProcessPowerThrottling = 4;
    private const uint PROCESS_POWER_THROTTLING_CURRENT_VERSION = 1;
    private const uint PROCESS_POWER_THROTTLING_EXECUTION_SPEED = 0x1;
    private const uint PROCESS_POWER_THROTTLING_IGNORE_TIMER_RESOLUTION = 0x4;
    private const uint MEMORY_PRIORITY_LOW = 2;
    private const uint MEMORY_PRIORITY_NORMAL = 5;

    private const int JobObjectExtendedLimitInformation = 9;
    private const uint JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE = 0x2000;

    private const uint CTRL_C_EVENT = 0;

    private static readonly nint CurrentProcess = -1;

    [StructLayout(LayoutKind.Sequential)]
    private struct PROCESS_POWER_THROTTLING_STATE
    {
        public uint Version;
        public uint ControlMask;
        public uint StateMask;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct JOBOBJECT_BASIC_LIMIT_INFORMATION
    {
        public long PerProcessUserTimeLimit;
        public long PerJobUserTimeLimit;
        public uint LimitFlags;
        public nuint MinimumWorkingSetSize;
        public nuint MaximumWorkingSetSize;
        public uint ActiveProcessLimit;
        public nuint Affinity;
        public uint PriorityClass;
        public uint SchedulingClass;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct IO_COUNTERS
    {
        public ulong ReadOperationCount;
        public ulong WriteOperationCount;
        public ulong OtherOperationCount;
        public ulong ReadTransferCount;
        public ulong WriteTransferCount;
        public ulong OtherTransferCount;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct JOBOBJECT_EXTENDED_LIMIT_INFORMATION
    {
        public JOBOBJECT_BASIC_LIMIT_INFORMATION BasicLimitInformation;
        public IO_COUNTERS IoInfo;
        public nuint ProcessMemoryLimit;
        public nuint JobMemoryLimit;
        public nuint PeakProcessMemoryUsed;
        public nuint PeakJobMemoryUsed;
    }

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetProcessInformation(nint hProcess, int processInformationClass, void* processInformation, uint processInformationSize);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetProcessWorkingSetSize(nint hProcess, nint dwMinimumWorkingSetSize, nint dwMaximumWorkingSetSize);

    [LibraryImport("kernel32.dll", EntryPoint = "CreateJobObjectW", SetLastError = true)]
    private static partial nint CreateJobObject(nint lpJobAttributes, nint lpName);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetInformationJobObject(nint hJob, int jobObjectInfoClass, void* lpJobObjectInfo, uint cbJobObjectInfoLength);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool AssignProcessToJobObject(nint hJob, nint hProcess);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool AttachConsole(uint dwProcessId);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool FreeConsole();

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetConsoleCtrlHandler(delegate* unmanaged<uint, int> handlerRoutine, [MarshalAs(UnmanagedType.Bool)] bool add);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GenerateConsoleCtrlEvent(uint dwCtrlEvent, uint dwProcessGroupId);

    /// <summary>
    /// Puts the tray app in "efficiency mode": EcoQoS scheduling and low memory priority, so that it
    /// never competes with a game for CPU time or physical memory.
    /// </summary>
    public static void EnableEfficiencyModeForCurrentProcess()
    {
        var throttling = new PROCESS_POWER_THROTTLING_STATE
        {
            Version = PROCESS_POWER_THROTTLING_CURRENT_VERSION,
            ControlMask = PROCESS_POWER_THROTTLING_EXECUTION_SPEED,
            StateMask = PROCESS_POWER_THROTTLING_EXECUTION_SPEED,
        };
        SetProcessInformation(CurrentProcess, ProcessPowerThrottling, &throttling, (uint)sizeof(PROCESS_POWER_THROTTLING_STATE));

        uint memoryPriority = MEMORY_PRIORITY_LOW;
        SetProcessInformation(CurrentProcess, ProcessMemoryPriority, &memoryPriority, sizeof(uint));
    }

    /// <summary>
    /// Explicitly opts a process out of EcoQoS / power throttling (which Windows 11 applies to
    /// windowless background processes and which causes audio dropouts) and gives it a normal
    /// memory priority, whatever it inherited from the tray app.
    /// </summary>
    public static void DisablePowerThrottling(nint processHandle)
    {
        var throttling = new PROCESS_POWER_THROTTLING_STATE
        {
            Version = PROCESS_POWER_THROTTLING_CURRENT_VERSION,
            ControlMask = PROCESS_POWER_THROTTLING_EXECUTION_SPEED | PROCESS_POWER_THROTTLING_IGNORE_TIMER_RESOLUTION,
            StateMask = 0,
        };
        SetProcessInformation(processHandle, ProcessPowerThrottling, &throttling, (uint)sizeof(PROCESS_POWER_THROTTLING_STATE));

        uint memoryPriority = MEMORY_PRIORITY_NORMAL;
        SetProcessInformation(processHandle, ProcessMemoryPriority, &memoryPriority, sizeof(uint));
    }

    /// <summary>Releases the pages of the current process that are not in use right now.</summary>
    public static void TrimWorkingSet() => SetProcessWorkingSetSize(CurrentProcess, -1, -1);

    /// <summary>
    /// Creates a job object that kills every process assigned to it when the tray app exits,
    /// even if it crashes or is killed, so that librespot never outlives it.
    /// </summary>
    public static nint CreateKillOnCloseJob()
    {
        nint job = CreateJobObject(0, 0);
        if (job == 0)
        {
            return 0;
        }

        var info = new JOBOBJECT_EXTENDED_LIMIT_INFORMATION();
        info.BasicLimitInformation.LimitFlags = JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE;
        return SetInformationJobObject(job, JobObjectExtendedLimitInformation, &info, (uint)sizeof(JOBOBJECT_EXTENDED_LIMIT_INFORMATION))
            ? job
            : 0;
    }

    public static bool AssignToJob(nint job, nint processHandle) => job != 0 && AssignProcessToJobObject(job, processHandle);

    /// <summary>
    /// Sends Ctrl+C to a console process (librespot runs in a hidden console). The event reaches
    /// every process attached to that console, the caller included: only call this from the
    /// short-lived helper process (see <see cref="Program"/>), never from the tray app itself.
    /// </summary>
    public static bool SendCtrlC(int processId)
    {
        FreeConsole();
        if (!AttachConsole((uint)processId))
        {
            return false;
        }

        // Ignore the event ourselves (the helper never starts any process that could inherit this).
        SetConsoleCtrlHandler(null, true);
        try
        {
            return GenerateConsoleCtrlEvent(CTRL_C_EVENT, 0);
        }
        finally
        {
            FreeConsole();
        }
    }
}
