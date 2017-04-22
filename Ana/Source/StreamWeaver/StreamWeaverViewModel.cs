namespace Ana.Source.StreamWeaver
{
    using Docking;
    using Main;
    using Mvvm.Command;
    using Output;
    using System;
    using System.Threading;
    using System.Windows.Input;
    using TwitchLib;
    using TwitchLib.Events.Client;
    using TwitchLib.Models.Client;
    using UserSettings;    /// <summary>
                           /// View model for the Stream Weaver.
                           /// </summary>
    internal class StreamWeaverViewModel : ToolViewModel
    {
        /// <summary>
        /// The content id for the docking library associated with this view model.
        /// </summary>
        public const String ToolContentId = nameof(StreamWeaverViewModel);

        /// <summary>
        /// The home url for the cheat browser.
        /// </summary>
        public const String HomeUrl = "https://www.anathena.com/CheatBrowser/Index";

        /// <summary>
        /// Singleton instance of the <see cref="StreamWeaverViewModel" /> class.
        /// </summary>
        private static Lazy<StreamWeaverViewModel> cheatBrowserViewModelInstance = new Lazy<StreamWeaverViewModel>(
                () => { return new StreamWeaverViewModel(); },
                LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// Prevents a default instance of the <see cref="CheatBrowserViewModel" /> class from being created.
        /// </summary>
        private StreamWeaverViewModel() : base("Stream Weaver")
        {
            this.ContentId = StreamWeaverViewModel.ToolContentId;

            // Note: Cannot be async, navigation must take place on the same thread as GUI
            this.ConnectCommand = new RelayCommand(() => this.Connect(), () => true);
            this.DisconnectCommand = new RelayCommand(() => this.Disconnect(), () => true);

            MainViewModel.GetInstance().RegisterTool(this);
        }

        private TwitchClient Client { get; set; }

        /// <summary>
        /// Gets the command to connect to Twitch.
        /// </summary>
        public ICommand ConnectCommand { get; private set; }

        /// <summary>
        /// Gets the command to disconnect to Twitch.
        /// </summary>
        public ICommand DisconnectCommand { get; private set; }

        /// <summary>
        /// Gets a singleton instance of the <see cref="StreamWeaverViewModel"/> class.
        /// </summary>
        /// <returns>A singleton instance of the class.</returns>
        public static StreamWeaverViewModel GetInstance()
        {
            return cheatBrowserViewModelInstance.Value;
        }

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
            this.Client.OnMessageReceived += onMessageReceived;
            this.Client.Connect();

            try
            {
                if (this.Client.IsConnected)
                {
                    this.Client.SendMessage("[Anathena] Connected");
                    OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Info, "Twitch chat connection successful.");

                    return;
                }
            }
            catch (Exception)
            {
            }

            OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Warn, "Twitch chat connection unsuccessful. Please check your username and access token in the settings.");
        }

        private void Disconnect()
        {
            if (this.Client == null)
            {
                OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Info, "Twitch connection is already disconnected.");
                return;
            }

            try
            {
                try
                {
                    this.Client.SendMessage("[Anathena] Disconnected");
                }
                catch (Exception)
                {
                }

                try
                {
                    this.Client.OnMessageReceived -= onMessageReceived;
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

        private void onMessageReceived(Object sender, OnMessageReceivedArgs e)
        {
            OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Info, e.ChatMessage.Message);
        }
    }
    //// End class
}
//// End namespace