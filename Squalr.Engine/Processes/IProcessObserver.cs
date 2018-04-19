namespace Squalr.Engine.Processes
{
    /// <summary>
    /// An interface for an object that enumerates and selects processes running on the system.
    /// </summary>
    public interface IProcessObserver
    {
        /// <summary>
        /// Recieves an update of the process that was opened.
        /// </summary>
        /// <param name="process">The process being opened.</param>
        void Update(NormalizedProcess process);
    }
    //// End interface
}
//// End namespace