namespace SqualrClient.Source.Api.Models
{
    using SqualrClient.Properties;
    using SqualrCore.Content;
    using SqualrCore.Source.Controls;
    using SqualrCore.Source.Output;
    using SqualrCore.Source.ProjectItems;
    using SqualrCore.Source.Utils.Extensions;
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Json;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Media.Imaging;

    [DataContract]
    public class Cheat : INotifyPropertyChanged
    {
        /// <summary>
        /// A value indicating if this script is enabled for stream interaction.
        /// </summary>
        [Browsable(false)]
        protected Boolean isStreamDisabled;

        /// <summary>
        /// The cooldown in milliseconds of this cheat.
        /// </summary>
        [Browsable(false)]
        protected Single cooldown;

        /// <summary>
        /// The duration in milliseconds of this cheat.
        /// </summary>
        [Browsable(false)]
        protected Single duration;

        /// <summary>
        /// The stream icon path associated with this cheat.
        /// </summary>
        [DataMember(Name = "icon")]
        private String iconName;

        /// <summary>
        /// The current cooldown for this script item.
        /// </summary>
        private Single currentCooldown;

        /// <summary>
        /// The current duration for this script item.
        /// </summary>
        private Single currentDuration;

        public event PropertyChangedEventHandler PropertyChanged;

        public Cheat()
        {
            this.LastUpdate = DateTime.MinValue;
        }

        [Browsable(false)]
        [DataMember(Name = "id")]
        public Int32 CheatId { get; set; }

        [Browsable(false)]
        [DataMember(Name = "user_id")]
        public Int32 UserId { get; set; }

        [Browsable(false)]
        [DataMember(Name = "game_id")]
        public Int32 GameId { get; set; }

        [Browsable(false)]
        [DataMember(Name = "library_id")]
        public Int32 LibraryId { get; set; }

        [Browsable(false)]
        [DataMember(Name = "game_distributor_id")]
        public Int32 GameDistributorId { get; set; }

        [Browsable(true)]
        [DataMember(Name = "cheat_name")]
        public String CheatName { get; set; }

        [Browsable(true)]
        [DataMember(Name = "cheat_description")]
        public String CheatDescription { get; set; }

        [Browsable(false)]
        [DataMember(Name = "cheat_payload")]
        public String CheatPayload { get; set; }

        [Browsable(false)]
        [DataMember(Name = "cost")]
        public Int32 Cost { get; set; }

        [Browsable(false)]
        public Boolean IsFree
        {
            get
            {
                return this.Cost == 0;
            }
        }

        [Browsable(false)]
        public Boolean IsPaid
        {
            get
            {
                return !this.IsFree;
            }
        }

        [Browsable(false)]
        [DataMember(Name = "in_review")]
        public Boolean InReview { get; set; }

        [Browsable(false)]
        [DataMember(Name = "in_market")]
        public Boolean InMarket { get; set; }

        [Browsable(false)]
        [DataMember(Name = "default_cooldown")]
        public Single DefaultCooldown { get; set; }

        [Browsable(false)]
        [DataMember(Name = "default_duration")]
        public Single DefaultDuration { get; set; }

        [Browsable(false)]
        [DataMember(Name = "default_is_stream_disabled")]
        public Boolean DefaultIsStreamDisabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if this script is disabled for stream interaction.
        /// </summary>
        [DataMember]
        [Browsable(true)]
        [SortedCategory(SortedCategory.CategoryType.Stream), DisplayName("Stream Disabled"), Description("Indicates whether this item is available for activation via stream commands.")]
        public Boolean IsStreamDisabled
        {
            get
            {
                return this.isStreamDisabled;
            }

            set
            {
                if (this.isStreamDisabled == value)
                {
                    return;
                }

                this.isStreamDisabled = value;

                // ProjectExplorerViewModel.GetInstance().ProjectItemStorage.HasUnsavedChanges = true;
                this.NotifyPropertyChanged(nameof(this.IsStreamDisabled));
                this.NotifyPropertyChanged(nameof(this.IconName));
                // ProjectExplorerViewModel.GetInstance().OnPropertyUpdate();

                this.UpdateStreamMeta();
            }
        }

        /// <summary>
        /// Gets or sets the coolodown in milliseconds of this project item.
        /// </summary>
        [DataMember]
        [Browsable(true)]
        [SortedCategory(SortedCategory.CategoryType.Stream), DisplayName("Cooldown"), Description("The cooldown (in milliseconds) for stream activation for this project item.")]
        public Single Cooldown
        {
            get
            {
                return this.cooldown;
            }

            set
            {
                if (this.cooldown == value)
                {
                    return;
                }

                this.cooldown = value;

                // ProjectExplorerViewModel.GetInstance().ProjectItemStorage.HasUnsavedChanges = true;
                this.NotifyPropertyChanged(nameof(this.Cooldown));
                // ProjectExplorerViewModel.GetInstance().OnPropertyUpdate();

                this.UpdateStreamMeta();
            }
        }

        /// <summary>
        /// Gets or sets the coolodown in milliseconds of this project item.
        /// </summary>
        [DataMember]
        [Browsable(true)]
        [SortedCategory(SortedCategory.CategoryType.Stream), DisplayName("Duration"), Description("The duration (in milliseconds) for stream activation for this project item.")]
        public Single Duration
        {
            get
            {
                return this.duration;
            }

            set
            {
                if (this.duration == value)
                {
                    return;
                }

                this.duration = value;

                // ProjectExplorerViewModel.GetInstance().ProjectItemStorage.HasUnsavedChanges = true;
                this.NotifyPropertyChanged(nameof(this.Duration));
                // ProjectExplorerViewModel.GetInstance().OnPropertyUpdate();

                this.UpdateStreamMeta();
            }
        }

        /// <summary>
        /// Gets or sets the stream icon for this project item.
        /// </summary>
        [DataMember]
        [Browsable(true)]
        [SortedCategory(SortedCategory.CategoryType.Stream), DisplayName("Stream Icon"), Description("The stream icon for this item")]
        public String IconName
        {
            get
            {
                return this.iconName;
            }

            set
            {
                if (this.iconName == value)
                {
                    return;
                }

                this.iconName = value;

                this.NotifyPropertyChanged(nameof(this.IconName));
                this.NotifyPropertyChanged(nameof(this.Icon));
                // ProjectExplorerViewModel.GetInstance().OnPropertyUpdate();

                this.UpdateStreamMeta();
            }
        }

        /// <summary>
        /// Gets the image associated with this project item.
        /// </summary>
        [Browsable(false)]
        public BitmapSource Icon
        {
            get
            {
                BitmapSource displayIcon = null;

                if (this.IsStreamDisabled)
                {
                    return Images.Cancel;
                }

                return displayIcon ?? Images.Script;
            }
        }

        /// <summary>
        /// Gets or sets the current cooldown for this script item.
        /// </summary>
        [Browsable(false)]
        public Single CurrentCooldown
        {
            get
            {
                return this.currentCooldown;
            }

            set
            {
                this.currentCooldown = value;
                this.NotifyPropertyChanged(nameof(this.CurrentCooldown));
                this.NotifyPropertyChanged(nameof(this.CooldownProgress));
                // ProjectExplorerViewModel.GetInstance().OnPropertyUpdate();
            }
        }

        /// <summary>
        /// Gets or sets the current duration for this script item.
        /// </summary>
        [Browsable(false)]
        public Single CurrentDuration
        {
            get
            {
                return this.currentDuration;
            }

            set
            {
                this.currentDuration = value;
                this.NotifyPropertyChanged(nameof(this.CurrentDuration));
                this.NotifyPropertyChanged(nameof(this.DurationProgress));
                // ProjectExplorerViewModel.GetInstance().OnPropertyUpdate();
            }
        }

        /// <summary>
        /// Gets the progress of the current cooldown out of the maximum cooldown.
        /// </summary>
        [Browsable(false)]
        public Single CooldownProgress
        {
            get
            {
                return this.Cooldown <= 0.0f ? 0.0f : (this.CurrentCooldown / this.Cooldown);
            }
        }

        /// <summary>
        /// Gets the progress of the current duration out of the maximum duration.
        /// </summary>
        [Browsable(false)]
        public Single DurationProgress
        {
            get
            {
                return 1.0f - (this.Duration <= 0.0f ? 0.0f : (this.CurrentDuration / this.Duration));
            }
        }

        /// <summary>
        /// Updates the cooldown for this script.
        /// </summary>
        private void UpdateCooldown(Single elapsedTime)
        {
            if (this.IsStreamDisabled)
            {
                return;
            }

            // Update current cooldown
            this.CurrentCooldown = (this.CurrentCooldown - elapsedTime).Clamp(0, this.Cooldown);

        }

        private void UpdateDuration(Single elapsedTime)
        {
            if (this.IsStreamDisabled || !this.ProjectItem.IsActivated)
            {
                return;
            }

            // Update current duration
            this.CurrentDuration = (this.CurrentDuration + elapsedTime).Clamp(0, this.Duration);

            // Deactivate if exceeding the duration
            if (this.CurrentDuration >= this.Duration)
            {
                this.ProjectItem.IsActivated = false;
            }
        }

        [Browsable(false)]
        public ProjectItem ProjectItem { get; set; }

        /// <summary>
        /// Gets or sets the time since the last cooldown update.
        /// </summary>
        private DateTime LastUpdate { get; set; }

        /// <summary>
        /// Invoked when this object is deserialized.
        /// </summary>
        /// <param name="streamingContext">Streaming context.</param>
        [OnDeserialized]
        public void OnDeserialized(StreamingContext streamingContext)
        {
            this.LastUpdate = DateTime.MinValue;

            if (this.CheatPayload.IsNullOrEmpty())
            {
                return;
            }

            // Deserialize the payload of the inner project item
            using (MemoryStream memoryStream = new MemoryStream(Encoding.ASCII.GetBytes(this.CheatPayload)))
            {
                DataContractJsonSerializer deserializer = new DataContractJsonSerializer(typeof(ProjectItem));

                this.ProjectItem = deserializer.ReadObject(memoryStream) as ProjectItem;
            }
        }

        public void Update()
        {

            DateTime currentTime = DateTime.Now;

            if (this.LastUpdate == DateTime.MinValue)
            {
                this.LastUpdate = currentTime;
            }

            Single elapsedTime = (Single)(currentTime - this.LastUpdate).TotalSeconds;

            this.UpdateCooldown(elapsedTime);
            this.UpdateDuration(elapsedTime);

            this.LastUpdate = currentTime;
        }

        private void UpdateStreamMeta()
        {
            Task.Run(() =>
            {
                try
                {
                    SqualrApi.UpdateCheatStreamMeta(SettingsViewModel.GetInstance().AccessTokens?.AccessToken, this);
                }
                catch (Exception ex)
                {
                    OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Error, "Error updating stream with local change. Try again.", ex);
                }
            });
        }

        /// <summary>
        /// Indicates that a given property in this project item has changed.
        /// </summary>
        /// <param name="propertyName">The name of the changed property.</param>
        protected void NotifyPropertyChanged(String propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    //// End class
}
//// End namespace