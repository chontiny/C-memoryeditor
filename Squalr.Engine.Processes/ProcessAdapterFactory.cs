namespace Squalr.Engine.Processes
{
    using Squalr.Engine.Output;
    using Squalr.Engine.Processes.Windows;
    using System;
    using System.Threading;

    /// <summary>
    /// A class responsible for collecting all running processes on the system.
    /// </summary>
    public class ProcessAdapterFactory
    {
        /// <summary>
        /// Singleton instance of the <see cref="WindowsAdapter"/> class
        /// </summary>
        private static Lazy<WindowsAdapter> windowsAdapterInstance = new Lazy<WindowsAdapter>(
            () => { return new WindowsAdapter(); },
            LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// Gets an adapter to process manipulation.
        /// </summary>
        /// <returns>An adapter that provides access to the virtual memory of a process.</returns>
        public static IProcessAdapter GetProcessAdapter()
        {
            OperatingSystem os = Environment.OSVersion;
            PlatformID platformid = os.Platform;
            Exception ex;

            switch (platformid)
            {
                case PlatformID.Win32NT:
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                case PlatformID.WinCE:
                    return windowsAdapterInstance.Value;
                case PlatformID.Unix:
                    ex = new Exception("Unix operating system is not supported");
                    break;
                case PlatformID.MacOSX:
                    ex = new Exception("MacOSX operating system is not supported");
                    break;
                default:
                    ex = new Exception("Unknown operating system");
                    break;
            }

            Output.Log(LogLevel.Fatal, "Unsupported Operating System", ex);
            throw ex;
        }
    }
    //// End class
}
//// End namespace