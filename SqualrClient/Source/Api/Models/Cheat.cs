namespace SqualrClient.Source.Api.Models
{
    using SqualrClient.Properties;
    using SqualrCore.Source.Output;
    using SqualrCore.Source.ProjectExplorer.ProjectItems;
    using SqualrCore.Source.Utils.Extensions;
    using System;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Json;
    using System.Text;
    using System.Threading.Tasks;

    [DataContract]
    internal class Cheat
    {
        [DataMember(Name = "is_stream_disabled")]
        private Boolean isStreamDisabled;

        [DataMember(Name = "cooldown")]
        private Single cooldown;

        [DataMember(Name = "duration")]
        private Single duration;

        [DataMember(Name = "icon")]
        private String icon;

        public Cheat()
        {
        }

        public ProjectItem ProjectItem;

        [DataMember(Name = "id")]
        public Int32 CheatId { get; set; }

        [DataMember(Name = "user_id")]
        public Int32 UserId { get; set; }

        [DataMember(Name = "game_id")]
        public Int32 GameId { get; set; }

        [DataMember(Name = "library_id")]
        public Int32 LibraryId { get; set; }

        [DataMember(Name = "game_distributor_id")]
        public Int32 GameDistributorId { get; set; }

        [DataMember(Name = "cheat_name")]
        public String CheatName { get; set; }

        [DataMember(Name = "cheat_description")]
        public String CheatDescription { get; set; }

        [DataMember(Name = "cheat_payload")]
        public String CheatPayload { get; set; }

        [DataMember(Name = "cost")]
        public Int32 Cost { get; set; }

        public Boolean IsFree
        {
            get
            {
                return this.Cost == 0;
            }
        }

        public Boolean IsPaid
        {
            get
            {
                return !this.IsFree;
            }
        }

        public Boolean IsStreamDisabled
        {
            get
            {
                return this.isStreamDisabled;
            }

            set
            {
                this.isStreamDisabled = value;
                this.UpdateStreamMeta();
            }
        }

        public Single Cooldown
        {
            get
            {
                return this.cooldown;
            }

            set
            {
                this.cooldown = value;
                this.UpdateStreamMeta();
            }
        }

        public Single Duration
        {
            get
            {
                return this.duration;
            }

            set
            {
                this.duration = value;
                this.UpdateStreamMeta();
            }
        }

        public String Icon
        {
            get
            {
                return this.icon;
            }

            set
            {
                this.icon = value;
                this.UpdateStreamMeta();
            }
        }

        [DataMember(Name = "in_review")]
        public Boolean InReview { get; set; }

        [DataMember(Name = "in_market")]
        public Boolean InMarket { get; set; }

        [DataMember(Name = "default_cooldown")]
        public Single DefaultCooldown { get; set; }

        [DataMember(Name = "default_duration")]
        public Single DefaultDuration { get; set; }

        [DataMember(Name = "default_is_stream_disabled")]
        public Boolean DefaultIsStreamDisabled { get; set; }

        /// <summary>
        /// Invoked when this object is deserialized.
        /// </summary>
        /// <param name="streamingContext">Streaming context.</param>
        [OnDeserialized]
        public void OnDeserialized(StreamingContext streamingContext)
        {
            if (this.CheatPayload.IsNullOrEmpty())
            {
                return;
            }

            // Deserialize the payload of the inner project item
            using (MemoryStream memoryStream = new MemoryStream(Encoding.ASCII.GetBytes(this.CheatPayload)))
            {
                DataContractJsonSerializer deserializer = new DataContractJsonSerializer(typeof(ProjectItem));

                this.ProjectItem = deserializer.ReadObject(memoryStream) as ProjectItem;
                throw new NotImplementedException();
                // this.ProjectItem.AssociateCheat(this);
            }
        }

        /// <summary>
        /// Loads the default stream settings into the associated project item.
        /// </summary>
        public void LoadDefaultStreamSettings()
        {
            throw new NotImplementedException();
            // this.ProjectItem.AssociateCheat(this, loadDefaults: true);
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
    }
    //// End class
}
//// End namespace