namespace Squalr.Engine.Proxy
{
    using SqualrProxy;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Threading;


    /// <summary>
    /// Communicates with proxy services. These issue commands that require 32 or 64 bit specifically.
    /// </summary>
    internal class ProxyCommunicator
    {
        /// <summary>
        /// The 32 bit proxy service executable
        /// </summary>
        private const String Proxy32Executable = "Engine.Internal.Proxy32.exe";

        /// <summary>
        /// The 64 bit proxy service executable
        /// </summary>
        private const String Proxy64Executable = "Engine.Internal.Proxy64.exe";

        /// <summary>
        /// Singleton instance of the <see cref="ProxyCommunicator" /> class
        /// </summary>
        private static Lazy<ProxyCommunicator> proxyCommunicatorInstance = new Lazy<ProxyCommunicator>(
            () => { return new ProxyCommunicator(); },
            LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// Prevents a default instance of the <see cref="ProxyCommunicator" /> class from being created
        /// </summary>
        private ProxyCommunicator()
        {
            // Create random pipe name
            string pipeName32 = PipeDream.PipeDream.GetUniquePipeName();
            string pipeName64 = PipeDream.PipeDream.GetUniquePipeName();

            // Start the 64 bit remote process
            this.StartServer(ProxyCommunicator.Proxy32Executable, pipeName32);
            this.StartServer(ProxyCommunicator.Proxy64Executable, pipeName64);

            // Start 32 and 64 bit proxy services
            try
            {
                this.Proxy32 = PipeDream.PipeDream.ClientInitialize<IProxyAssembler>(pipeName32);
                Output.Output.Log(Output.LogLevel.Info, "Initialized 32 bit proxy service over pipe: " + pipeName32);
            }
            catch (Exception ex)
            {
                Output.Output.Log(Output.LogLevel.Fatal, "Error initializing 32 bit proxy service over pipe: " + pipeName32, ex);
            }

            try
            {
                this.Proxy64 = PipeDream.PipeDream.ClientInitialize<IProxyAssembler>(pipeName64);
                Output.Output.Log(Output.LogLevel.Info, "Initialized 64 bit proxy service over pipe: " + pipeName64);
            }
            catch (Exception ex)
            {
                Output.Output.Log(Output.LogLevel.Fatal, "Error initializing 64 bit proxy service over pipe: " + pipeName64, ex);
            }
        }

        /// <summary>
        /// Gets or sets the 32 bit proxy service.
        /// </summary>
        private IProxyAssembler Proxy32 { get; set; }

        /// <summary>
        /// Gets or sets the 64 bit proxy service.
        /// </summary>
        private IProxyAssembler Proxy64 { get; set; }

        /// <summary>
        /// Gets a singleton instance of the <see cref="ProxyCommunicator"/> class.
        /// </summary>
        /// <returns>A singleton instance of the class.</returns>
        public static ProxyCommunicator GetInstance()
        {
            return proxyCommunicatorInstance.Value;
        }

        /// <summary>
        /// Gets the proxy service based on provided parameters.
        /// </summary>
        /// <param name="is32Bit">Whether or not to get the 32 or 64 bit service.</param>
        /// <returns>The 32 or 64 bit service.</returns>
        public IProxyAssembler GetProxyService(Boolean is32Bit)
        {
            if (is32Bit)
            {
                return this.Proxy32;
            }
            else
            {
                return this.Proxy64;
            }
        }

        /// <summary>
        /// Starts a proxy service.
        /// </summary>
        /// <param name="executableName">The executable name of the service to start.</param>
        /// <param name="pipeName">The pipe name for IPC.</param>
        /// <returns>The proxy service that is created.</returns>
        private void StartServer(string executableName, string pipeName)
        {
            try
            {
                // Start the proxy service
                string exePath = escape(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), executableName));
                ProcessStartInfo processInfo = new ProcessStartInfo(exePath);
                processInfo.Arguments = Process.GetCurrentProcess().Id.ToString() + " " + pipeName;
                processInfo.UseShellExecute = false;
                processInfo.CreateNoWindow = true;
                Process.Start(processInfo);
            }
            catch (Exception ex)
            {
                Output.Output.Log(Output.LogLevel.Fatal, "Error starting proxy service, some features will not be available.", ex);
            }
        }

        private static string escape(string str)
        {
            return string.Format("\"{0}\"", str);
        }
    }
    //// End class
}
//// End namespace