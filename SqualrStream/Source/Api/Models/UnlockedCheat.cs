namespace SqualrStream.Source.Api.Models
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    [KnownType(typeof(Cheat))]
    public class UnlockedCheat
    {
        public UnlockedCheat()
        {
        }

        [DataMember(Name = "cheat")]
        public Cheat Cheat { get; set; }

        [DataMember(Name = "remaining_coins")]
        public Int32 RemainingCoins { get; set; }
    }
    //// End class
}
//// End namespace