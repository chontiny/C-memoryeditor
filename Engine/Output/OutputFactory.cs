namespace Squalr.Engine.Output
{
    using System;
    using System.Threading;

    /// <summary>
    /// Returns the instance of an object implementing the <see cref="IOutput"/> interface.
    /// </summary>
    internal class OutputFactory
    {
        /// <summary>
        /// Singleton instance of the <see cref="Output"/> class.
        /// </summary>
        private static Lazy<Output> outputInstance = new Lazy<Output>(
            () => { return new Output(); },
            LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// Gets an object to read the logs of internal events.
        /// </summary>
        /// <returns>An object to read the logs of internal events.</returns>
        public static IOutput GetOutput()
        {
            return outputInstance.Value;
        }
    }
    //// End class
}
//// End namespace