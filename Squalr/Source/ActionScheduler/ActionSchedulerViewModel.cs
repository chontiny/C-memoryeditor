namespace Squalr.Source.ActionScheduler
{
    using Squalr.Source.Docking;
    using System;
    using System.Collections.Generic;
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
            //// this.CancelTaskCommand = new RelayCommand<Task>(task => task.Cancel(), (task) => true);
        }

        /// <summary>
        /// Gets a command to cancel a running task.
        /// </summary>
        public ICommand CancelTaskCommand { get; private set; }

        /// <summary>
        /// Gets the tasks that are actively running.
        /// </summary>
        public IEnumerable<Task> ActiveTasks
        {
            get
            {
                return null; // return Scheduler.GetInstance().Actions.Select(x => x).Where(x => !x.IsTaskComplete);
            }
        }

        /// <summary>
        /// Gets a singleton instance of the <see cref="ActionSchedulerViewModel"/> class.
        /// </summary>
        /// <returns>A singleton instance of the class.</returns>
        public static ActionSchedulerViewModel GetInstance()
        {
            return ActionSchedulerViewModel.actionSchedulerViewModelInstance.Value;
        }

        public void OnTaskListChanged()
        {
            this.RaisePropertyChanged(nameof(this.ActiveTasks));
        }
    }
    //// End class
}
//// End namespace