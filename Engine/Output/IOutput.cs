namespace Squalr.Engine.Output
{
    using System;

    public interface IOutput
    {
        /// <summary>
        /// Subscribes the listener to logging events.
        /// </summary>
        /// <param name="listener">The object that wants to listen to logging events.</param>
        void Subscribe(IOutputObserver listener);

        /// <summary>
        /// Unsubscribes the listener from logging events.
        /// </summary>
        /// <param name="listener">The object that wants to stop listening to logging events.</param>
        void Unsubscribe(IOutputObserver listener);

        /// <summary>
        /// Logs a message to output, filtering out sensitive text with a specific output mask.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        /// <param name="message">The log message.</param>
        /// <param name="innerMessage">The log inner message.</param>
        /// <param name="outputMask">The output masking filter.</param>
        void Log(LogLevel logLevel, String message, String innerMessage, OutputMask outputMask);

        /// <summary>
        /// Logs a message to output.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        /// <param name="message">The log message.</param>
        /// <param name="exception">An exception to be shown as the log inner message.</param>
        void Log(LogLevel logLevel, String message, Exception exception);

        /// <summary>
        /// Logs a message to output.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        /// <param name="message">The log message.</param>
        /// <param name="innerMessage">The log inner message.</param>
        void Log(LogLevel logLevel, String message, String innerMessage = null);
    }
    //// End interface
}
//// End namespace