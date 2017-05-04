namespace SqualrHookClient.Source
{
    using System;

    /// <summary>
    /// Provides capability to access the hook in the target process.
    /// </summary>
    [Serializable]
    public abstract class HookClientBase : MarshalByRefObject, IHookClient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HookClientBase" /> class
        /// </summary>
        public HookClientBase()
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
        public abstract void Inject(Int32 processId);

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
        public abstract void Ping();


        public abstract void Log(String message);

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