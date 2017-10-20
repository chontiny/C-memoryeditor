namespace SqualrClient.Source.Api.Models
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    internal class Library
    {
        public Library()
        {
            this.LibraryId = 0;
            this.UserId = 0;
            this.GameId = 0;
            this.LibraryName = String.Empty;
        }

        [DataMember(Name = "id")]
        public Int32 LibraryId { get; set; }

        [DataMember(Name = "user_id")]
        public Int32 UserId { get; set; }

        [DataMember(Name = "game_id")]
        public Int32 GameId { get; set; }

        [DataMember(Name = "library_name")]
        public String LibraryName { get; set; }
    }
    //// End class
}
//// End namespace