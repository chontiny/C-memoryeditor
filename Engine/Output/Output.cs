namespace Squalr.Engine.Output
{
    using Squalr.Engine.DataStructures;
    using System;
    using System.Collections.Generic;

    internal class Output : IOutput
    {
        /// <summary>
        /// 
        /// </summary>
        public Output()
        {
            this.OutputMasks = new List<OutputMask>();
            this.Observers = new ConcurrentHashSet<IOutputObserver>();
        }

        /// <summary>
        /// Gets or sets the list of output masks to apply to all logged messages.
        /// </summary>
        private IList<OutputMask> OutputMasks { get; set; }

        /// <summary>
        /// Gets or sets the set of active observers.
        /// </summary>
        private ConcurrentHashSet<IOutputObserver> Observers { get; set; }

        /// <summary>
        /// Subscribes the listener to logging events.
        /// </summary>
        /// <param name="listener">The object that wants to listen to logging events.</param>
        public void Subscribe(IOutputObserver listener)
        {
            this.Observers.Add(listener);
        }

        /// <summary>
        /// Unsubscribes the listener from logging events.
        /// </summary>
        /// <param name="listener">The object that wants to stop listening to logging events.</param>
        public void Unsubscribe(IOutputObserver listener)
        {
            this.Observers.Remove(listener);
        }

        /// <summary>
        /// Logs a message to output, filtering out sensitive text with a specific output mask.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        /// <param name="message">The log message.</param>
        /// <param name="innerMessage">The log inner message.</param>
        /// <param name="outputMask">The output masking filter.</param>
        public void Log(LogLevel logLevel, String message, String innerMessage, OutputMask outputMask)
        {
            message = outputMask.ApplyFilter(message);
            innerMessage = outputMask.ApplyFilter(innerMessage);

            this.Log(logLevel, message, innerMessage);
        }

        /// <summary>
        /// Logs a message to output.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        /// <param name="message">The log message.</param>
        /// <param name="exception">An exception to be shown as the log inner message.</param>
        public void Log(LogLevel logLevel, String message, Exception exception)
        {
            this.Log(logLevel, message, exception?.ToString());
        }

        /// <summary>
        /// Logs a message to output.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        /// <param name="message">The log message.</param>
        /// <param name="innerMessage">The log inner message.</param>
        public void Log(LogLevel logLevel, String message, String innerMessage = null)
        {
            foreach (OutputMask outputMask in this.OutputMasks)
            {
                message = outputMask.ApplyFilter(message);
                innerMessage = outputMask.ApplyFilter(innerMessage);
            }

            foreach(IOutputObserver observer in this.Observers.ToList())
            {
                observer.OnLogEvent(logLevel, message, innerMessage);
            }
        }

        /// <summary>
        /// Adds a new output mask to the list of applied output masks.
        /// </summary>
        /// <param name="outputMask">The output mask to add.</param>
        public void AddOutputMask(OutputMask outputMask)
        {
            this.OutputMasks.Add(outputMask);
        }

        /// <summary>
        /// Removes an output mask from the list of applied output masks.
        /// </summary>
        /// <param name="outputMask">The output mask to remove.</param>
        public void RemoveOutputMask(OutputMask outputMask)
        {
            if (this.OutputMasks.Contains(outputMask))
            {
                this.OutputMasks.Remove(outputMask);
            }
        }
    }
}
