namespace SqualrStream.Source.Api.Models
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    public class Game
    {
        public Game()
        {
        }

        [DataMember(Name = "id")]
        public Int32 GameId { get; set; }

        [DataMember(Name = "game_name")]
        public String GameName { get; set; }

        [DataMember(Name = "game_mode")]
        public Int32 GameMode { get; set; }
    }
    //// End class
}
//// End namespace