namespace Squalr.Engine
{
    using System;
    using System.ComponentModel;
    using System.Threading;
    using System.Threading.Tasks;

    public class TrackableTask<T> : INotifyPropertyChanged
    {
        public delegate void OnTaskCanceled(TrackableTask<T> task);

        public delegate void OnTaskCompleted(TrackableTask<T> task);

        public delegate void OnProgressUpdate(Single progress);

        public event OnTaskCanceled OnCanceledEvent;

        public event OnTaskCompleted OnCompletedEvent;

        public event OnProgressUpdate OnProgressUpdatedEvent;

        private Single progress;

        private String name;

        private Boolean isCanceled;

        private Boolean isCompleted;

        protected TrackableTask()
        {
        }

        public Single Progress
        {
            get
            {
                return this.progress;
            }

            set
            {
                this.progress = value;
                this.RaisePropertyChanged(nameof(this.Progress));
                this.OnProgressUpdatedEvent?.Invoke(value);
            }
        }

        public String Name
        {
            get
            {
                return this.name;
            }

            set
            {
                this.name = value;
                this.RaisePropertyChanged(nameof(this.Name));
            }
        }

        public Boolean IsCanceled
        {
            get
            {
                return this.isCanceled;
            }

            protected set
            {
                this.isCanceled = value;
                this.RaisePropertyChanged(nameof(this.IsCanceled));
                this.OnCanceledEvent?.Invoke(this);
            }
        }

        public Boolean IsCompleted
        {
            get
            {
                return this.isCompleted;
            }

            protected set
            {
                this.isCompleted = value;
                this.RaisePropertyChanged(nameof(this.IsCompleted));
                this.OnCompletedEvent?.Invoke(this);
            }
        }

        protected CancellationTokenSource CancellationTokenSource { get; set; }

        public void Cancel()
        {
            this.CancellationTokenSource.Cancel();

            this.IsCanceled = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Indicates that a given property in this project item has changed.
        /// </summary>
        /// <param name="propertyName">The name of the changed property.</param>
        protected void RaisePropertyChanged(String propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public TrackableTask(String name)
        {
            this.Name = name;
        }

        public T Result
        {
            get
            {
                return this.Task.Result;
            }
        }

        private Task<T> Task { get; set; }

        public void SetTrackedTask(Task<T> task, CancellationTokenSource cancellationTokenSource)
        {
            this.Task = task;
            this.CancellationTokenSource = cancellationTokenSource;

            this.AwaitCompletion();
        }

        private async void AwaitCompletion()
        {
            await this.Task;

            this.IsCompleted = true;
        }
    }
    //// End class
}
//// End namespace