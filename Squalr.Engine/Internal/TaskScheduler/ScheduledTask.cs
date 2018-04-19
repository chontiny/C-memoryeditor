namespace Squalr.Engine.TaskScheduler
{
    using Squalr.Engine.Output;
    using Squalr.Engine.Utils.Extensions;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Utils.Extensions;

    /// <summary>
    /// A task that repeatedly performs an action.
    /// </summary>
    public abstract class ScheduledTask : INotifyPropertyChanged
    {
        /// <summary>
        /// The minimum progress.
        /// </summary>
        public const Double MinimumProgress = 0.0;

        /// <summary>
        /// The maximum progress.
        /// </summary>
        public const Double MaximumProgress = 100.0;

        /// <summary>
        /// The default update loop time.
        /// </summary>
        private const Int32 DefaultUpdateTime = 400;

        /// <summary>
        /// The default progress completion threshold.
        /// </summary>
        private const Double DefaultProgressCompletionThreshold = 100.0;

        /// <summary>
        /// The progress of this task.
        /// </summary>
        private Double progress;

        /// <summary>
        /// A value indicating whether the scheduled task was canceled.
        /// </summary>
        private Boolean isCanceled;

        /// <summary>
        /// A value indicating whether the scheduled task has completed in terms of progress, although not necessarily finalized.
        /// </summary>
        private Boolean isTaskComplete;

        /// <summary>
        /// A value indicating whether to track progress for the scheduled task.
        /// </summary>
        private Boolean trackProgress;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScheduledTask" /> class.
        /// </summary>
        /// <param name="taskName">The name of this task.</param>
        /// <param name="isRepeated">Whether or not this task is repeated.</param>
        /// <param name="trackProgress">Whether or not progress is tracked for this task.</param>
        public ScheduledTask(
            String taskName,
            Boolean isRepeated,
            Boolean trackProgress)
        {
            this.ResetState();
            this.AccessLock = new Object();

            this.TaskName = taskName;
            this.TrackProgress = trackProgress;
            this.IsRepeated = isRepeated;

            this.progress = 0.0;

            this.ProgressCompletionThreshold = ScheduledTask.DefaultProgressCompletionThreshold;

            this.Dependencies = new Queue<ScheduledTask>();
        }

        /// <summary>
        /// Occurs after a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets or sets the time to wait (in ms) before next update (and time to wait for cancelation).
        /// </summary>
        public Int32 UpdateInterval { get; set; }

        /// <summary>
        /// Gets a value indicating whether the scheduled task is repeated.
        /// </summary>
        public Boolean IsRepeated { get; private set; }

        /// <summary>
        /// Gets a value indicating whether to track progress for the scheduled task.
        /// </summary>
        public Boolean TrackProgress
        {
            get
            {
                return this.trackProgress;
            }

            private set
            {
                this.trackProgress = value;

                this.RaisePropertyChanged(nameof(this.TrackProgress));
            }
        }

        /// <summary>
        /// Gets or sets the name of this task.
        /// </summary>
        public String TaskName { get; set; }

        /// <summary>
        /// Gets a value indicating whether the start callback function can be called.
        /// </summary>
        public Boolean CanStart
        {
            get
            {
                return !this.IsCanceled && !this.HasStarted && this.AreDependenciesResolved();
            }
        }

        /// <summary>
        /// Gets a value indicating whether the update callback function can be called.
        /// </summary>
        public Boolean CanUpdate
        {
            get
            {
                return !this.IsCanceled && this.HasStarted && !this.HasUpdated && !this.IsBusy;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the end callback function can be called.
        /// </summary>
        public Boolean CanEnd
        {
            get
            {
                return this.HasUpdated && !this.IsBusy;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this task is busy.
        /// </summary>
        public Boolean IsBusy { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the scheduled task was canceled.
        /// </summary>
        public Boolean IsCanceled
        {
            get
            {
                return this.isCanceled;
            }

            private set
            {
                this.isCanceled = value;
                this.RaisePropertyChanged(nameof(this.isCanceled));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the scheduled task has completed in terms of progress, although not necessarily finalized.
        /// </summary>
        public Boolean IsTaskComplete
        {
            get
            {
                return this.isTaskComplete;
            }

            set
            {
                this.isTaskComplete = value;
                this.RaisePropertyChanged(nameof(this.IsTaskComplete));
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
                this.RaisePropertyChanged(nameof(this.Progress));
            }
        }

        /// <summary>
        /// Gets or sets the tasks that this task depends on.
        /// </summary>
        public Queue<ScheduledTask> Dependencies { get; set; }

        /// <summary>
        /// Gets or sets the progress completion threshold. Progress higher this threshold will be considered complete.
        /// </summary>
        protected Double ProgressCompletionThreshold { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this task has started.
        /// </summary>
        private Boolean HasStarted { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this task has been updated.
        /// </summary>
        private Boolean HasUpdated { get; set; }

        /// <summary>
        /// Gets or sets a cancelation request for the update loop.
        /// </summary>
        private CancellationTokenSource CancelRequest { get; set; }

        /// <summary>
        /// Gets or sets a lock for access to state information.
        /// </summary>
        private Object AccessLock { get; set; }

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
                if (!this.IsRepeated)
                {
                    this.HasUpdated = true;
                }

                this.IsBusy = true;
            }
        }

        /// <summary>
        /// Initializes end state variables. Must be called before calling the end callback.
        /// </summary>
        public void InitializeEnd()
        {
            lock (this.AccessLock)
            {
                this.IsBusy = true;
            }
        }

        /// <summary>
        /// Canels this task.
        /// </summary>
        public void Cancel()
        {
            this.IsCanceled = true;

            this.CancelRequest?.Cancel();
        }

        /// <summary>
        /// Starts the repeated task.
        /// </summary>
        public void Start()
        {
            Scheduler.GetInstance().ScheduleAction(this);
        }

        /// <summary>
        /// Ends the repeated task update loop if this task is a repeating task.
        /// </summary>
        public void EndUpdateLoop()
        {
            if (this.IsRepeated)
            {
                this.HasUpdated = true;
            }
        }

        /// <summary>
        /// Updates the progress of this task.
        /// </summary>
        /// <param name="progress">The new progress.</param>
        /// <param name="canFinalize">A value indicating whether the update can trigger a completion.</param>
        public void UpdateProgress(Double progress, Boolean canFinalize = true)
        {
            this.Progress = progress.Clamp(ScheduledTask.MinimumProgress, ScheduledTask.MaximumProgress);

            if (canFinalize)
            {
                if (this.Progress >= this.ProgressCompletionThreshold)
                {
                    this.IsTaskComplete = true;
                }
            }
        }

        /// <summary>
        /// Updates the progress of this task.
        /// </summary>
        /// <param name="subtotal">The current subtotal of an arbitrary progress goal.</param>
        /// <param name="total">The progress goal total.</param>
        /// <param name="canFinalize">A value indicating whether the update can trigger a completion.</param>
        public void UpdateProgress(Int32 subtotal, Int32 total, Boolean canFinalize = true)
        {
            this.UpdateProgress(total <= 0 ? 0.0 : (((Double)subtotal / (Double)total) * ScheduledTask.MaximumProgress) + ScheduledTask.MinimumProgress, canFinalize);
        }

        /// <summary>
        /// Updates the progress of this task.
        /// </summary>
        /// <param name="subtotal">The current subtotal of an arbitrary progress goal.</param>
        /// <param name="total">The progress goal total.</param>
        /// <param name="canFinalize">A value indicating whether the update can trigger a completion.</param>
        public void UpdateProgress(Int64 subtotal, Int64 total, Boolean canFinalize = true)
        {
            this.UpdateProgress(total <= 0 ? 0.0 : (((Double)subtotal / (Double)total) * ScheduledTask.MaximumProgress) + ScheduledTask.MinimumProgress, canFinalize);
        }

        /// <summary>
        /// Resets all state tracking variables.
        /// </summary>
        public void ResetState()
        {
            this.Progress = 0.0;
            this.IsCanceled = false;
            this.HasStarted = false;
            this.HasUpdated = false;
            this.IsBusy = false;
            this.IsTaskComplete = false;
        }

        /// <summary>
        /// A wrapper function for the start callback. This will call the start function and update required state information.
        /// </summary>
        public void Begin()
        {
            lock (this.AccessLock)
            {
                if (!this.IsBusy)
                {
                    String error = "Error in task scheduler. Attempting to start before flagging action as busy.";
                    Output.Log(LogLevel.Fatal, error);
                    throw new Exception(error);
                }

                this.OnBegin();
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
                    Output.Log(LogLevel.Fatal, error);
                    throw new Exception(error);
                }

                this.CancelRequest = new CancellationTokenSource();

                Task updateTask = Task.Run(
                () =>
                {
                    try
                    {
                        this.OnUpdate(this.CancelRequest.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        Output.Log(LogLevel.Info, "Task cancelled: " + this.TaskName);
                    }
                },
                this.CancelRequest.Token);

                updateTask.Wait();

                Thread.Sleep(this.UpdateInterval);
                this.IsBusy = false;
            }
        }

        /// <summary>
        /// Called when the task is ending.
        /// </summary>
        public void End()
        {
            if (!this.IsCanceled)
            {
                this.OnEnd();
            }

            this.ResetState();

            this.IsTaskComplete = true;
        }

        /// <summary>
        /// Called when the scheduled task starts.
        /// </summary>
        protected abstract void OnBegin();

        /// <summary>
        /// Called when the scheduled task is updated.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token for handling canceled tasks.</param>
        protected abstract void OnUpdate(CancellationToken cancellationToken);

        /// <summary>
        /// Called when the repeated task completes.
        /// </summary>
        protected abstract void OnEnd();

        /// <summary>
        /// Indicates that a given property in this project item has changed.
        /// </summary>
        /// <param name="propertyName">The name of the changed property.</param>
        protected void RaisePropertyChanged(String propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Determines if all dependencies are resolved based on the provided list of completed dependencies.
        /// </summary>
        /// <returns>True if all dependencies are resolved, otherwise false.</returns>
        private Boolean AreDependenciesResolved()
        {
            return this.Dependencies.Count <= 0 || this.Dependencies.All(dependency => dependency.IsTaskComplete);
        }
    }
    //// End class
}
//// End namespace