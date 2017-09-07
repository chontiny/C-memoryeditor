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
    using System.Windows.Input;
    using System.Windows.Media.Imaging;

    /// <summary>
    /// View model for the Stream Config.
    /// </summary>
    internal class StreamConfigViewModel : ToolViewModel
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
        /// Prevents a default instance of the <see cref="StreamConfigViewModel" /> class from being created.
        /// </summary>
        private StreamConfigViewModel() : base("Stream Config")
        {
            this.ContentId = StreamConfigViewModel.ToolContentId;

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
        /// Gets a singleton instance of the <see cref="StreamConfigViewModel"/> class.
        /// </summary>
        /// <returns>A singleton instance of the class.</returns>
        public static StreamConfigViewModel GetInstance()
        {
            return StreamConfigViewModel.streamConfigViewModelInstance.Value;
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
                StreamActivationIds streamActivationIds = SqualrApi.GetStreamActivationIds(SettingsViewModel.GetInstance().TwitchChannel);
                IEnumerable<ProjectItem> candidateProjectItems = ProjectExplorerViewModel.GetInstance().ProjectRoot.Flatten();

                // Use the given ids to determine which project items to activate
                var itemsToActivate = streamActivationIds.ActivatedIds
                     .Join(
                         candidateProjectItems,
                         activatedId => activatedId,
                         projectItem => projectItem.Guid,
                         (guid, projectItem) => new { projectItem = projectItem, guid = guid });

                // Use the given ids to determine which project items to deactivate
                var itemsToDeactivate = streamActivationIds.DeactivatedIds
                     .Join(
                         candidateProjectItems,
                         activatedId => activatedId,
                         projectItem => projectItem.Guid,
                         (guid, projectItem) => new { projectItem = projectItem, guid = guid });

                // Handle deactivations
                itemsToDeactivate.ForEach(item => item.projectItem.IsActivated = false);

                // Handle activations
                itemsToActivate.ForEach(item => item.projectItem.IsActivated = true);
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