namespace AnathenaHook.Client
{
    using System.Diagnostics;

    /// <summary>
    /// Interface defining a hook client to control the hook injected into an external process
    /// </summary>
    internal interface IHookClient
    {
        /// <summary>
        /// Injects the hook into the specified process
        /// </summary>
        /// <param name="process">The process to inject into</param>
        void Inject(Process process);

        /// <summary>
        /// Allows the server to ping the client to ensure that it is still alive
        /// </summary>
        void Ping();

        /// <summary>
        /// Uninjects the hook from the external process
        /// </summary>
        void Uninject();
    }
    //// End interface
}
//// End namespace