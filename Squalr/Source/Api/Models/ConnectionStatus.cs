namespace Squalr.Source.Api.Models
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    internal class ConnectionStatus
    {
        public ConnectionStatus()
        {
        }

        [DataMember(Name = "connected")]
        public Boolean Connected { get; set; }
    }
    //// End class
}
//// End namespace