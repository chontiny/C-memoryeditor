namespace Squalr.Engine.Debugger
{
    using Squalr.Engine.Debugger.Windows.DebugEngine;
    using Squalr.Engine.Output;
    using System;
    using System.Threading;

    /// <summary>
    /// Factory for obtaining an object that enables debugging of a process.
    /// </summary>
    public class DebuggerFactory
    {
        /// <summary>
        /// Singleton instance of the <see cref="DebugEngine"/> class
        /// </summary>
        private static Lazy<DebugEngine> windowsDebuggerInstance = new Lazy<DebugEngine>(
            () => { return new DebugEngine(); },
            LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// Gets an object that enables debugging of a process.
        /// </summary>
        /// <returns>An object that enables debugging of a process.</returns>
        public static IDebugger GetDebugger()
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
                    return windowsDebuggerInstance.Value;
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