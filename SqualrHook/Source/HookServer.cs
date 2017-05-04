namespace SqualrHookServer.Source
{
    using EasyHook;
    using SqualrHookClient.Source;
    using SqualrHookServer.Source.Graphics;
    using SqualrHookServer.Source.Network;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Entry point for a hook in the target process. Automatically loads when RemoteHooking.Inject() is called.
    /// </summary>
    public class HookServer : IEntryPoint
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HookServer" /> class.
        /// </summary>
        /// <param name="context">Easyhook remoting context.</param>
        /// <param name="channelName">IPC channel name.</param>
        /// <param name="projectDirectory">The project directory to use when loading content.</param>
        public HookServer(RemoteHooking.IContext context, String channelName)
        {
            // Get reference to IPC to host application
            this.HookClient = RemoteHooking.IpcConnectClient<HookClientBase>(channelName);

            // We try to call the client immediately -- if this fails then the injection fails
            this.HookClient.Log("Hook successfully connected");
        }

        /// <summary>
        /// Gets or sets the graphics hook.
        /// </summary>
        private GraphicsHook GraphicsHook { get; set; }

        /// <summary>
        /// Gets or sets the network hook.
        /// </summary>
        private NetworkHook NetworkHook { get; set; }

        /// <summary>
        /// Gets or sets the hook client to control the hook.
        /// </summary>
        private IHookClient HookClient { get; set; }

        /// <summary>
        /// Gets or sets the cancellation token for the client pinging task.
        /// </summary>
        private CancellationTokenSource CancelRequest { get; set; }

        /// <summary>
        /// Gets or sets the task to detect if the client is still running.
        /// </summary>
        private ManualResetEvent TaskRunning { get; set; }

        /// <summary>
        /// Entry point for the server hook.
        /// </summary>
        /// <param name="context">Easyhook remoting context.</param>
        /// <param name="channelName">IPC channel name.</param>
        /// <param name="projectDirectory">The project directory to use when loading content.</param>
        public void Run(RemoteHooking.IContext context, String channelName)
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

                this.NetworkHook = new NetworkHook(this.HookClient);

                // We start a thread here to periodically check if the host is still running
                // If the host process stops then we will automatically uninstall the hooks
                this.MaintainConnection();
            }
            catch
            {
            }
            finally
            {
                this.HookClient.Log("Detaching hooks");

                // Always sleep long enough for any remaining messages to complete sending
                Thread.Sleep(100);
            }
        }

        /// <summary>
        /// Begin a background thread to check periodically that the host process is still accessible on its IPC channel.
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