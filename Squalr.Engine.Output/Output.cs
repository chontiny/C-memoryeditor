namespace Squalr.Engine.Output
{
    using Squalr.Engine.Utils.DataStructures;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Handles recieving logging events, processing them, and sending them to observers.
    /// </summary>
    public static class Output
    {
        /// <summary>
        /// Gets or sets the list of output masks to apply to all logged messages.
        /// </summary>
        private static IList<OutputMask> outputMasks = new List<OutputMask>();

        /// <summary>
        /// Gets or sets the set of active observers.
        /// </summary>
        private static ConcurrentHashSet<IOutputObserver> observers = new ConcurrentHashSet<IOutputObserver>();

        /// <summary>
        /// Subscribes the listener to logging events.
        /// </summary>
        /// <param name="listener">The object that wants to listen to logging events.</param>
        public static void Subscribe(IOutputObserver listener)
        {
            Output.observers.Add(listener);
        }

        /// <summary>
        /// Unsubscribes the listener from logging events.
        /// </summary>
        /// <param name="listener">The object that wants to stop listening to logging events.</param>
        public static void Unsubscribe(IOutputObserver listener)
        {
            Output.observers.Remove(listener);
        }

        /// <summary>
        /// Logs a message to output, filtering out sensitive text with a specific output mask.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        /// <param name="message">The log message.</param>
        /// <param name="innerMessage">The log inner message.</param>
        /// <param name="outputMask">The output masking filter.</param>
        public static void Log(LogLevel logLevel, String message, String innerMessage, OutputMask outputMask)
        {
            message = outputMask.ApplyFilter(message);
            innerMessage = outputMask.ApplyFilter(innerMessage);

            Output.Log(logLevel, message, innerMessage);
        }

        /// <summary>
        /// Logs a message to output.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        /// <param name="message">The log message.</param>
        /// <param name="exception">An exception to be shown as the log inner message.</param>
        public static void Log(LogLevel logLevel, String message, Exception exception)
        {
            Output.Log(logLevel, message, exception?.ToString());
        }

        /// <summary>
        /// Logs a message to output.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        /// <param name="message">The log message.</param>
        /// <param name="innerMessage">The log inner message.</param>
        public static void Log(LogLevel logLevel, String message, String innerMessage = null)
        {
            foreach (OutputMask outputMask in Output.outputMasks)
            {
                message = outputMask.ApplyFilter(message);
                innerMessage = outputMask.ApplyFilter(innerMessage);
            }

            foreach (IOutputObserver observer in Output.observers.ToList())
            {
                observer.OnLogEvent(logLevel, message, innerMessage);
            }
        }

        /// <summary>
        /// Adds a new output mask to the list of applied output masks.
        /// </summary>
        /// <param name="outputMask">The output mask to add.</param>
        public static void AddOutputMask(OutputMask outputMask)
        {
            Output.outputMasks.Add(outputMask);
        }

        /// <summary>
        /// Removes an output mask from the list of applied output masks.
        /// </summary>
        /// <param name="outputMask">The output mask to remove.</param>
        public static void RemoveOutputMask(OutputMask outputMask)
        {
            if (Output.outputMasks.Contains(outputMask))
            {
                Output.outputMasks.Remove(outputMask);
            }
        }
    }
    //// End class
}
//// End namespace
