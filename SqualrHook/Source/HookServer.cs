namespace SqualrHookServer.Source
{
    using EasyHook;
    using SqualrHookClient.Source;
    using SqualrHookServer.Source.Graphics;
    using SqualrHookServer.Source.Network;
    using System;
    using System.Runtime.Remoting.Channels;
    using System.Runtime.Remoting.Channels.Ipc;
    using System.Runtime.Serialization.Formatters;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Entry point for a hook in the target process. Automatically loads when RemoteHooking.Inject() is called.
    /// </summary>
    [Serializable]
    public class HookServer : IEntryPoint
    {
        [NonSerialized]
        private CancellationTokenSource cancelRequest;

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

            // Call the client immediately to test a successful connection
            this.HookClient.Log("Hook successfully connected");

            // Attempt to create a IpcServerChannel so that any event handlers on the client will function correctly
            System.Collections.IDictionary properties = new System.Collections.Hashtable();
            properties["name"] = channelName;
            properties["portName"] = channelName + Guid.NewGuid().ToString("N");

            BinaryServerFormatterSinkProvider binaryProv = new BinaryServerFormatterSinkProvider();
            binaryProv.TypeFilterLevel = TypeFilterLevel.Full;

            IpcServerChannel clientServerChannel = new IpcServerChannel(properties, binaryProv);
            ChannelServices.RegisterChannel(clientServerChannel, false);
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
        private HookClientBase HookClient { get; set; }

        /// <summary>
        /// Gets or sets the cancellation token for the client pinging task.
        /// </summary>
        private CancellationTokenSource CancelRequest
        {
            get
            {
                return this.cancelRequest;
            }

            set
            {
                this.cancelRequest = value;
            }
        }

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
            try
            {
                // When not using GAC there can be issues with remoting assemblies resolving correctly
                // this is a workaround that ensures that the current assembly is correctly associated
                AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
                {
                    return this.GetType().Assembly.FullName == args.Name ? this.GetType().Assembly : null;
                };

                this.TaskRunning = new ManualResetEvent(false);
                this.TaskRunning.Reset();

                this.HookClient.Disconnected += () =>
                {
                    this.TaskRunning.Set();
                };

                this.NetworkHook = new NetworkHook(this.HookClient);

                // We start a thread here to periodically check if the host is still running
                // If the host process stops then we will automatically uninstall the hooks
                this.MaintainConnection();
            }
            catch (Exception ex)
            {
                this.HookClient.Log("Error initializing hook - " + ex.ToString());
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

            // Ping repeatedly on another thread
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

            // Block until task is no longer running
            this.TaskRunning.WaitOne();

            // Task was Set(), so we can now cancel our ping thread
            this.CancelRequest?.Cancel();
        }
    }
    //// End class
}
//// End namespace