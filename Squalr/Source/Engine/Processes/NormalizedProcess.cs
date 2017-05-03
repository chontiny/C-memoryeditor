namespace Squalr.Source.Engine.Processes
{
    using System;
    using System.Drawing;

    /// <summary>
    /// A platform independent object to store process information.
    /// </summary>
    public class NormalizedProcess : IEquatable<NormalizedProcess>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NormalizedProcess" /> class.
        /// </summary>
        /// <param name="processId">The process id.</param>
        /// <param name="processName">The process name.</param>
        /// <param name="startTime">The time the process was created.</param>
        /// <param name="isSystemProcess">Whether or not the process is a system process.</param>
        /// <param name="icon">The icon associated with the process.</param>
        public NormalizedProcess(Int32 processId, String processName, DateTime startTime, Boolean isSystemProcess, Boolean hasWindow, Icon icon)
        {
            this.ProcessId = processId;
            this.ProcessName = processName;
            this.StartTime = startTime;
            this.IsSystemProcess = isSystemProcess;
            this.HasWindow = hasWindow;
            this.Icon = icon;
        }

        /// <summary>
        /// Gets the id of the running process.
        /// </summary>
        public Int32 ProcessId { get; private set; }

        /// <summary>
        /// Gets the name of the running process.
        /// </summary>
        public String ProcessName { get; private set; }

        /// <summary>
        /// Gets the time that the process began execution.
        /// </summary>
        public DateTime StartTime { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the process is a system process.
        /// </summary>
        public Boolean IsSystemProcess { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the process has a window.
        /// </summary>
        public Boolean HasWindow { get; private set; }

        /// <summary>
        /// Gets the icon associated with this process.
        /// </summary>
        public Icon Icon { get; private set; }

        public bool Equals(NormalizedProcess other)
        {
            if (other != null && this.ProcessId == other.ProcessId)
            {
                return true;
            }

            return false;
        }

        public override Int32 GetHashCode()
        {
            return this.ProcessId;
        }
    }
    //// End class
}
//// End namespace