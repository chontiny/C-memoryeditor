namespace Squalr.Engine.OS
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    /// <summary>
    /// An interface for manipulations and queries to processes on the system.
    /// </summary>
    public interface IProcesses
    {
        /// <summary>
        /// Subscribes the listener to process change events.
        /// </summary>
        /// <param name="listener">The object that wants to listen to process update events.</param>
        void Subscribe(IProcessObserver listener);

        /// <summary>
        /// Unsubscribes the listener from process change events.
        /// </summary>
        /// <param name="listener">The object that wants to stop listening to process update events.</param>
        void Unsubscribe(IProcessObserver listener);

        /// <summary>
        /// The process to which the engine is attached.
        /// </summary>
        Process OpenedProcess { get; set; }

        /// <summary>
        /// Gets all running processes on the system.
        /// </summary>
        /// <returns>An enumeration of see <see cref="ExternalProcess" />.</returns>
        IEnumerable<Process> GetProcesses();

        /// <summary>
        /// Gets the process that has been opened.
        /// </summary>
        /// <returns>The opened process.</returns>
        Process GetOpenedProcess();

        /// <summary>
        /// Determines if the opened process is 32 bit.
        /// </summary>
        /// <returns>Returns true if the opened process is 32 bit, otherwise false.</returns>
        Boolean IsOpenedProcess32Bit();

        /// <summary>
        /// Determines if the opened process is 64 bit.
        /// </summary>
        /// <returns>Returns true if the opened process is 64 bit, otherwise false.</returns>
        Boolean IsOpenedProcess64Bit();

        /// <summary>
        /// Determines if this program is 32 bit.
        /// </summary>
        /// <returns>A boolean indicating if this program is 32 bit or not.</returns>
        Boolean IsSelf32Bit();

        /// <summary>
        /// Determines if this program is 64 bit.
        /// </summary>
        /// <returns>A boolean indicating if this program is 64 bit or not.</returns>
        Boolean IsSelf64Bit();

        /// <summary>
        /// Determines if a process is 32 bit.
        /// </summary>
        /// <param name="process">The process to check.</param>
        /// <returns>Returns true if the process is 32 bit, otherwise false.</returns>
        Boolean IsProcess32Bit(Process process);

        /// <summary>
        /// Determines if a process is 64 bit.
        /// </summary>
        /// <param name="process">The process to check.</param>
        /// <returns>Returns true if the process is 64 bit, otherwise false.</returns>
        Boolean IsProcess64Bit(Process process);

        /// <summary>
        /// Determines if the operating system is 32 bit.
        /// </summary>
        /// <returns>A boolean indicating if the OS is 32 bit or not.</returns>
        Boolean IsOperatingSystem32Bit();

        /// <summary>
        /// Determines if the operating system is 64 bit.
        /// </summary>
        /// <returns>A boolean indicating if the OS is 64 bit or not.</returns>
        Boolean IsOperatingSystem64Bit();
    }
    //// End interface
}
//// End namespace