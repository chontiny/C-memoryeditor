namespace SqualrProxy
{
    using Squalr.PipeDream;
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Proxy service to be contained by a 32 and 64 bit service, with services exposed via IPC. Useful for certain things that
    /// Squalr requires, such as:
    /// - FASM Compiler, which can only be run in 32 bit mode
    /// - Microsoft.Diagnostics.Runtime, which can only be used on processes of the same bitness
    /// </summary>
    public class SqualrProxy
    {
        /// <summary>
        /// The delay in milliseconds to check if the parent process is still running.
        /// </summary>
        private const Int32 ParentCheckDelayMs = 500;

        public SqualrProxy(int parentProcessId, string pipeName)
        {
            Console.WriteLine("SERVER " + (Environment.Is64BitProcess ? "64" : "32"));
            Console.WriteLine("-----------------------------");
            Console.WriteLine("Pipe: " + pipeName);

            this.InitializeAutoExit(parentProcessId);

            IProxyAssembler instance = new ProxyAssembler();
            PipeDream.ServerInitialize<IProxyAssembler>(instance, pipeName);
        }

        /// <summary>
        /// Runs a loop constantly checking if the parent process still exists. This service closes when the parent is closed.
        /// </summary>
        /// <param name="parentProcessId">The process id of the parent process.</param>
        private void InitializeAutoExit(Int32 parentProcessId)
        {
            Task.Run(() =>
            {
                Console.WriteLine("Initializing auto-exit");

                while (true)
                {
                    try
                    {
                        // Check if the process is still running
                        Process process = Process.GetProcessById(parentProcessId);

                        // Could not find process
                        if (process == null || process.HasExited)
                        {
                            break;
                        }
                    }
                    catch (Exception)
                    {
                        // Could not find process
                        break;
                    }

                    Thread.Sleep(SqualrProxy.ParentCheckDelayMs);
                }

                Console.WriteLine("Parent process not found -- exiting");
                Environment.Exit(0);
            });
        }
    }
    //// End class
}
//// End namespace