namespace SqualrStream.Source.Api.Models
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    public class OverlayMeta
    {
        public OverlayMeta()
        {
        }

        public OverlayMeta(Int32 cheatId, Single cooldown, Single duration)
        {
            this.CheatId = cheatId;
            this.Cooldown = cooldown;
            this.Duration = duration;
        }

        [DataMember(Name = "cheat_id")]
        public Int32 CheatId { get; set; }

        [DataMember(Name = "cooldown")]
        public Single Cooldown { get; set; }

        [DataMember(Name = "duration")]
        public Single Duration { get; set; }
    }
    //// End class
}
//// End namespace