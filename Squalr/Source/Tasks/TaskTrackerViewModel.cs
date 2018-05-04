namespace Squalr.Source.Tasks
{
    using GalaSoft.MvvmLight.CommandWpf;
    using Squalr.Engine;
    using Squalr.Engine.Utils.DataStructures;
    using Squalr.Source.Docking;
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Input;

    /// <summary>
    /// Class to schedule tasks that are executed.
    /// </summary>
    public class TaskTrackerViewModel : ToolViewModel
    {
        /// <summary>
        /// Singleton instance of the <see cref="TaskTrackerViewModel" /> class.
        /// </summary>
        private static Lazy<TaskTrackerViewModel> actionSchedulerViewModelInstance = new Lazy<TaskTrackerViewModel>(
            () => { return new TaskTrackerViewModel(); },
            LazyThreadSafetyMode.ExecutionAndPublication);

        private FullyObservableCollection<TrackableTask> trackedTasks;

        /// <summary>
        /// Prevents a default instance of the <see cref="TaskTrackerViewModel" /> class from being created.
        /// </summary>
        private TaskTrackerViewModel() : base("Action Scheduler")
        {
            this.trackedTasks = new FullyObservableCollection<TrackableTask>();

            this.CancelTaskCommand = new RelayCommand<Task>(task => { }, (task) => true);
        }

        /// <summary>
        /// Gets a singleton instance of the <see cref="TaskTrackerViewModel"/> class.
        /// </summary>
        /// <returns>A singleton instance of the class.</returns>
        public static TaskTrackerViewModel GetInstance()
        {
            return TaskTrackerViewModel.actionSchedulerViewModelInstance.Value;
        }

        /// <summary>
        /// Gets a command to cancel a running task.
        /// </summary>
        public ICommand CancelTaskCommand { get; private set; }

        /// <summary>
        /// Gets the tasks that are actively running.
        /// </summary>
        public FullyObservableCollection<TrackableTask> TrackedTasks
        {
            get
            {
                return this.trackedTasks;
            }
        }

        public void TrackTask(TrackableTask task)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                task.TaskCompletedCallback = this.OnTaskComplete;
                this.TrackedTasks.Add(task);
            }));
        }

        private void OnTaskComplete(TrackableTask task)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                if (this.TrackedTasks.Contains(task))
                {
                    this.TrackedTasks.Remove(task);
                }
            }));
        }
    }
    //// End class
}
//// End namespace