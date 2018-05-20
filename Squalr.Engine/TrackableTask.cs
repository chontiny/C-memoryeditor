namespace Squalr.Engine
{
    using System;
    using System.ComponentModel;
    using System.Threading;
    using System.Threading.Tasks;

    public abstract class TrackableTask : INotifyPropertyChanged
    {
        public delegate void OnTaskCanceled(TrackableTask task);

        public delegate void OnTaskCompleted(TrackableTask task);

        public delegate void OnProgressUpdate(Single progress);

        public delegate void UpdateProgress(Single progress);

        public event OnTaskCanceled OnCanceledEvent;

        public event OnTaskCompleted OnCompletedEvent;

        public event OnProgressUpdate OnProgressUpdatedEvent;

        private Single progress;

        private String name;

        private Boolean isCanceled;

        private Boolean isCompleted;

        public TrackableTask(String name)
        {
            this.Name = name;

            this.AccessLock = new Object();
            this.CancellationTokenSource = new CancellationTokenSource();
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
                lock (this.AccessLock)
                {
                    if (this.isCanceled == value)
                    {
                        return;
                    }

                    this.isCanceled = value;
                    this.RaisePropertyChanged(nameof(this.IsCanceled));
                    this.OnCanceledEvent?.Invoke(this);
                }
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
                lock (this.AccessLock)
                {
                    if (this.isCompleted == value)
                    {
                        return;
                    }

                    this.isCompleted = value;
                    this.RaisePropertyChanged(nameof(this.IsCompleted));
                    this.OnCompletedEvent?.Invoke(this);
                }
            }
        }

        public CancellationToken CancellationToken
        {
            get
            {
                return this.CancellationTokenSource.Token;
            }
        }

        protected CancellationTokenSource CancellationTokenSource { get; set; }

        private Object AccessLock { get; set; }

        public void Cancel()
        {
            this.CancellationTokenSource.Cancel();

            this.IsCanceled = true;
        }
    }

    public class TrackableTask<T> : TrackableTask
    {
        private TrackableTask(String name) : base(name)
        {
        }

        public static TrackableTask<T> Create(String name, out UpdateProgress progressUpdater, out CancellationToken cancellationToken)
        {
            TrackableTask<T> instance = new TrackableTask<T>(name);

            progressUpdater = instance.UpdateProgressCallback;
            cancellationToken = instance.CancellationTokenSource.Token;

            return instance;
        }

        public TrackableTask<T> With(Task<T> task)
        {
            this.Task = task;

            if (task.Status == TaskStatus.Created)
            {
                Task<T>.Run(() => task.RunSynchronously());
            }

            this.AwaitCompletion();

            return this;
        }

        public T Result
        {
            get
            {
                T result = this.Task.Result;

                this.IsCompleted = true;

                return result;
            }
        }

        private void UpdateProgressCallback(Single progress)
        {
            this.Progress = progress;
        }

        private Task<T> Task { get; set; }

        private async void AwaitCompletion()
        {
            await this.Task;

            this.IsCompleted = true;
        }
    }
    //// End class
}
//// End namespace