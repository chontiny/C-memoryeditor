namespace Squalr.Engine.Memory
{
    using Squalr.Engine.Memory.Windows;
    using System;
    using System.Threading;

    public static class Reader
    {
        /// <summary>
        /// Singleton instance of the <see cref="WindowsMemoryReader"/> class.
        /// </summary>
        private static Lazy<WindowsMemoryReader> windowsMemoryReaderInstance = new Lazy<WindowsMemoryReader>(
            () => { return new WindowsMemoryReader(); },
            LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// Gets the default memory reader for the current operating system.
        /// </summary>
        public static IMemoryReader Default
        {
            get
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
                        return windowsMemoryReaderInstance.Value;
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

                Output.Output.Log(Output.LogLevel.Fatal, "Unsupported Operating System", ex);
                throw ex;
            }
        }

        /// <summary>
        /// Gets the memory reader for the windows operating system.
        /// </summary>
        public static IMemoryReader Windows
        {
            get
            {
                return windowsMemoryReaderInstance.Value;
            }
        }
    }
    //// End class
}
//// End namespace