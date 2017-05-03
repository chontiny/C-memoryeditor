namespace AnathenaHook.Server
{
    using Client;
    using EasyHook;
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting.Channels;
    using System.Runtime.Remoting.Channels.Ipc;
    using System.Runtime.Serialization.Formatters;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Entry point for a hook in the target process. Automatically loads when RemoteHooking.Inject() is called.
    /// </summary>
    internal class HookServer : IEntryPoint
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HookServer" /> class
        /// </summary>
        /// <param name="context">Easyhook remoting context</param>
        /// <param name="channelName">IPC channel name</param>
        /// <param name="projectDirectory">The Ana project directory to use when loading content</param>
        public HookServer(RemoteHooking.IContext context, String channelName, String projectDirectory)
        {
            this.IpcServerChannel = null;

            // Get reference to IPC to host application
            this.HookClient = RemoteHooking.IpcConnectClient<HookClient>(channelName);

            // We try to ping immediately, if it fails then injection fails
            this.HookClient.Ping();

            // Attempt to create a IpcServerChannel so that any event handlers on the client will function correctly
            IDictionary properties = new Hashtable();
            properties["name"] = channelName;
            properties["portName"] = channelName + Guid.NewGuid().ToString("N");

            BinaryServerFormatterSinkProvider binaryServerFormatterSinkProvider = new BinaryServerFormatterSinkProvider();
            binaryServerFormatterSinkProvider.TypeFilterLevel = TypeFilterLevel.Full;

            IpcServerChannel clientServerChannel = new IpcServerChannel(properties, binaryServerFormatterSinkProvider);
            ChannelServices.RegisterChannel(clientServerChannel, false);
        }

        /// <summary>
        /// Gets or sets the channel for inter-process communication
        /// </summary>
        private IpcServerChannel IpcServerChannel { get; set; }

        /// <summary>
        /// Gets or sets the hook client to control the hook
        /// </summary>
        private IHookClient HookClient { get; set; }

        /// <summary>
        /// Gets or sets the cancellation token for the client pinging task
        /// </summary>
        private CancellationTokenSource CancelRequest { get; set; }

        /// <summary>
        /// Gets or sets the task to detect if the client is still running
        /// </summary>
        private ManualResetEvent TaskRunning { get; set; }

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetModuleHandle(String moduleName);

        /// <summary>
        /// Entry point for the server hook
        /// </summary>
        /// <param name="context">Easyhook remoting context</param>
        /// <param name="channelName">IPC channel name</param>
        /// <param name="projectDirectory">The Ana project directory to use when loading content</param>
        public void Run(RemoteHooking.IContext context, String channelName, String projectDirectory)
        {
            // When not using GAC there can be issues with remoting assemblies resolving correctly
            // this is a workaround that ensures that the current assembly is correctly associated
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.AssemblyResolve += (sender, args) =>
            {
                return this.GetType().Assembly.FullName == args.Name ? this.GetType().Assembly : null;
            };

            this.TaskRunning = new ManualResetEvent(false);
            this.TaskRunning.Reset();

            try
            {
                // We start a thread here to periodically check if the host is still running
                // If the host process stops then we will automatically uninstall the hooks
                this.MaintainConnection();
            }
            catch
            {
            }
            finally
            {
                // Remove the client server channel (that allows client event handlers)
                ChannelServices.UnregisterChannel(this.IpcServerChannel);

                // Always sleep long enough for any remaining messages to complete sending
                Thread.Sleep(100);
            }
        }

        /// <summary>
        /// Begin a background thread to check periodically that the host process is still accessible on its IPC channel
        /// </summary>
        private void MaintainConnection()
        {
            this.CancelRequest = new CancellationTokenSource();

            Task.Run(
            async () =>
            {
                while (true)
                {
                    try
                    {
                        this.HookClient.Ping();
                    }
                    catch
                    {
                        this.TaskRunning.Set();
                    }

                    // Await with cancellation
                    await Task.Delay(1000, this.CancelRequest.Token);
                }
            },
            this.CancelRequest.Token);

            // Wait until task is no longer running
            this.TaskRunning.WaitOne();

            // Cancel task to ensure it ends
            this.CancelRequest?.Cancel();
        }
    }
    //// End class
}
//// End namespace