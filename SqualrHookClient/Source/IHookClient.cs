namespace SqualrHookClient.Source
{
    using System;

    /// <summary>
    /// Interface defining a hook client to control the hook injected into an external process
    /// </summary>
    public interface IHookClient
    {
        /// <summary>
        /// Injects the hook into the specified process
        /// </summary>
        /// <param name="process">The process to inject into</param>
        void Inject(Int32 processId);

        /// <summary>
        /// Gets the graphics interface hook object
        /// </summary>
        /// <returns>The graphics interface hook object</returns>
        Object GetGraphicsInterface();

        /// <summary>
        /// Gets the speed hack hook object
        /// </summary>
        /// <returns>The speed hack hook object</returns>
        Object GetSpeedHackInterface();

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