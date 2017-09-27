namespace Squalr.Source.Api.Models
{
    using System.Runtime.Serialization;

    [DataContract]
    [KnownType(typeof(Cheat))]
    internal class StoreCheats
    {
        public StoreCheats()
        {
            this.LockedCheats = null;
            this.UnlockedCheats = null;
        }

        [DataMember(Name = "locked_cheats")]
        public Cheat[] LockedCheats { get; set; }

        [DataMember(Name = "unlocked_cheats")]
        public Cheat[] UnlockedCheats { get; set; }
    }
    //// End class
}
//// End namespace