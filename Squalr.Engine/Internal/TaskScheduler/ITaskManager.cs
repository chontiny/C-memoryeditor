namespace Squalr.Engine.TaskScheduler
{
    /// <summary>
    /// An interface for an object that manages running tasks in this engine.
    /// </summary>
    public interface ITaskManager
    {
        /// <summary>
        /// Subscribes the listener to task manager change events.
        /// </summary>
        /// <param name="listener">The object that wants to listen to task manager update events.</param>
        void Subscribe(ITaskManagerObserver listener);

        /// <summary>
        /// Unsubscribes the listener from task manager change events.
        /// </summary>
        /// <param name="listener">The object that wants to stop listening to task manager update events.</param>
        void Unsubscribe(ITaskManagerObserver listener);
    }
    //// End interface
}
//// End namespace