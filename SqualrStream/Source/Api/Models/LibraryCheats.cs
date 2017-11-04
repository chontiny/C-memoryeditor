namespace SqualrStream.Source.Api.Models
{
    using System.Runtime.Serialization;

    [DataContract]
    [KnownType(typeof(Cheat))]
    public class LibraryCheats
    {
        public LibraryCheats()
        {
        }

        [DataMember(Name = "cheats_in_library")]
        public Cheat[] CheatsInLibrary { get; set; }

        [DataMember(Name = "cheats_available")]
        public Cheat[] CheatsAvailable { get; set; }
    }
    //// End class
}
//// End namespace