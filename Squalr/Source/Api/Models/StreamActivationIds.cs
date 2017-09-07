namespace Squalr.Source.Api.Models
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    internal class StreamActivationIds
    {
        public StreamActivationIds()
        {
            this.ActivatedIds = null;
            this.DeactivatedIds = null;
        }

        [DataMember(Name = "activation_ids")]
        public Guid[] ActivatedIds { get; set; }

        [DataMember(Name = "deactivation_ids")]
        public Guid[] DeactivatedIds { get; set; }
    }
    //// End class
}
//// End namespace