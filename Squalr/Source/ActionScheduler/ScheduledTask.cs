namespace Squalr.Source.ActionScheduler
{
    using Squalr.Source.Output;
    using System;
    using System.ComponentModel;
    using System.Threading;
    using Utils.Extensions;

    /// <summary>
    /// A task that repeatedly performs an action.
    /// </summary>
    internal abstract class ScheduledTask : INotifyPropertyChanged
    {
        /// <summary>
        /// The default update loop time.
        /// </summary>
        private const Int32 DefaultUpdateTime = 400;

        /// <summary>
        /// The minimum progress.
        /// </summary>
        private const Double MinimumProgress = 0.0;

        /// <summary>
        /// The maximum progress.
        /// </summary>
        private const Double MaximumProgress = 100.0;

        /// <summary>
        /// The default progress completion threshold.
        /// </summary>
        private const Double DefaultProgressCompletionThreshold = 100.0;

        /// <summary>
        /// The progress of this task.
        /// </summary>
        private Double progress;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScheduledTask" /> class.
        /// </summary>
        /// <param name="startAction">The start callback function.</param>
        /// <param name="updateAction">The update callback function.</param>
        /// <param name="endAction">The end callback function.</param>
        /// <param name="taskName">The dependencies and dependency behavior of this task.</param>
        /// <param name="isRepeated">Whether or not this task is repeated.</param>
        /// <param name="trackProgress">Whether or not progress is tracked for this task.</param>
        public ScheduledTask(
            String taskName,
            Boolean isRepeated,
            Boolean trackProgress) : this(taskName, isRepeated, trackProgress, new DependencyBehavior())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScheduledTask" /> class.
        /// </summary>
        /// <param name="startAction">The start callback function.</param>
        /// <param name="updateAction">The update callback function.</param>
        /// <param name="endAction">The end callback function.</param>
        /// <param name="taskName">The name of this task.</param>
        /// <param name="isRepeated">Whether or not this task is repeated.</param>
        /// <param name="trackProgress">Whether or not progress is tracked for this task.</param>
        /// <param name="dependencyBehavior">The dependencies and dependency behavior of this task.</param>
        public ScheduledTask(
            String taskName,
            Boolean isRepeated,
            Boolean trackProgress,
            DependencyBehavior dependencyBehavior)
        {
            this.IsCanceled = false;
            this.HasStarted = false;
            this.AccessLock = new Object();

            this.TaskName = taskName;
            this.IsRepeated = isRepeated;
            this.IsTaskComplete = !trackProgress;
            this.DependencyBehavior = dependencyBehavior == null ? new DependencyBehavior() : dependencyBehavior;

            this.progress = 0.0;

            this.ProgressCompletionThreshold = ScheduledTask.DefaultProgressCompletionThreshold;
        }

        /// <summary>
        /// Occurs after a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets or sets the dependency behavior of this task.
        /// </summary>
        public DependencyBehavior DependencyBehavior { get; set; }

        /// <summary>
        /// Gets or sets the time to wait (in ms) before next update (and time to wait for cancelation).
        /// </summary>
        public Int32 UpdateInterval { get; set; }

        /// <summary>
        /// Gets a value indicating whether the scheduled task is repeated.
        /// </summary>
        public Boolean IsRepeated { get; private set; }

        /// <summary>
        /// Gets or sets the name of this task.
        /// </summary>
        public String TaskName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the scheduled task has completed. This is not in terms of progress, but instead indicates the task is entirely done.
        /// </summary>
        public Boolean IsTaskComplete { get; protected set; }

        /// <summary>
        /// Gets a value indicating whether the start callback function can be called.
        /// </summary>
        public Boolean CanStart
        {
            get
            {
                return !this.IsCanceled && !this.HasStarted;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the update callback function can be called.
        /// </summary>
        public Boolean CanUpdate
        {
            get
            {
                return !this.IsCanceled && !this.IsBusy && (!this.HasUpdated || this.IsRepeated);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the end callback function can be called.
        /// </summary>
        public Boolean CanEnd
        {
            get
            {
                return !this.IsBusy && (this.HasUpdated || this.IsCanceled);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this task is busy.
        /// </summary>
        public Boolean IsBusy { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this task has been canceled.
        /// </summary>
        public Boolean IsCanceled { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this task has started.
        /// </summary>
        private Boolean HasStarted { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this task has been updated.
        /// </summary>
        private Boolean HasUpdated { get; set; }

        /// <summary>
        /// Gets or sets a lock for access to state information.
        /// </summary>
        private Object AccessLock { get; set; }

        /// <summary>
        /// Gets a value indicating whether the scheduled task has completed in terms of progress, although not necessarily finalized.
        /// </summary>
        public Boolean IsProgressComplete
        {
            get
            {
                return this.Progress >= this.ProgressCompletionThreshold;
            }
        }

        /// <summary>
        /// Gets the progress of this task.
        /// </summary>
        public Double Progress
        {
            get
            {
                return this.progress;
            }

            private set
            {
                this.progress = value;
                this.NotifyPropertyChanged(nameof(this.Progress));
            }
        }

        /// <summary>
        /// Gets or sets the progress completion threshold. Progress higher this threshold will be considered complete.
        /// </summary>
        protected Double ProgressCompletionThreshold { get; set; }

        /// <summary>
        /// Initializes start state variables. Must be called before calling the start callback.
        /// </summary>
        public void InitializeStart()
        {
            lock (this.AccessLock)
            {
                this.HasStarted = true;
                this.IsBusy = true;
            }
        }

        /// <summary>
        /// Initializes update state variables. Must be called before calling the update callback.
        /// </summary>
        public void InitializeUpdate()
        {
            lock (this.AccessLock)
            {
                this.HasUpdated = true;
                this.IsBusy = true;
            }
        }

        /// <summary>
        /// Canels this task.
        /// </summary>
        public void Cancel()
        {
            ActionSchedulerViewModel.GetInstance().CancelAction(this);
            this.IsCanceled = true;
        }

        /// <summary>
        /// A wrapper function for the start callback. This will call the start function and update required state information.
        /// </summary>
        public void Start()
        {
            lock (this.AccessLock)
            {
                if (!this.IsBusy)
                {
                    String error = "Error in task scheduler. Attempting to start before flagging action as busy.";
                    OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Fatal, error);
                    throw new Exception(error);
                }

                this.OnUpdate();
                this.IsBusy = false;
            }
        }

        /// <summary>
        /// A wrapper function for the update callback. This will call the update function and update required state information.
        /// </summary>
        public void Update()
        {
            lock (this.AccessLock)
            {
                if (!this.IsBusy)
                {
                    String error = "Error in task scheduler. Attempting to update before flagging action as busy.";
                    OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Fatal, error);
                    throw new Exception(error);
                }

                this.OnUpdate();
                Thread.Sleep(this.UpdateInterval);
                this.IsBusy = false;
            }
        }

        /// <summary>
        /// Called when the task is ending.
        /// </summary>
        public void End()
        {
            this.OnEnd();
        }

        /// <summary>
        /// Starts the repeated task.
        /// </summary>
        public void Schedule()
        {
            ActionSchedulerViewModel.GetInstance().ScheduleAction(this);
        }

        /// <summary>
        /// Updates the progress of this task.
        /// </summary>
        /// <param name="progress">The new progress.</param>
        public void UpdateProgress(Double progress)
        {
            this.Progress = progress.Clamp(ScheduledTask.MinimumProgress, ScheduledTask.MaximumProgress);
        }

        /// <summary>
        /// Updates the progress of this task.
        /// </summary>
        /// <param name="subtotal">The current subtotal of an arbitrary progress goal.</param>
        /// <param name="total">The progress goal total.</param>
        public void UpdateProgress(Int32 subtotal, Int32 total)
        {
            this.UpdateProgress(total <= 0 ? 0.0 : (((Double)subtotal / (Double)total) * ScheduledTask.MaximumProgress) + ScheduledTask.MinimumProgress);
        }

        /// <summary>
        /// Called when the scheduled task starts.
        /// </summary>
        protected virtual void OnBegin()
        {
        }

        /// <summary>
        /// Called when the scheduled task is updated.
        /// </summary>
        protected virtual void OnUpdate()
        {
        }

        /// <summary>
        /// Called when the repeated task completes.
        /// </summary>
        protected virtual void OnEnd()
        {
        }

        /// <summary>
        /// Indicates that a given property in this project item has changed.
        /// </summary>
        /// <param name="propertyName">The name of the changed property.</param>
        protected void NotifyPropertyChanged(String propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    //// End class
}
//// End namespace