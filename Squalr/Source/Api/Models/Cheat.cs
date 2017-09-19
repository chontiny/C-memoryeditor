namespace Squalr.Source.Api.Models
{
    using Squalr.Source.ProjectExplorer.ProjectItems;
    using Squalr.Source.Utils.Extensions;
    using System;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Json;
    using System.Text;

    [DataContract]
    internal class Cheat
    {
        public Cheat()
        {
            this.CheatId = 0;
            this.UserId = 0;
            this.GameId = 0;
            this.LibraryId = 0;
            this.GameDistributorId = 0;
            this.CheatName = String.Empty;
            this.CheatDescription = String.Empty;
            this.ProjectItem = null;
            this.Cost = 0;
            this.StreamCommand = String.Empty;
            this.StreamIcon = String.Empty;
            this.InReview = true;
            this.InMarket = true;
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

        [DataMember(Name = "stream_command")]
        public String StreamCommand { get; set; }

        [DataMember(Name = "stream_icon")]
        public String StreamIcon { get; set; }

        [DataMember(Name = "in_review")]
        public Boolean InReview { get; set; }

        [DataMember(Name = "in_market")]
        public Boolean InMarket { get; set; }

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
                this.ProjectItem.AssociatedCheat = this;
            }
        }
    }
    //// End class
}
//// End namespace