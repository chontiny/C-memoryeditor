namespace Squalr.Source.Browse.StreamConfig
{
    using Content;
    using Docking;
    using GalaSoft.MvvmLight.Command;
    using Main;
    using Squalr.Properties;
    using Squalr.Source.Api;
    using Squalr.Source.Api.Models;
    using Squalr.Source.Output;
    using Squalr.Source.ProjectExplorer;
    using Squalr.Source.ProjectExplorer.ProjectItems;
    using Squalr.Source.Utils.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using System.Windows.Media.Imaging;

    /// <summary>
    /// View model for the Stream Config.
    /// </summary>
    internal class StreamConfigViewModel : ToolViewModel, INavigable
    {
        /// <summary>
        /// The content id for the docking library associated with this view model.
        /// </summary>
        public const String ToolContentId = nameof(StreamConfigViewModel);

        /// <summary>
        /// Singleton instance of the <see cref="StreamConfigViewModel" /> class.
        /// </summary>
        private static Lazy<StreamConfigViewModel> streamConfigViewModelInstance = new Lazy<StreamConfigViewModel>(
                () => { return new StreamConfigViewModel(); },
                LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// Indicates whether a Stream connection is open.
        /// </summary>
        private Boolean isConnected;

        /// <summary>
        /// A value indicating if the connection status is loading
        /// </summary>
        public Boolean isConnectionStatusLoading;

        /// <summary>
        /// Prevents a default instance of the <see cref="StreamConfigViewModel" /> class from being created.
        /// </summary>
        private StreamConfigViewModel() : base("Stream Config")
        {
            this.ContentId = StreamConfigViewModel.ToolContentId;
            this.VoteLock = new Object();
            this.IsConnected = true;

            StreamVotePollTask streamVotePollTask = new StreamVotePollTask(this.OnUpdate);

            this.ToggleConnectionCommand = new RelayCommand(() => this.ToggleConnection(), () => true);

            MainViewModel.GetInstance().RegisterTool(this);
        }

        /// <summary>
        /// Gets the command to connect to the stream.
        /// </summary>
        public ICommand ToggleConnectionCommand { get; private set; }

        /// <summary>
        /// Gets the image indicating the current connection status.
        /// </summary>
        public BitmapSource ConnectionImage
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
        /// Gets or sets a value indicating whether or not there is an active Stream connection.
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
        /// Gets or sets the previous cheat votes.
        /// </summary>
        private IEnumerable<CheatVotes> PreviousCheatVotes { get; set; }

        /// <summary>
        /// Gets or sets and object to access votes.
        /// </summary>
        private Object VoteLock { get; set; }

        /// <summary>
        /// Gets a singleton instance of the <see cref="StreamConfigViewModel"/> class.
        /// </summary>
        /// <returns>A singleton instance of the class.</returns>
        public static StreamConfigViewModel GetInstance()
        {
            return StreamConfigViewModel.streamConfigViewModelInstance.Value;
        }

        /// <summary>
        /// Called when the cheat list changes.
        /// </summary>
        public void OnCheatListChange()
        {
            lock (this.VoteLock)
            {
                this.PreviousCheatVotes = null;
            }
        }

        /// <summary>
        /// Event fired when the browse view navigates to a new page.
        /// </summary>
        /// <param name="browsePage">The new browse page.</param>
        public void OnNavigate(BrowsePage browsePage)
        {
            switch (browsePage)
            {
                case BrowsePage.StreamHome:
                    break;
                default:
                    return;
            }
        }

        /// <summary>
        /// Toggles the current Stream connection.
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

            try
            {
                lock (this.VoteLock)
                {
                    if (!this.IsConnected || !BrowseViewModel.GetInstance().IsLoggedIn)
                    {
                        return;
                    }

                    IEnumerable<CheatVotes> cheatVotes = SqualrApi.GetStreamActivationIds(SettingsViewModel.GetInstance().TwitchChannel);
                    IEnumerable<ProjectItem> candidateProjectItems = ProjectExplorerViewModel.GetInstance().ProjectItems;

                    if (this.PreviousCheatVotes == null)
                    {
                        this.PreviousCheatVotes = cheatVotes;
                        return;
                    }

                    // Get cheat IDs to activate based on increased vote counts
                    IEnumerable<Int32> cheatIdsToActivate = cheatVotes
                          .Join(
                              this.PreviousCheatVotes,
                              currentVote => currentVote.CheatId,
                              previousVote => previousVote.CheatId,
                              (currentVote, previousVote) => new { cheatId = currentVote.CheatId, currentCount = currentVote.VoteCount, previousCount = previousVote.VoteCount })
                          .Where(combinedVote => combinedVote.currentCount != combinedVote.previousCount)
                          .Select(combinedVote => combinedVote.cheatId);

                    // Add in new votes with no previous vote count
                    cheatIdsToActivate = cheatVotes
                        .Select(vote => vote.CheatId)
                        .Except(this.PreviousCheatVotes.Select(vote => vote.CheatId))
                        .Concat(cheatIdsToActivate)
                        .Distinct();

                    IEnumerable<ProjectItem> projectItemsToActivate = cheatIdsToActivate
                          .Join(
                              candidateProjectItems,
                              cheatId => cheatId,
                              projectItem => projectItem.AssociatedCheat?.CheatId,
                              (cheatId, projectItem) => projectItem);

                    IEnumerable<ProjectItem> projectItemsToDeactivate = cheatVotes
                          .Join(
                              candidateProjectItems,
                              cheatVote => cheatVote.CheatId,
                              projectItem => projectItem.AssociatedCheat?.CheatId,
                              (cheatId, projectItem) => projectItem)
                          .Except(projectItemsToActivate);

                    // Handle activations
                    projectItemsToActivate.ForEach(item =>
                    {
                        item.IsActivated = true;

                        // Reset duration always
                        if (item is ScriptItem)
                        {
                            (item as ScriptItem).CurrentDuration = 0.0f;
                        }
                    });

                    // Notify which project items were activated such that Squalr can update the stream overlay
                    if (projectItemsToActivate.Count() > 0)
                    {
                        Task.Run(() =>
                        {
                            IEnumerable<ProjectItem> activatedProjectItems = candidateProjectItems
                                .Select(item => item)
                                .Where(item => !item.AssociatedCheat.IsStreamDisabled)
                                .Where(item => item.IsActivated);

                            IEnumerable<OverlayMeta> overlayMeta = activatedProjectItems
                                .Select(item => new OverlayMeta(item.AssociatedCheat.CheatId, item.AssociatedCheat.Cooldown, item.AssociatedCheat.Duration));

                            if (overlayMeta.Count() > 0)
                            {
                                try
                                {
                                    AccessTokens accessTokens = SettingsViewModel.GetInstance().AccessTokens;
                                    SqualrApi.UpdateOverlayMeta(accessTokens.AccessToken, overlayMeta.ToArray());
                                }
                                catch (Exception ex)
                                {
                                    OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Error, "Error updating overlay cooldowns and durations", ex);
                                }
                            }
                        });
                    }

                    this.PreviousCheatVotes = cheatVotes;
                }
            }
            catch (Exception ex)
            {
                OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Warn, "Error fetching activated cheats", ex);
            }
        }

        /// <summary>
        /// Connects to the stream service.
        /// </summary>
        private void Connect()
        {
            this.IsConnected = true;
        }

        /// <summary>
        /// Disconnects from the stream service.
        /// </summary>
        private void Disconnect()
        {
            this.IsConnected = false;
        }
    }
    //// End class
}
//// End namespace