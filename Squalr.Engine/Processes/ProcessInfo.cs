namespace Squalr.Engine.Processes
{
    using Squalr.Engine.Logging;
    using Squalr.Engine.Processes.Windows;
    using System;
    using System.Threading;

    public class ProcessInfo
    {
        /// <summary>
        /// Singleton instance of the <see cref="WindowsProcessInfo"/> class.
        /// </summary>
        private static Lazy<IProcessInfo> windowsProcessInfoInstance = new Lazy<IProcessInfo>(
            () => { return new WindowsProcessInfo(); },
            LazyThreadSafetyMode.ExecutionAndPublication);

        public static IProcessInfo Default
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
                        return windowsProcessInfoInstance.Value;
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

                Logger.Log(LogLevel.Fatal, "Unsupported Operating System", ex);
                throw ex;
            }
        }

        public static IProcessInfo Windows
        {
            get
            {
                return windowsProcessInfoInstance.Value;
            }
        }
    }
    //// End class
}
//// End namespace