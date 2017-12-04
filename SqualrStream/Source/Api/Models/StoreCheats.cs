namespace SqualrStream.Source.Api.Models
{
    using System.Runtime.Serialization;

    [DataContract]
    [KnownType(typeof(Cheat))]
    public class StoreCheats
    {
        public StoreCheats()
        {
        }

        [DataMember(Name = "locked_cheats")]
        public Cheat[] LockedCheats { get; set; }

        [DataMember(Name = "unlocked_cheats")]
        public Cheat[] UnlockedCheats { get; set; }
    }
    //// End class
}
//// End namespace