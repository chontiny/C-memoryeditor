namespace Ana.Source.StreamWeaver
{
    using Content;
    using Docking;
    using LiveCharts;
    using LiveCharts.Wpf;
    using Main;
    using Mvvm.Command;
    using Output;
    using Project;
    using Project.ProjectItems;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using TwitchLib;
    using TwitchLib.Events.Client;
    using TwitchLib.Models.Client;
    using UserSettings;
    using Utils.Extensions;
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
        /// The histogram collection object.
        /// </summary>
        private SeriesCollection seriesCollection;

        /// <summary>
        /// The labels for the chart.
        /// </summary>
        private String[] chartLabels;

        /// <summary>
        /// Prevents a default instance of the <see cref="StreamWeaverViewModel" /> class from being created.
        /// </summary>
        private StreamWeaverViewModel() : base("Stream Weaver")
        {
            this.ContentId = StreamWeaverViewModel.ToolContentId;
            this.CommandVotes = new ConcurrentDictionary<String, Int64>();

            StreamWeaverTask streamWeaverTask = new StreamWeaverTask(this.OnUpdate);

            this.ToggleConnectionCommand = new RelayCommand(() => this.ToggleConnection(), () => true);
            this.ClearVotesCommand = new RelayCommand(() => this.ClearVotes(), () => true);

            MainViewModel.GetInstance().RegisterTool(this);
        }

        /// <summary>
        /// Gets the command to clear votes.
        /// </summary>
        public ICommand ClearVotesCommand { get; private set; }

        /// <summary>
        /// Gets the command to connect to Twitch.
        /// </summary>
        public ICommand ToggleConnectionCommand { get; private set; }

        /// <summary>
        /// Gets or sets the number of glitches to allow to be activated at once via stream commands.
        /// </summary>
        public Int32 NumberOfGlitches
        {
            get
            {
                return SettingsViewModel.GetInstance().NumberOfGlitches;
            }

            set
            {
                SettingsViewModel.GetInstance().NumberOfGlitches = value;
                this.RaisePropertyChanged(nameof(this.NumberOfGlitches));
            }
        }

        /// <summary>
        /// Gets or sets the number of curses to allow to be activated at once via stream commands.
        /// </summary>
        public Int32 NumberOfCurses
        {
            get
            {
                return SettingsViewModel.GetInstance().NumberOfCurses;
            }

            set
            {
                SettingsViewModel.GetInstance().NumberOfCurses = value;
                this.RaisePropertyChanged(nameof(this.NumberOfCurses));
            }
        }

        /// <summary>
        /// Gets or sets the number of buffs to allow to be activated at once via stream commands.
        /// </summary>
        public Int32 NumberOfBuffs
        {
            get
            {
                return SettingsViewModel.GetInstance().NumberOfBuffs;
            }

            set
            {
                SettingsViewModel.GetInstance().NumberOfBuffs = value;
                this.RaisePropertyChanged(nameof(this.NumberOfBuffs));
            }
        }

        /// <summary>
        /// Gets or sets the number of utilities to allow to be activated at once via stream commands.
        /// </summary>
        public Int32 NumberOfUtilities
        {
            get
            {
                return SettingsViewModel.GetInstance().NumberOfUtilities;
            }

            set
            {
                SettingsViewModel.GetInstance().NumberOfUtilities = value;
                this.RaisePropertyChanged(nameof(this.NumberOfUtilities));
            }
        }

        /// <summary>
        /// Gets or sets the histogram collection object.
        /// </summary>
        public SeriesCollection SeriesCollection
        {
            get
            {
                return this.seriesCollection;
            }

            set
            {
                this.seriesCollection = value;
                this.RaisePropertyChanged(nameof(this.SeriesCollection));
            }
        }

        /// <summary>
        /// Gets or sets the histogram labels.
        /// </summary>
        public String[] ChartLabels
        {
            get
            {
                return this.chartLabels;
            }

            set
            {
                this.chartLabels = value;
                this.RaisePropertyChanged(nameof(this.ChartLabels));
            }
        }

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
        /// Gets or sets the current command votes.
        /// </summary>
        private ConcurrentDictionary<String, Int64> CommandVotes { get; set; }

        /// <summary>
        /// Gets or sets the last vote total.
        /// </summary>
        private Int64 LastVoteTotal { get; set; }

        /// <summary>
        /// Gets a singleton instance of the <see cref="StreamWeaverViewModel"/> class.
        /// </summary>
        /// <returns>A singleton instance of the class.</returns>
        public static StreamWeaverViewModel GetInstance()
        {
            return StreamWeaverViewModel.cheatBrowserViewModelInstance.Value;
        }

        /// <summary>
        /// Event fired when the stream commands need to be update.
        /// </summary>
        private void OnUpdate()
        {
            // Collect project items
            IEnumerable<ProjectItem> candidateProjectItems = ProjectExplorerViewModel.GetInstance().ProjectRoot.Flatten()
                .Select(item => item)
                .Where(item => item.Category != ProjectItem.ProjectItemCategory.None)
                .Where(item => !String.IsNullOrWhiteSpace(item.StreamCommand));

            // Tally up votes for each project item
            var itemVotes = this.CommandVotes
                 .Join(
                     candidateProjectItems,
                     votes => votes.Key,
                     item => item.StreamCommand,
                     (votes, item) => new { command = votes, item = item })
                .OrderBy(x => x.item.Category);

            Int64 totalVotes = itemVotes.Sum(tally => tally.command.Value);

            if (totalVotes == this.LastVoteTotal)
            {
                return;
            }

            this.LastVoteTotal = totalVotes;

            // Activate top votes
            foreach (ProjectItem.ProjectItemCategory category in Enum.GetValues(typeof(ProjectItem.ProjectItemCategory)))
            {
                Int32 numberToActivate = 0;

                switch (category)
                {
                    case ProjectItem.ProjectItemCategory.Glitch:
                        numberToActivate = this.NumberOfGlitches;
                        break;
                    case ProjectItem.ProjectItemCategory.Curse:
                        numberToActivate = this.NumberOfCurses;
                        break;
                    case ProjectItem.ProjectItemCategory.Buff:
                        numberToActivate = this.NumberOfBuffs;
                        break;
                    case ProjectItem.ProjectItemCategory.Utility:
                        numberToActivate = this.NumberOfUtilities;
                        break;
                }

                IEnumerable<ProjectItem> candidateItems = itemVotes
                    .Select(tally => tally)
                    .OrderByDescending(tally => tally.command.Value)
                    .Where(tally => tally.item.Category == category)
                    .Select(tally => tally.item);

                // Handle deactivations
                candidateItems
                    .Skip(numberToActivate)
                    .ForEach(item => item.IsActivated = false);

                // Handle activations
                candidateItems
                    .Take(numberToActivate)
                    .ForEach(item => item.IsActivated = true);
            }

            // Collect labels
            this.ChartLabels = itemVotes.Select(x => x.command.Key).ToArray();

            // Collect values
            IChartValues chartGlitchValues = new ChartValues<Int64>(itemVotes.Select(tally => tally.item.Category == ProjectItem.ProjectItemCategory.Glitch ? tally.command.Value : 0));
            IChartValues chartCurseValues = new ChartValues<Int64>(itemVotes.Select(tally => tally.item.Category == ProjectItem.ProjectItemCategory.Curse ? tally.command.Value : 0));
            IChartValues chartBuffValues = new ChartValues<Int64>(itemVotes.Select(tally => tally.item.Category == ProjectItem.ProjectItemCategory.Buff ? tally.command.Value : 0));
            IChartValues chartUtilityValues = new ChartValues<Int64>(itemVotes.Select(tally => tally.item.Category == ProjectItem.ProjectItemCategory.Utility ? tally.command.Value : 0));

            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                if (this.SeriesCollection == null)
                {

                    this.SeriesCollection = new SeriesCollection()
                    {
                        // Glitches
                        new ColumnSeries
                        {
                            Values = chartGlitchValues,
                            Fill = Brushes.Green,
                            DataLabels = true
                        },
                        
                        // Curses
                        new ColumnSeries
                        {
                            Values = chartCurseValues,
                            Fill = Brushes.Red,
                            DataLabels = true
                        },

                        // Buffs
                        new ColumnSeries
                        {
                            Values = chartBuffValues,
                            Fill = Brushes.Blue,
                            DataLabels = true
                        },
                        
                        // Utilities
                        new ColumnSeries
                        {
                            Values = chartUtilityValues,
                            Fill = Brushes.Yellow,
                            DataLabels = true
                        }
                    };
                }
                else
                {
                    this.SeriesCollection[0].Values = chartGlitchValues;
                    this.SeriesCollection[1].Values = chartCurseValues;
                    this.SeriesCollection[2].Values = chartBuffValues;
                    this.SeriesCollection[3].Values = chartUtilityValues;
                }
            });
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

            this.Client = new TwitchClient(credentials, "beyondthesummit");
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

        private Random random = new Random();
        private String DebugMap(String command)
        {
            IEnumerable<ProjectItem> candidateProjectItems = ProjectExplorerViewModel.GetInstance().ProjectRoot.Flatten()
                .Select(item => item)
                .Where(item => item.Category != ProjectItem.ProjectItemCategory.None)
                .Where(item => !String.IsNullOrWhiteSpace(item.StreamCommand));

            Int32 index = random.Next(0, candidateProjectItems.Count());

            command = candidateProjectItems.ElementAt(index).StreamCommand;

            return command;
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
        /// Clears the current votes.
        /// </summary>
        private void ClearVotes()
        {
            this.CommandVotes.Clear();
        }

        /// <summary>
        /// Processes a user's Twitch chat command.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="command">The command given by the user.</param>
        private void ProcessCommand(Int64 userId, String command)
        {
            command = this.DebugMap(command);

            this.CommandVotes.AddOrUpdate(command, 1, (key, count) => count + 1);

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