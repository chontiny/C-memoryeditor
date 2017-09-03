namespace Squalr.Source.Api.Models
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    internal class TwitchUser
    {
        [DataMember]
        private String name;

        [DataMember]
        private String displayName;

        [DataMember]
        private String email;

        public TwitchUser()
        {
            this.name = String.Empty;
            this.displayName = String.Empty;
            this.email = String.Empty;
        }

        public String Name
        {
            get
            {
                return this.name;
            }

            private set
            {
                this.name = value;
            }
        }

        public String DisplayName
        {
            get
            {
                return this.displayName;
            }

            private set
            {
                this.displayName = value;
            }
        }

        public String Email
        {
            get
            {
                return this.email;
            }

            private set
            {
                this.email = value;
            }
        }
    }
    //// End class
}
//// End namespace