namespace Squalr.Source.Api.Models
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    internal class Cheat
    {
        public Cheat()
        {
            this.CheatId = 0;
            this.UserId = 0;
            this.GameId = 0;
            this.GameDistributorId = 0;
            this.CheatName = String.Empty;
            this.CheatDescription = String.Empty;
            this.CheatPayload = String.Empty;
            this.Cost = 0;
            this.CheatPayload = String.Empty;
            this.StreamCommand = String.Empty;
            this.StreamIcon = String.Empty;
            this.InReview = true;
            this.InMarket = true;
        }

        [DataMember(Name = "id")]
        public Int32 CheatId { get; set; }

        [DataMember(Name = "user_id")]
        public Int32 UserId { get; set; }

        [DataMember(Name = "game_id")]
        public Int32 GameId { get; set; }

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
    }
    //// End class
}
//// End namespace