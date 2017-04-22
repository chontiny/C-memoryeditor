namespace Ana.Source.StreamWeaver
{
    using Content;
    using Docking;
    using Main;
    using Mvvm.Command;
    using Output;
    using System;
    using System.Threading;
    using System.Windows.Input;
    using System.Windows.Media.Imaging;
    using TwitchLib;
    using TwitchLib.Events.Client;
    using TwitchLib.Models.Client;
    using UserSettings;

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

            // Note: Cannot be async, navigation must take place on the same thread as GUI
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
        /// Gets or sets the Twitch client connection object.
        /// </summary>
        private TwitchClient Client { get; set; }

        /// <summary>
        /// Gets a singleton instance of the <see cref="StreamWeaverViewModel"/> class.
        /// </summary>
        /// <returns>A singleton instance of the class.</returns>
        public static StreamWeaverViewModel GetInstance()
        {
            return cheatBrowserViewModelInstance.Value;
        }

        /// <summary>
        /// Connects the Twitch client.
        /// </summary>
        private void Connect()
        {
            if (this.Client != null)
            {
                try
                {
                    if (this.Client.IsConnected)
                    {
                        OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Info, "Twitch connection is already active.");
                        return;
                    }
                }
                catch (Exception)
                {
                }
            }

            String username = SettingsViewModel.GetInstance().TwitchUsername;
            String accessToken = SettingsViewModel.GetInstance().TwitchAccessToken;

            ConnectionCredentials credentials = new ConnectionCredentials(username, accessToken);

            this.Client = new TwitchClient(credentials, username);
            this.Client.OnMessageReceived += this.OnMessageReceived;
            this.Client.Connect();

            try
            {
                if (this.Client.IsConnected)
                {
                    this.IsConnected = true;
                    OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Info, "Twitch chat connection successful.");
                    return;
                }
            }
            catch (Exception)
            {
            }

            this.IsConnected = false;
            OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Warn, "Twitch chat connection unsuccessful. Please check your username and access token in the settings.");
        }

        /// <summary>
        /// Disconnects the Twitch client.
        /// </summary>
        private void Disconnect()
        {
            if (this.Client == null)
            {
                OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Info, "Twitch connection is already disconnected.");
                return;
            }

            this.IsConnected = false;

            try
            {
                try
                {
                    this.Client.OnMessageReceived -= this.OnMessageReceived;
                }
                catch (Exception)
                {
                }

                this.Client.Disconnect();
                this.Client = null;
            }
            catch (Exception)
            {
            }

            OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Info, "Disconnected from Twitch.");
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
        /// Processes a user's Twitch chat command.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="command">The command given by the user.</param>
        private void ProcessCommand(Int64 userId, String command)
        {
            // ProjectItem projectItem = ProjectExplorerViewModel.GetInstance().ProjectRoot.Children[0];
            //  projectItem.IsActivated = !projectItem.IsActivated;

            OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Info, userId + " - " + command);
        }

        /// <summary>
        /// Event fired when a message is recieved from Twitch chat.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The message event.</param>
        private void OnMessageReceived(Object sender, OnMessageReceivedArgs e)
        {
            Int64 userId;

            if (Int64.TryParse(e.ChatMessage?.UserId, out userId))
            {
                this.ProcessCommand(userId, e.ChatMessage?.Message);
            }
        }
    }
    //// End class
}
//// End namespace