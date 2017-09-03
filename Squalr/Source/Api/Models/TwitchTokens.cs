namespace Squalr.Source.Api.Models
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    internal class TwitchTokens
    {
        [DataMember]
        private String accessToken;

        [DataMember]
        private String refreshToken;

        public TwitchTokens()
        {
            this.accessToken = String.Empty;
            this.refreshToken = String.Empty;
        }

        public String AccessToken
        {
            get
            {
                return this.accessToken;
            }

            private set
            {
                this.accessToken = value;
            }
        }

        public String RefreshToken
        {
            get
            {
                return this.refreshToken;
            }

            private set
            {
                this.refreshToken = value;
            }
        }
    }
    //// End class
}
//// End namespace