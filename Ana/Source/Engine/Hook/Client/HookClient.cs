namespace Ana.Source.Engine.Hook.Client
{
    using Ana.Source.Output;
    using AnathenaHookServer.Source;
    using EasyHook;
    using Project;
    using System;
    using System.IO;
    using System.Runtime.Remoting;

    /// <summary>
    /// Provides capability to access the hook in the target process.
    /// </summary>
    [Serializable]
    public class HookClient : AnathenaHookClient.Source.HookClientBase
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
            String projectDirectory = Path.GetDirectoryName(ProjectExplorerViewModel.GetInstance().ProjectFilePath);
            String channelName = null;

            // this.GraphicsInterface = GraphicsFactory.GetGraphicsInterface(projectDirectory);
            // this.SpeedHackInterface = new SpeedHackInterface();

            // Initialize the IPC server, giving the server access to the interfaces defined here
            RemoteHooking.IpcCreateServer<HookClient>(ref channelName, WellKnownObjectMode.Singleton, this);

            try
            {
                // Inject DLL into target process
                RemoteHooking.Inject(
                    processId,
                    InjectionOptions.Default,
                    typeof(HookServer).Assembly.Location,
                    typeof(HookServer).Assembly.Location,
                    channelName,
                    projectDirectory);
            }
            catch (Exception ex)
            {
                OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Error, "Unable to Hook Process. Some features may not be available - " + ex.ToString());
            }
        }

        /// <summary>
        /// Gets the graphics interface hook object
        /// </summary>
        /// <returns>The graphics interface hook object</returns>
        public Object GetGraphicsInterface()
        {
            return null; // this.GraphicsInterface;
        }

        /// <summary>
        /// Gets the speed hack hook object
        /// </summary>
        /// <returns>The speed hack hook object</returns>
        public Object GetSpeedHackInterface()
        {
            return null; //this.SpeedHackInterface;
        }

        /// <summary>
        /// Allows the server to ping the client to ensure that it is still alive
        /// </summary>
        public override void Ping()
        {
            OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Debug, "Hook pinged client");
        }

        /// <summary>
        /// Uninjects the hook from the external process
        /// </summary>
        public void Uninject()
        {
            // this.GraphicsInterface = null;
            // this.SpeedHackInterface = null;
        }
    }
    //// End class
}
//// End namespace