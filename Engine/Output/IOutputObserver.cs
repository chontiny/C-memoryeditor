namespace Squalr.Engine.Output
{
    using System;

    public interface IOutputObserver
    {
        /// <summary>
        /// Logs a message to output.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        /// <param name="message">The log message.</param>
        /// <param name="innerMessage">The log inner message.</param>
        void OnLogEvent(LogLevel logLevel, String message, String innerMessage);
    }
    //// End interface
}
//// End namespace