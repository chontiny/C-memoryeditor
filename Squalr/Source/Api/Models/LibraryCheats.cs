namespace Squalr.Source.Api.Models
{
    using System.Runtime.Serialization;

    [DataContract]
    [KnownType(typeof(Cheat))]
    internal class LibraryCheats
    {
        public LibraryCheats()
        {
            this.CheatsInLibrary = null;
            this.CheatsAvailable = null;
        }

        [DataMember(Name = "cheats_in_library")]
        public Cheat[] CheatsInLibrary { get; set; }

        [DataMember(Name = "cheats_available")]
        public Cheat[] CheatsAvailable { get; set; }
    }
    //// End class
}
//// End namespace