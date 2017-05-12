namespace Squalr.Source.Engine.Hook.Client
{
    using Squalr.Source.Output;
    using SqualrHookClient.Source;
    using SqualrHookServer.Source;
    using System;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Channels;

    /// <summary>
    /// Provides capability to access the hook in the target process.
    /// </summary>
    [Serializable]
    public class HookClient : HookClientBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HookClient" /> class
        /// </summary>
        public HookClient()
        {
        }

        /// <summary>
        /// Gets or sets the graphics interface shared between client and hook
        /// </summary>
        // private IGraphicsInterface GraphicsInterface { get; set; }

        /// <summary>
        /// Gets or sets the speedhack interface shared between client and hook
        /// </summary>
        //  private ISpeedHackInterface SpeedHackInterface { get; set; }

        /// <summary>
        /// Injects the hook into the specified process
        /// </summary>
        /// <param name="process">The process to inject into</param>
        public override void Inject(Int32 processId)
        {
            String channelName = null;

            // Initialize the IPC server, giving the server access to the interfaces defined here
            IChannel server = EasyHook.RemoteHooking.IpcCreateServer<HookClient>(ref channelName, WellKnownObjectMode.Singleton, this);

            try
            {
                // Inject DLL into target process
                EasyHook.RemoteHooking.Inject(
                    processId,
                    EasyHook.InjectionOptions.Default,
                    typeof(HookServer).Assembly.Location,
                    typeof(HookServer).Assembly.Location,
                    channelName);
            }
            catch (Exception ex)
            {
                OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Error, "Unable to Hook Process, some features may not be available", ex);
            }

            // this.GraphicsInterface = GraphicsFactory.GetGraphicsInterface(projectDirectory);
            // this.SpeedHackInterface = new SpeedHackInterface();
        }

        /// <summary>
        /// Gets the graphics interface hook object.
        /// </summary>
        /// <returns>The graphics interface hook object.</returns>
        public override Object GetGraphicsInterface()
        {
            return null; // this.GraphicsInterface;
        }

        /// <summary>
        /// Gets the speed hack hook object.
        /// </summary>
        /// <returns>The speed hack hook object.</returns>
        public override Object GetSpeedHackInterface()
        {
            return null; //this.SpeedHackInterface;
        }

        /// <summary>
        /// Allows the server to ping the client to ensure that it is still alive.
        /// </summary>
        public override void Ping()
        {
            OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Debug, "Hook pinged client");
        }

        public override void Log(String message, String innerMessage = null)
        {
            OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Info, "Hook: " + message, innerMessage);
        }

        /// <summary>
        /// Uninjects the hook from the external process.
        /// </summary>
        public override void Uninject()
        {
            base.Uninject();

            // this.GraphicsInterface = null;
            // this.SpeedHackInterface = null;
        }
    }
    //// End class
}
//// End namespace