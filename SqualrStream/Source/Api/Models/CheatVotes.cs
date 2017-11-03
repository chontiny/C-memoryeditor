namespace SqualrStream.Source.Api.Models
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    public class CheatVotes
    {
        public CheatVotes()
        {
        }

        [DataMember(Name = "cheat_id")]
        public Int32 CheatId { get; set; }

        [DataMember(Name = "vote_count")]
        public Int32 VoteCount { get; set; }
    }
    //// End class
}
//// End namespace