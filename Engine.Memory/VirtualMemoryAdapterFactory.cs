namespace Squalr.Engine.Memory
{
    using Squalr.Engine.Output;
    using System;
    using System.Threading;
    using Windows;

    /// <summary>
    /// Factory for obtaining an object that allows access to the underlying virtual memory of a process.
    /// </summary>
    public class VirtualMemoryAdapterFactory
    {
        /// <summary>
        /// Singleton instance of the <see cref="WindowsAdapter"/> class
        /// </summary>
        private static Lazy<WindowsAdapter> windowsAdapterInstance = new Lazy<WindowsAdapter>(
            () => { return new WindowsAdapter(); },
            LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// Gets an adapter to the virtual memory of another process.
        /// </summary>
        /// <returns>An adapter that provides access to the virtual memory of a process.</returns>
        public static IVirtualMemoryAdapter GetVirtualMemoryAdapter()
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