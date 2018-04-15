namespace Squalr.Engine.TaskScheduler
{
    using Output;
    using Squalr.Engine.Utils.DataStructures;
    using Squalr.Engine.Utils.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Class to schedule tasks that are executed.
    /// </summary>
    public class Scheduler : ITaskManager
    {
        /// <summary>
        /// The interval between scheduler calls, in milliseconds.
        /// </summary>
        private const Int32 SchedulerInterval = 16;

        /// <summary>
        /// Singleton instance of the <see cref="Scheduler" /> class.
        /// </summary>
        private static Lazy<Scheduler> scheduleInstance = new Lazy<Scheduler>(
            () => { return new Scheduler(); },
            LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// Thread safe collection of listeners.
        /// </summary>
        private ConcurrentHashSet<ITaskManagerObserver> processListeners;

        /// <summary>
        /// Prevents a default instance of the <see cref="Scheduler" /> class from being created.
        /// </summary>
        private Scheduler()
        {
            this.AccessLock = new Object();
            this.Actions = new LinkedList<ScheduledTask>();
            this.processListeners = new ConcurrentHashSet<ITaskManagerObserver>();

            this.Update();
        }

        /// <summary>
        /// Gets or sets actions being scheduled.
        /// </summary>
        public LinkedList<ScheduledTask> Actions { get; set; }

        /// <summary>
        /// Gets or sets the next action being scheduled.
        /// </summary>
        private LinkedListNode<ScheduledTask> NextAction { get; set; }

        /// <summary>
        /// Gets or sets a lock for access to scheduled tasks.
        /// </summary>
        private Object AccessLock { get; set; }

        /// <summary>
        /// Gets a singleton instance of the <see cref="Scheduler"/> class.
        /// </summary>
        /// <returns>A singleton instance of the class.</returns>
        public static Scheduler GetInstance()
        {
            return Scheduler.scheduleInstance.Value;
        }

        /// <summary>
        /// Subscribes the listener to task manager change events.
        /// </summary>
        /// <param name="listener">The object that wants to listen to task manager update events.</param>
        public void Subscribe(ITaskManagerObserver listener)
        {
            this.processListeners.Add(listener);
        }

        /// <summary>
        /// Unsubscribes the listener from task manager change events.
        /// </summary>
        /// <param name="listener">The object that wants to stop listening to task manager update events.</param>
        public void Unsubscribe(ITaskManagerObserver listener)
        {
            this.processListeners.Remove(listener);
        }

        /// <summary>
        /// Schedules a given task.
        /// </summary>
        /// <param name="scheduledTask">The task to be scheduled.</param>
        public void ScheduleAction(ScheduledTask scheduledTask)
        {
            lock (this.AccessLock)
            {
                // Do not schedule actions of the same type
                if (this.Actions.Select(x => x.GetType()).Any(x => x == scheduledTask.GetType()))
                {
                    Output.Log(LogLevel.Warn, "Action not scheduled. This action is already queued.");
                    return;
                }

                scheduledTask.ResetState();
                this.Actions.AddLast(scheduledTask);
                this.NotifyObservers();

                foreach (ScheduledTask task in scheduledTask.Dependencies)
                {
                    this.ScheduleAction(task);
                }
            }
        }

        /// <summary>
        /// The scheduler update loop. Cycles through tasks, updating them.
        /// </summary>
        private void Update()
        {
            Task.Run(
                async () =>
            {
                do
                {
                    await Task.Delay(Scheduler.SchedulerInterval);

                    lock (this.AccessLock)
                    {
                        // Cycle to the next task
                        this.NextAction = this.NextAction?.NextOrFirst() ?? this.Actions.First;

                        if (NextAction == null)
                        {
                            continue;
                        }

                        ScheduledTask nextTask = this.NextAction.Value;

                        if (nextTask.CanStart)
                        {
                            // Start the task
                            nextTask.InitializeStart();
                            Task.Run(() => nextTask.Begin());
                        }
                        else if (nextTask.CanUpdate)
                        {
                            // Update the task
                            nextTask.InitializeUpdate();
                            Task.Run(() => nextTask.Update());
                        }
                        else if (nextTask.CanEnd)
                        {
                            // End the task
                            nextTask.InitializeEnd();
                            Task.Run(() => nextTask.End());

                            // Permanently remove this task
                            this.Actions.Remove(nextTask);
                            this.NotifyObservers();
                        }
                    }
                }
                while (true);
            });
        }

        /// <summary>
        /// Notifies observers of changes in the task list.
        /// </summary>
        private void NotifyObservers()
        {
            foreach (ITaskManagerObserver observer in this.processListeners)
            {
                observer.OnTaskListChanged();
            }
        }
    }
    //// End class
}
//// End namespace