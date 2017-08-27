namespace Squalr.Source.StreamWeaver
{
    using Content;
    using Docking;
    using Main;
    using Mvvm.Command;
    using System;
    using System.Threading;
    using System.Windows.Input;
    using System.Windows.Media.Imaging;

    /// <summary>
    /// View model for the Stream Weaver.
    /// </summary>
    internal class StreamWeaverViewModel : ToolViewModel
    {
        /// <summary>
        /// The content id for the docking library associated with this view model.
        /// </summary>
        public const String ToolContentId = nameof(StreamWeaverViewModel);

        /// <summary>
        /// Singleton instance of the <see cref="StreamWeaverViewModel" /> class.
        /// </summary>
        private static Lazy<StreamWeaverViewModel> cheatBrowserViewModelInstance = new Lazy<StreamWeaverViewModel>(
                () => { return new StreamWeaverViewModel(); },
                LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// Indicates whether a Twitch connection is open.
        /// </summary>
        private Boolean isConnected;

        /// <summary>
        /// Prevents a default instance of the <see cref="StreamWeaverViewModel" /> class from being created.
        /// </summary>
        private StreamWeaverViewModel() : base("Stream Weaver")
        {
            this.ContentId = StreamWeaverViewModel.ToolContentId;

            StreamWeaverTask streamWeaverTask = new StreamWeaverTask(this.OnUpdate);

            this.ToggleConnectionCommand = new RelayCommand(() => this.ToggleConnection(), () => true);

            MainViewModel.GetInstance().RegisterTool(this);
        }

        /// <summary>
        /// Gets the command to connect to Twitch.
        /// </summary>
        public ICommand ToggleConnectionCommand { get; private set; }

        /// <summary>
        /// Gets the image indicating the current connection status.
        /// </summary>
        public BitmapImage ConnectionImage
        {
            get
            {
                if (this.IsConnected)
                {
                    return Images.Connected;
                }
                else
                {
                    return Images.Disconnected;
                }
            }
        }

        /// <summary>
        /// Gets the connection toggle option string for our connection.
        /// </summary>
        public String ConnectionOption
        {
            get
            {
                if (this.IsConnected)
                {
                    return "Disconnect";
                }
                else
                {
                    return "Connect";
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not there is an active Twitch connection.
        /// </summary>
        public Boolean IsConnected
        {
            get
            {
                return this.isConnected;
            }

            set
            {
                this.isConnected = value;
                this.RaisePropertyChanged(nameof(this.IsConnected));
                this.RaisePropertyChanged(nameof(this.ConnectionOption));
                this.RaisePropertyChanged(nameof(this.ConnectionImage));
            }
        }

        /// <summary>
        /// Gets a singleton instance of the <see cref="StreamWeaverViewModel"/> class.
        /// </summary>
        /// <returns>A singleton instance of the class.</returns>
        public static StreamWeaverViewModel GetInstance()
        {
            return StreamWeaverViewModel.cheatBrowserViewModelInstance.Value;
        }


        /// <summary>
        /// Toggles the current Twitch connection.
        /// </summary>
        private void ToggleConnection()
        {
            if (this.IsConnected)
            {
                this.Disconnect();
            }
            else
            {
                this.Connect();
            }
        }

        /// <summary>
        /// Event fired when the stream commands need to be update.
        /// </summary>
        private void OnUpdate()
        {
            if (!this.IsConnected)
            {
                return;
            }
        }

        /// <summary>
        /// Connects to the Twitch client.
        /// </summary>
        private void Connect()
        {
            this.IsConnected = true;
        }

        /// <summary>
        /// Disconnects from the Twitch client.
        /// </summary>
        private void Disconnect()
        {
            this.IsConnected = false;
        }
    }
    //// End class
}
//// End namespace