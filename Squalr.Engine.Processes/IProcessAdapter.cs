namespace Squalr.Engine.Processes
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// An interface for an object that enumerates and selects processes running on the system.
    /// </summary>
    public interface IProcessAdapter
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
        /// Gets all running processes on the system.
        /// </summary>
        /// <returns>An enumeration of see <see cref="NormalizedProcess" />.</returns>
        IEnumerable<NormalizedProcess> GetProcesses();

        /// <summary>
        /// Opens a process for editing.
        /// </summary>
        /// <param name="process">The process to be opened.</param>
        void OpenProcess(NormalizedProcess process);

        /// <summary>
        /// Closes a process for editing.
        /// </summary>
        void CloseProcess();

        /// <summary>
        /// Gets the process that has been opened.
        /// </summary>
        /// <returns>The opened process.</returns>
        NormalizedProcess GetOpenedProcess();

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
        Boolean IsProcess32Bit(NormalizedProcess process);

        /// <summary>
        /// Determines if a process is 64 bit.
        /// </summary>
        /// <param name="process">The process to check.</param>
        /// <returns>Returns true if the process is 64 bit, otherwise false.</returns>
        Boolean IsProcess64Bit(NormalizedProcess process);

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