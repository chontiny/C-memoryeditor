namespace SqualrHookServer.Source
{
    using EasyHook;
    using SqualrHookClient.Source;
    using SqualrHookServer.Source.Graphics;
    using SqualrHookServer.Source.Network;
    using SqualrHookServer.Source.Random;
    using SqualrHookServer.Source.Speed;
    using System;
    using System.Reflection;
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
            try
            {
                System.Collections.IDictionary properties = new System.Collections.Hashtable();
                properties["name"] = channelName;
                properties["portName"] = channelName + Guid.NewGuid().ToString("N");

                //// BinaryServerFormatterSinkProvider binaryProv = new BinaryServerFormatterSinkProvider();
                ////  binaryProv.TypeFilterLevel = TypeFilterLevel.Full;

                ////  IpcServerChannel clientServerChannel = new IpcServerChannel(properties, binaryProv);
                ////  ChannelServices.RegisterChannel(clientServerChannel, false);
            }
            catch (Exception ex)
            {
                this.HookClient.Log("Failed to set up bidirectional hook", ex.ToString());
            }
        }

        public static LocalHook CreateHook(String moduleName, String functionName, Delegate callback, Object sender)
        {
            LocalHook hook = LocalHook.Create(LocalHook.GetProcAddress(moduleName, functionName), callback, sender);
            hook.ThreadACL.SetExclusiveACL(new Int32[0]);

            return hook;
        }

        /// <summary>
        /// Gets or sets the graphics hook.
        /// </summary>
        private GraphicsHook GraphicsHook { get; set; }

        /// <summary>
        /// Gets or sets the random hook.
        /// </summary>
        private RandomHook RandomHook { get; set; }

        /// <summary>
        /// Gets or sets the speed hook.
        /// </summary>
        private SpeedHook SpeedHook { get; set; }

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
                AppDomain.CurrentDomain.AssemblyResolve += this.AssemblyResolve;
                this.HookClient.Disconnected += this.OnDisconnect;

                this.TaskRunning = new ManualResetEvent(false);
                this.TaskRunning.Reset();

                this.RandomHook = new RandomHook(this.HookClient);
                this.SpeedHook = new SpeedHook(this.HookClient);
                this.NetworkHook = new NetworkHook(this.HookClient);

                this.MaintainConnection();
            }
            catch (Exception ex)
            {
                this.HookClient.Log("Error initializing hook", ex.ToString());
            }
            finally
            {
                try
                {
                    this.HookClient.Log("Detaching hooks");
                }
                catch
                {
                }

                // Sleep long enough for any remaining messages to complete sending
                Thread.Sleep(100);
            }
        }

        /// <summary>
        /// When not using GAC there can be issues with remoting assemblies resolving correctly this is a workaround that ensures that the current assembly is correctly associated.
        /// </summary>
        /// <param name="sender">The sending object.</param>
        /// <param name="args">The resolve event args.</param>
        /// <returns>The resolved assembly.</returns>
        private Assembly AssemblyResolve(Object sender, ResolveEventArgs args)
        {
            return this.GetType().Assembly.FullName == args.Name ? this.GetType().Assembly : null;
        }

        private void OnDisconnect()
        {
            try
            {
                this.TaskRunning.Set();
            }
            catch
            {
                // Target process died
            }
        }

        /// <summary>
        /// Begin a background thread to check periodically that the host process is still accessible on its IPC channel.
        /// If the host process stops then we will automatically uninstall the hooks.
        /// </summary>
        private void MaintainConnection()
        {
            try
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
            catch
            {
            }
        }
    }
    //// End class
}
//// End namespace