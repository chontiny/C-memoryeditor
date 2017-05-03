namespace AnathenaHook.Client
{
    using System;
    using System.Diagnostics;

    /// <summary>
    /// Provides capability to access objects in the target process
    /// </summary>
    [Serializable]
    internal class HookClient : MarshalByRefObject, IHookClient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HookClient" /> class
        /// </summary>
        public HookClient()
        {
        }

        /// <summary>
        /// Injects the hook into the specified process
        /// </summary>
        /// <param name="process">The process to inject into</param>
        public void Inject(Process process)
        {
        }

        /// <summary>
        /// Allows the server to ping the client to ensure that it is still alive
        /// </summary>
        public void Ping()
        {
        }

        /// <summary>
        /// Uninjects the hook from the external process
        /// </summary>
        public void Uninject()
        {
        }
    }
    //// End class
}
//// End namespace