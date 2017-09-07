namespace Squalr.Source.ActionScheduler
{
    using Docking;
    using Output;
    using Squalr.Source.Utils.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Class to schedule tasks that are executed.
    /// </summary>
    internal class ActionSchedulerViewModel : ToolViewModel
    {
        /// <summary>
        /// The content id for the docking library associated with this view model.
        /// </summary>
        public const String ToolContentId = nameof(ActionSchedulerViewModel);

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
            this.ContentId = ActionSchedulerViewModel.ToolContentId;
            this.AccessLock = new Object();
            this.Actions = new LinkedList<ScheduledTask>();

            this.Update();
        }

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
        /// Gets or sets a lock for access to scheduled tasks.
        /// </summary>
        private Object AccessLock { get; set; }

        /// <summary>
        /// Gets or sets actions being scheduled.
        /// </summary>
        public LinkedList<ScheduledTask> Actions { get; set; }

        /// <summary>
        /// Gets or sets the next action being scheduled.
        /// </summary>
        private LinkedListNode<ScheduledTask> NextAction { get; set; }

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
        /// <param name="startAction">The start callback function.</param>
        /// <param name="updateAction">The update callback function.</param>
        /// <param name="endAction">The end callback function.</param>
        public void ScheduleAction(ScheduledTask scheduledTask)
        {
            lock (this.AccessLock)
            {
                // Do not schedule actions of the same type
                if (this.Actions.Select(x => x.GetType()).Any(x => x == scheduledTask.GetType()))
                {
                    OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Warn, "Action not scheduled. This action is already queued.");
                    return;
                }

                this.Actions.AddLast(scheduledTask);
                this.RaisePropertyChanged(nameof(this.ActiveTasks));
            }
        }

        /// <summary>
        /// Cancels the given scheduled task.
        /// </summary>
        /// <param name="scheduledTask">The scheduled task to cancel.</param>
        public void CancelAction(ScheduledTask scheduledTask)
        {
            lock (this.AccessLock)
            {
                scheduledTask.Cancel();
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
                            // Check if dependencies are complete for this task to start
                            if (nextTask.DependencyBehavior.IsDependencyRequiredForStart
                                && !this.DependenciesResolved(nextTask))
                            {
                                continue;
                            }

                            // Start the task
                            nextTask.InitializeStart();
                            Task.Run(() => nextTask.Start());
                        }
                        else if (nextTask.CanUpdate)
                        {
                            // Check if dependencies are complete for this task to update
                            if (nextTask.DependencyBehavior.IsDependencyRequiredForUpdate
                                && !this.DependenciesResolved(nextTask))
                            {
                                continue;
                            }

                            // Update the task
                            nextTask.InitializeUpdate();
                            Task.Run(() => nextTask.Update());
                        }
                        else if (nextTask.CanEnd)
                        {
                            // End the task
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

        /// <summary>
        /// Determines if the depencies are resolved for a given scheduled task.
        /// </summary>
        /// <param name="scheduledTask">The scheduled task.</param>
        /// <returns>True if the dependencies are resolved, otherwise false.</returns>
        private Boolean DependenciesResolved(ScheduledTask scheduledTask)
        {
            IEnumerable<Type> completedDependencies = this.Actions.Select(x => x)
                .Where(x => x.IsTaskComplete)
                .Select(x => x.GetType());

            return scheduledTask.DependencyBehavior.AreDependenciesResolved(completedDependencies);
        }
    }
    //// End class
}
//// End namespace