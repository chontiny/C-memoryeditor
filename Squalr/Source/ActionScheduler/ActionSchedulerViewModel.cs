namespace Squalr.Source.ActionScheduler
{
    using GalaSoft.MvvmLight.CommandWpf;
    using Squalr.Engine.ActionScheduler;
    using Squalr.Engine.Output;
    using SqualrCore.Source.Docking;
    using SqualrCore.Source.Utils.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Input;

    /// <summary>
    /// Class to schedule tasks that are executed.
    /// </summary>
    public class ActionSchedulerViewModel : ToolViewModel
    {
        /// <summary>
        /// The interval between scheduler calls, in milliseconds.
        /// </summary>
        private const Int32 SchedulerInterval = 16;

        /// <summary>
        /// Singleton instance of the <see cref="ActionSchedulerViewModel" /> class.
        /// </summary>
        private static Lazy<ActionSchedulerViewModel> actionSchedulerViewModelInstance = new Lazy<ActionSchedulerViewModel>(
            () => { return new ActionSchedulerViewModel(); },
            LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// Prevents a default instance of the <see cref="ActionSchedulerViewModel" /> class from being created.
        /// </summary>
        private ActionSchedulerViewModel() : base("Action Scheduler")
        {
            this.AccessLock = new Object();
            this.Actions = new LinkedList<ScheduledTask>();

            this.CancelTaskCommand = new RelayCommand<ScheduledTask>(task => task.Cancel(), (task) => true);

            this.Update();
        }

        /// <summary>
        /// Gets a command to cancel a running task.
        /// </summary>
        public ICommand CancelTaskCommand { get; private set; }

        /// <summary>
        /// Gets the tasks that are actively running.
        /// </summary>
        public IEnumerable<ScheduledTask> ActiveTasks
        {
            get
            {
                return this.Actions.Select(x => x).Where(x => !x.IsTaskComplete);
            }
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
        /// Gets a singleton instance of the <see cref="ActionSchedulerViewModel"/> class.
        /// </summary>
        /// <returns>A singleton instance of the class.</returns>
        public static ActionSchedulerViewModel GetInstance()
        {
            return ActionSchedulerViewModel.actionSchedulerViewModelInstance.Value;
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
                this.RaisePropertyChanged(nameof(this.ActiveTasks));

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
                    await Task.Delay(ActionSchedulerViewModel.SchedulerInterval);

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
                            this.RaisePropertyChanged(nameof(this.ActiveTasks));
                        }
                    }
                }
                while (true);
            });
        }
    }
    //// End class
}
//// End namespace