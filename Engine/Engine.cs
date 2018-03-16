namespace Squalr.Engine
{
    using Squalr.Engine.Processes;
    using System;
    using System.Threading;
    using VirtualMemory;

    /// <summary>
    /// </summary>
    public class Engine
    {
        /// <summary>
        /// Singleton instance of the <see cref="Engine" /> class.
        /// </summary>
        private static readonly Lazy<Engine> engineInstance = new Lazy<Engine>(
            () => { return new Engine(); },
            LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// Gets an instance of the engine.
        /// </summary>
        /// <returns>An instance of the engine.</returns>
        public static Engine GetInstance()
        {
            return engineInstance.Value;
        }

        public Engine()
        {
            this.Processes = ProcessAdapterFactory.GetProcessAdapter();
            this.VirtualMemory = VirtualMemoryAdapterFactory.GetVirtualMemoryAdapter();
        }

        /// <summary>
        /// Gets an object that provides access to target process manipulations.
        /// </summary>
        public IProcessAdapter Processes { get; private set; }

        /// <summary>
        /// Gets an object that provides access to target memory manipulations.
        /// </summary>
        public IVirtualMemoryAdapter VirtualMemory { get; private set; }
    }
    //// End class
}
//// End namespace