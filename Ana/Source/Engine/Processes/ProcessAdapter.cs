namespace Ana.Source.Engine.Processes
{
    using OperatingSystems.Windows.Native;
    using Output;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.Linq;
    using System.Runtime.InteropServices;
    using Utils.DataStructures;

    /// <summary>
    /// A class responsible for collecting all running processes on the system.
    /// TODO: Icon fetching and thread enumeration are native calls and should be placed in the Windows OS adapter.
    /// </summary>
    internal class ProcessAdapter : IProcesses
    {
        /// <summary>
        /// Thread safe collection of listeners.
        /// </summary>
        private ConcurrentHashSet<IProcessObserver> processListeners;

        /// <summary>
        /// Collection of process ids that have caused access issues.
        /// </summary>
        private ConcurrentHashSet<Int32> systemProcessCache;

        /// <summary>
        /// Collection of process ids for which an icon could not be fetched.
        /// </summary>
        private ConcurrentHashSet<Int32> noIconProcessCache;

        /// <summary>
        /// Collection of processes with a window.
        /// </summary>
        private ConcurrentHashSet<Int32> windowedProcessCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessAdapter" /> class.
        /// </summary>
        public ProcessAdapter()
        {
            this.processListeners = new ConcurrentHashSet<IProcessObserver>();
            this.systemProcessCache = new ConcurrentHashSet<Int32>();
            this.noIconProcessCache = new ConcurrentHashSet<Int32>();
            this.windowedProcessCache = new ConcurrentHashSet<Int32>();
        }

        /// <summary>
        /// Gets or sets the the opened process.
        /// </summary>
        private NormalizedProcess OpenedProcess { get; set; }

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
        /// Gets all running processes on the system.
        /// </summary>
        /// <returns>An enumeration of see <see cref="NormalizedProcess" />.</returns>
        public IEnumerable<NormalizedProcess> GetProcesses()
        {
            return Process.GetProcesses()
                .Select(process => new NormalizedProcess(
                        process.Id,
                        process.ProcessName,
                        this.IsProcessSystemProcess(process) ? DateTime.MinValue : process.StartTime,
                        this.IsProcessSystemProcess(process),
                        this.isProcessWindowed(process),
                        this.GetIcon(process)))
                .OrderByDescending(normalizedProcess => normalizedProcess.StartTime);
        }

        /// <summary>
        /// Opens a process for editing.
        /// </summary>
        /// <param name="process">The process to be opened.</param>
        public void OpenProcess(NormalizedProcess process)
        {
            this.OpenedProcess = process;

            if (process != null)
            {
                OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Info, "Attached to process: " + process.ProcessName);
            }
            else
            {
                OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Warn, "Detached from target process");
            }

            if (this.processListeners != null)
            {
                foreach (IProcessObserver listener in this.processListeners)
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
            if (EngineCore.GetInstance().OperatingSystemAdapter.IsOperatingSystem32Bit())
            {
                return true;
            }

            return EngineCore.GetInstance().OperatingSystemAdapter.IsProcess32Bit(this.OpenedProcess);
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
        /// Determines if the provided process is a system process.
        /// </summary>
        /// <param name="externalProcess">The process to check.</param>
        /// <returns>A value indicating whether or not the given process is a system process.</returns>
        private Boolean IsProcessSystemProcess(Process externalProcess)
        {
            if (systemProcessCache.Contains(externalProcess.Id))
            {
                return true;
            }

            if (externalProcess.SessionId == 0 || externalProcess.BasePriority == 13)
            {
                systemProcessCache.Add(externalProcess.Id);
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
                systemProcessCache.Add(externalProcess.Id);
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
            // Step 0: Check if we have already cached the result of this process.
            if (windowedProcessCache.Contains(externalProcess.Id))
            {
                return true;
            }

            // Step 1: Check if there is a window handle
            if (externalProcess.MainWindowHandle != IntPtr.Zero)
            {
                return true;
            }

            // Step 2: Enumerate threads, looking for window threads that reference visible windows
            foreach (ProcessThread threadInfo in externalProcess.Threads)
            {
                IntPtr[] windows = GetWindowHandlesForThread(threadInfo.Id);

                if (windows != null)
                {
                    foreach (IntPtr handle in windows)
                    {
                        if (IsWindowVisible(handle))
                        {
                            windowedProcessCache.Add(externalProcess.Id);
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private IntPtr[] GetWindowHandlesForThread(Int32 threadHandle)
        {
            results.Clear();
            EnumWindows(WindowEnum, threadHandle);

            return results.ToArray();
        }

        private delegate Int32 EnumWindowsProc(IntPtr hwnd, Int32 lParam);

        [DllImport("user32")]
        private static extern Int32 EnumWindows(EnumWindowsProc x, Int32 y);
        [DllImport("user32")]
        public static extern Int32 GetWindowThreadProcessId(IntPtr handle, out Int32 processId);
        [DllImport("user32")]
        static extern Boolean IsWindowVisible(IntPtr hWnd);

        private List<IntPtr> results = new List<IntPtr>();

        private Int32 WindowEnum(IntPtr hWnd, Int32 lParam)
        {
            Int32 processID = 0;
            Int32 threadID = GetWindowThreadProcessId(hWnd, out processID);
            if (threadID == lParam)
            {
                results.Add(hWnd);
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
            const Icon NoIcon = null;

            if (this.noIconProcessCache.Contains(externalProcess.Id))
            {
                return NoIcon;
            }

            if (this.IsProcessSystemProcess(externalProcess))
            {
                this.noIconProcessCache.Add(externalProcess.Id);
                return NoIcon;
            }

            try
            {
                IntPtr iconHandle = NativeMethods.ExtractIcon(externalProcess.Handle, externalProcess.MainModule.FileName, 0);

                if (iconHandle == IntPtr.Zero)
                {
                    this.noIconProcessCache.Add(externalProcess.Id);
                    return NoIcon;
                }

                return Icon.FromHandle(iconHandle);
            }
            catch
            {
                this.noIconProcessCache.Add(externalProcess.Id);
                return NoIcon;
            }
        }
    }
    //// End class
}
//// End namespace