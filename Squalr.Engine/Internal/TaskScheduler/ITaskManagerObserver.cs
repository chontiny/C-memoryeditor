namespace Squalr.Engine.TaskScheduler
{
    /// <summary>
    /// An interface for an object that listens for changes in scheduled tasks.
    /// </summary>
    public interface ITaskManagerObserver
    {
        /// <summary>
        /// Recieves indication tasks were updated.
        /// </summary>
        void OnTaskListChanged();
    }
    //// End interface
}
//// End namespace