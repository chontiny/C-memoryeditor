namespace SqualrHookClient.Source
{
    using System;

    /// <summary>
    /// Provides capability to access the hook in the target process.
    /// </summary>
    [Serializable]
    public abstract class HookClientBase : MarshalByRefObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HookClientBase" /> class.
        /// </summary>
        public HookClientBase()
        {
        }

        [Serializable]
        public delegate void DisconnectedEvent();

        /// <summary>
        /// Client event used to notify the hook to exit.
        /// </summary>
        public event DisconnectedEvent Disconnected;

        /// <summary>
        /// Injects the hook into the specified process.
        /// </summary>
        /// <param name="process">The process to inject into.</param>
        public abstract void Inject(Int32 processId);

        /// <summary>
        /// Allows the server to ping the client to ensure that it is still alive.
        /// </summary>
        public abstract void Ping();

        /// <summary>
        /// Logs a message from the server to the client.
        /// </summary>
        /// <param name="message">The log message.</param>
        /// <param name="innerMessage">The inner log message.</param>
        public abstract void Log(String message, String innerMessage = null);

        /// <summary>
        /// Uninjects the hook from the external process.
        /// </summary>
        public virtual void Uninject()
        {
            this.Disconnected?.Invoke();
        }
    }
    //// End class
}
//// End namespace