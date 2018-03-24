namespace Squalr.Engine.Processes.Windows
{
    using Squalr.Engine.Output;
    using Squalr.Engine.Processes.Windows.Native;
    using Squalr.Engine.TaskScheduler;
    using Squalr.Engine.Utils.DataStructures;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.Linq;
    using System.Threading;

    /// <summary>
    /// A class responsible for collecting all running processes on a Windows system.
    /// </summary>
    internal class WindowsAdapter : ScheduledTask, IProcessAdapter, IProcessObserver
    {
        /// <summary>
        /// Thread safe collection of listeners.
        /// </summary>
        private ConcurrentHashSet<IProcessObserver> processListeners;

        /// <summary>
        /// A reference to target process.
        /// </summary>
        private Process systemProcess;

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowsAdapter" /> class.
        /// </summary>
        public WindowsAdapter() : base(
            taskName: "Check Process Alive",
            isRepeated: true,
            trackProgress: false)
        {
            this.processListeners = new ConcurrentHashSet<IProcessObserver>();

            this.Results = new List<IntPtr>();
            this.SystemProcessCache = new TtlCache<Int32>(TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(15));
            this.NoIconProcessCache = new TtlCache<Int32>(TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(15));
            this.WindowedProcessCache = new TtlCache<Int32>(TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(15));
            this.NoWindowProcessCache = new TtlCache<Int32>(TimeSpan.FromSeconds(15), TimeSpan.FromSeconds(5));

            // TODO: For now we will not expire the TTL icons. This may cause cosmetic bugs. Icons are currently not disposed,
            // so putting a TTL on this would cause a memory leak.
            this.IconCache = new TtlCache<Int32, Icon>();

            // Subscribe to process events
            this.Subscribe(this);

            this.Start();
        }

        /// <summary>
        /// Gets a reference to the target process. This is an optimization to minimize accesses to the Processes component of the Engine.
        /// </summary>
        public Process SystemProcess
        {
            get
            {
                return this.systemProcess;
            }

            private set
            {
                this.systemProcess = value;
            }
        }

        private List<IntPtr> Results;

        /// <summary>
        /// Collection of process ids that have caused access issues.
        /// </summary>
        private TtlCache<Int32> SystemProcessCache;

        /// <summary>
        /// Collection of process ids for which an icon could not be fetched.
        /// </summary>
        private TtlCache<Int32> NoIconProcessCache;

        /// <summary>
        /// Collection of icons fetched from processes.
        /// </summary>
        private TtlCache<Int32, Icon> IconCache;

        /// <summary>
        /// Collection of processes with a window.
        /// </summary>
        private TtlCache<Int32> WindowedProcessCache;

        /// <summary>
        /// Collection of processes without a window.
        /// </summary>
        private TtlCache<Int32> NoWindowProcessCache;

        /// <summary>
        /// Gets or sets the the opened process.
        /// </summary>
        private NormalizedProcess OpenedProcess { get; set; }

        /// <summary>
        /// Represents an empty icon;
        /// </summary>
        private const Icon NoIcon = null;

        /// <summary>
        /// Subscribes the listener to process change events.
        /// </summary>
        /// <param name="listener">The object that wants to listen to process update events.</param>
        public void Subscribe(IProcessObserver listener)
        {
            this.processListeners.Add(listener);
        }

        /// <summary>
        /// Unsubscribes the listener from process change events.
        /// </summary>
        /// <param name="listener">The object that wants to listen to process update events.</param>
        public void Unsubscribe(IProcessObserver listener)
        {
            this.processListeners?.Remove(listener);
        }

        /// <summary>
        /// Recieves a process update. This is an optimization over grabbing the process from the <see cref="IProcessAdapter"/> component
        /// of the <see cref="EngineCore"/> every time we need it, which would be cumbersome when doing hundreds of thousands of memory read/writes.
        /// </summary>
        /// <param name="process">The newly selected process.</param>
        public void Update(NormalizedProcess process)
        {
            if (process == null)
            {
                // Avoid setter functions
                this.systemProcess = null;
                return;
            }

            try
            {
                this.SystemProcess = Process.GetProcessById(process.ProcessId);
            }
            catch
            {
                // Avoid setter functions
                this.systemProcess = null;
            }
        }

        /// <summary>
        /// Closes a process for editing.
        /// </summary>
        public void CloseProcess()
        {
            if (this.OpenedProcess != null)
            {
                Output.Log(LogLevel.Info, "Detached from target process");
            }

            this.OpenProcess(null);
        }

        /// <summary>
        /// Opens a process for editing.
        /// </summary>
        /// <param name="process">The process to be opened.</param>
        public void OpenProcess(NormalizedProcess process)
        {
            if (process != null)
            {
                Output.Log(LogLevel.Info, "Attached to process: " + process.ProcessName);
            }

            this.OpenedProcess = process;

            if (this.processListeners != null)
            {
                foreach (IProcessObserver listener in this.processListeners.ToList())
                {
                    listener.Update(process);
                }
            }
        }

        /// <summary>
        /// Gets the process that has been opened.
        /// </summary>
        /// <returns>The opened process.</returns>
        public NormalizedProcess GetOpenedProcess()
        {
            return this.OpenedProcess;
        }

        /// <summary>
        /// Determines if the opened process is 32 bit.
        /// </summary>
        /// <returns>Returns true if the opened process is 32 bit, otherwise false.</returns>
        public Boolean IsOpenedProcess32Bit()
        {
            // First do the simple check if seeing if the OS is 32 bit, in which case the process wont be 64 bit
            if (this.IsOperatingSystem32Bit())
            {
                return true;
            }

            return this.IsProcess32Bit(this.OpenedProcess);
        }

        /// <summary>
        /// Determines if the opened process is 64 bit.
        /// </summary>
        /// <returns>Returns true if the opened process is 64 bit, otherwise false.</returns>
        public Boolean IsOpenedProcess64Bit()
        {
            return !this.IsOpenedProcess32Bit();
        }

        /// <summary>
        /// Gets all running processes on the system.
        /// </summary>
        /// <returns>An enumeration of see <see cref="NormalizedProcess" />.</returns>
        public IEnumerable<NormalizedProcess> GetProcesses()
        {
            return Process.GetProcesses()
                .Select(process =>
                    new
                    {
                        baseProcess = process,
                        isSystemProcess = this.IsProcessSystemProcess(process),
                        isProcessWindowed = this.isProcessWindowed(process),
                        icon = this.GetIcon(process),
                    })
                .Select(intermediateProcess => new NormalizedProcess(
                        intermediateProcess.baseProcess.Id,
                        intermediateProcess.baseProcess.ProcessName,
                        intermediateProcess.isSystemProcess ? DateTime.MinValue : intermediateProcess.baseProcess.StartTime,
                        intermediateProcess.isSystemProcess,
                        intermediateProcess.isProcessWindowed,
                        intermediateProcess.icon))
                .OrderByDescending(normalizedProcess => normalizedProcess.StartTime);
        }

        /// <summary>
        /// Determines if the provided process is a system process.
        /// </summary>
        /// <param name="externalProcess">The process to check.</param>
        /// <returns>A value indicating whether or not the given process is a system process.</returns>
        private Boolean IsProcessSystemProcess(Process externalProcess)
        {
            if (this.SystemProcessCache.Contains(externalProcess.Id))
            {
                return true;
            }

            if (externalProcess.SessionId == 0 || externalProcess.BasePriority == 13)
            {
                this.SystemProcessCache.Add(externalProcess.Id);
                return true;
            }

            try
            {
                if (externalProcess.PriorityBoostEnabled)
                {
                    // Accessing this field will cause an access exception for system processes. This saves
                    // time because handling the exception is faster than failing to fetch the icon later
                    return false;
                }
            }
            catch
            {
                this.SystemProcessCache.Add(externalProcess.Id);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Determines if a process has a window.
        /// </summary>
        /// <param name="externalProcess">The process to check.</param>
        /// <returns>A value indicating whether or not the given process has a window.</returns>
        private Boolean isProcessWindowed(Process externalProcess)
        {
            if (this.WindowedProcessCache.Contains(externalProcess.Id))
            {
                return true;
            }

            if (this.NoWindowProcessCache.Contains(externalProcess.Id))
            {
                return false;
            }

            // Check if the window handle is set
            if (externalProcess.MainWindowHandle != IntPtr.Zero)
            {
                this.WindowedProcessCache.Add(externalProcess.Id);
                return true;
            }

            // Ignore system processes
            if (this.IsProcessSystemProcess(externalProcess))
            {
                this.NoWindowProcessCache.Add(externalProcess.Id);
                return false;
            }

            // Window handle was not set, so to be certain we must enumerate the process threads, looking for window threads
            foreach (ProcessThread threadInfo in externalProcess.Threads)
            {
                IntPtr[] windows = this.GetWindowHandlesForThread(threadInfo.Id);

                if (windows != null)
                {
                    foreach (IntPtr handle in windows)
                    {
                        if (NativeMethods.IsWindowVisible(handle))
                        {
                            this.WindowedProcessCache.Add(externalProcess.Id);
                            return true;
                        }
                    }
                }
            }

            this.NoWindowProcessCache.Add(externalProcess.Id);
            return false;
        }

        private IntPtr[] GetWindowHandlesForThread(Int32 threadHandle)
        {
            this.Results.Clear();
            NativeMethods.EnumWindows(WindowEnum, threadHandle);

            return Results.ToArray();
        }

        private Int32 WindowEnum(IntPtr hWnd, Int32 lParam)
        {
            Int32 processID = 0;
            Int32 threadID = NativeMethods.GetWindowThreadProcessId(hWnd, out processID);

            if (threadID == lParam)
            {
                this.Results.Add(hWnd);
            }

            return 1;
        }

        /// <summary>
        /// Fetches the icon associated with the provided process.
        /// </summary>
        /// <param name="intermediateProcess">An intermediate process structure.</param>
        /// <returns>An Icon associated with the given process. Returns null if there is no icon.</returns>
        private Icon GetIcon(Process externalProcess)
        {
            Icon icon = null;

            if (this.NoIconProcessCache.Contains(externalProcess.Id))
            {
                return WindowsAdapter.NoIcon;
            }

            if (this.IconCache.TryGetValue(externalProcess.Id, out icon))
            {
                return icon;
            }

            if (this.IsProcessSystemProcess(externalProcess))
            {
                this.NoIconProcessCache.Add(externalProcess.Id);
                return WindowsAdapter.NoIcon;
            }

            try
            {
                IntPtr iconHandle = NativeMethods.ExtractIcon(externalProcess.Handle, externalProcess.MainModule.FileName, 0);

                if (iconHandle == IntPtr.Zero)
                {
                    this.NoIconProcessCache.Add(externalProcess.Id);
                    return WindowsAdapter.NoIcon;
                }

                icon = Icon.FromHandle(iconHandle);
                this.IconCache.Add(externalProcess.Id, icon);

                return icon;
            }
            catch
            {
                this.NoIconProcessCache.Add(externalProcess.Id);
                return WindowsAdapter.NoIcon;
            }
        }

        /// <summary>
        /// Determines if this program is 32 bit
        /// </summary>
        /// <returns>A boolean indicating if this program is 32 bit or not</returns>
        public Boolean IsSelf32Bit()
        {
            return !Environment.Is64BitProcess;
        }

        /// <summary>
        /// Determines if this program is 64 bit
        /// </summary>
        /// <returns>A boolean indicating if this program is 64 bit or not</returns>
        public Boolean IsSelf64Bit()
        {
            return Environment.Is64BitProcess;
        }

        /// <summary>
        /// Determines if a process is 32 bit
        /// </summary>
        /// <param name="process">The process to check</param>
        /// <returns>Returns true if the process is 32 bit, otherwise false</returns>
        public Boolean IsProcess32Bit(NormalizedProcess process)
        {
            Boolean isWow64;

            // First do the simple check if seeing if the OS is 32 bit, in which case the process wont be 64 bit
            if (this.IsOperatingSystem32Bit())
            {
                return true;
            }

            // No process provided, assume 32 bit
            if (process == null)
            {
                return true;
            }

            try
            {
                if (this.SystemProcess == null || !NativeMethods.IsWow64Process(this.SystemProcess.Handle, out isWow64))
                {
                    // Error, assume 32 bit
                    return true;
                }
            }
            catch
            {
                // Error, assume 32 bit
                return true;
            }

            return isWow64;
        }

        /// <summary>
        /// Determines if a process is 64 bit
        /// </summary>
        /// <param name="process">The process to check</param>
        /// <returns>Returns true if the process is 64 bit, otherwise false</returns>
        public Boolean IsProcess64Bit(NormalizedProcess process)
        {
            return !this.IsProcess32Bit(process);
        }

        /// <summary>
        /// Determines if the operating system is 32 bit
        /// </summary>
        /// <returns>A boolean indicating if the OS is 32 bit or not</returns>
        public Boolean IsOperatingSystem32Bit()
        {
            return !Environment.Is64BitOperatingSystem;
        }

        /// <summary>
        /// Determines if the operating system is 64 bit
        /// </summary>
        /// <returns>A boolean indicating if the OS is 64 bit or not</returns>
        public Boolean IsOperatingSystem64Bit()
        {
            return Environment.Is64BitOperatingSystem;
        }

        /// <summary>
        /// Called when the scheduled task starts.
        /// </summary>
        protected override void OnBegin()
        {
        }

        /// <summary>
        /// Called when the scheduled task is updated.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token for handling canceled tasks.</param>
        protected override void OnUpdate(CancellationToken cancellationToken)
        {
            try
            {
                if (this.SystemProcess?.HasExited ?? false)
                {
                    this.CloseProcess();
                    this.SystemProcess = null;
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// Called when the repeated task completes.
        /// </summary>
        protected override void OnEnd()
        {
        }
    }
    //// End class
}
//// End namespace