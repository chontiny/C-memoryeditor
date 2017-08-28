using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Squalr.Source.StreamWeaver
{
    [DataContract]
    internal class StreamActivationIds : INotifyPropertyChanged
    {
        private Guid[] activatedIds;

        private Guid[] deactivatedIds;

        public StreamActivationIds()
        {
        }

        [DataMember]
        public Guid[] ActivatedIds
        {
            get
            {
                return this.activatedIds;
            }

            set
            {
                this.activatedIds = value;
                this.NotifyPropertyChanged(nameof(this.ActivatedIds));
            }
        }

        [DataMember]
        public Guid[] DeactivatedIds
        {
            get
            {
                return this.deactivatedIds;
            }

            set
            {
                this.deactivatedIds = value;
                this.NotifyPropertyChanged(nameof(this.DeactivatedIds));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Indicates that a given property in this project item has changed.
        /// </summary>
        /// <param name="propertyName">The name of the changed property.</param>
        protected void NotifyPropertyChanged(String propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    //// End class
}
//// End namespace