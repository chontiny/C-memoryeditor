namespace Squalr.Source.Api.Models
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    internal class Vote
    {
        public Vote()
        {
            this.Coins = 0;
            this.Name = String.Empty;
            this.DisplayName = String.Empty;
            this.Email = String.Empty;
        }

        [DataMember(Name = "coins")]
        public Int32 Coins { get; set; }

        [DataMember(Name = "name")]
        public String Name { get; set; }

        [DataMember(Name = "displayName")]
        public String DisplayName { get; set; }

        [DataMember(Name = "email")]
        public String Email { get; set; }
    }
    //// End class
}
//// End namespace