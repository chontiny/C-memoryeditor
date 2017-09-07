namespace Squalr.Source.Api.Models
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    internal class TwitchAccessTokens
    {
        public TwitchAccessTokens()
        {
            this.AccessToken = String.Empty;
            this.RefreshToken = String.Empty;
        }

        [DataMember(Name = "access_token")]
        public String AccessToken { get; set; }

        [DataMember(Name = "refresh_token")]
        public String RefreshToken { get; set; }
    }
    //// End class
}
//// End namespace