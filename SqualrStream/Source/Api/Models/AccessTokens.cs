namespace SqualrStream.Source.Api.Models
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    public class AccessTokens
    {
        public AccessTokens()
        {
        }

        [DataMember(Name = "access_token")]
        public String AccessToken { get; set; }

        [DataMember(Name = "refresh_token")]
        public String RefreshToken { get; set; }
    }
    //// End class
}
//// End namespace