namespace SqualrStream.Source.Api.Models
{
    using System;
    using System.ComponentModel;
    using System.Runtime.Serialization;

    [DataContract]
    public class Library : INotifyPropertyChanged
    {
        [DataMember(Name = "id")]
        private Int32 libraryId;

        [DataMember(Name = "user_id")]
        private Int32 userId;

        [DataMember(Name = "game_id")]
        private Int32 gameId;

        [DataMember(Name = "library_name")]
        private String libraryName;

        public Library()
        {
        }

        public Int32 LibraryId
        {
            get
            {
                return this.libraryId;
            }

            set
            {
                this.libraryId = value;
                this.RaisePropertyChanged(nameof(this.LibraryId));
            }
        }

        public Int32 UserId
        {
            get
            {
                return this.userId;
            }

            set
            {
                this.userId = value;
                this.RaisePropertyChanged(nameof(this.UserId));
            }
        }

        public Int32 GameId
        {
            get
            {
                return this.gameId;
            }

            set
            {
                this.gameId = value;
                this.RaisePropertyChanged(nameof(this.GameId));
            }
        }

        public String LibraryName
        {
            get
            {
                return this.libraryName;
            }

            set
            {
                this.libraryName = value;
                this.RaisePropertyChanged(nameof(this.LibraryName));
            }
        }

        /// <summary>
        /// Occurs after a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Indicates that a given property in this project item has changed.
        /// </summary>
        /// <param name="propertyName">The name of the changed property.</param>
        protected void RaisePropertyChanged(String propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    //// End class
}
//// End namespace