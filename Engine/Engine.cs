namespace Squalr.Engine
{
    using Squalr.Engine.Output;
    using System;
    using System.Threading;

    /// <summary>
    /// 
    /// </summary>
    public class Engine
    {
        /// <summary>
        /// Singleton instance of the <see cref="Engine" /> class.
        /// </summary>
        private static Lazy<Engine> engineInstance = new Lazy<Engine>(
                () => { return new Engine(); },
                LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// 
        /// </summary>
        public Engine()
        {
        }

        /// <summary>
        /// Gets an instance of the engine.
        /// </summary>
        /// <returns>An instance of the engine.</returns>
        public static Engine GetInstance()
        {
            return engineInstance.Value;
        }
    }
    //// End class
}
//// End namespace