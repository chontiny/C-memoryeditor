namespace Ana.Source.Project.ProjectItems
{
    using System;
    using System.ComponentModel;
    using System.Runtime.Serialization;

    /// <summary>
    /// A base class for all project items that can be added to the project explorer
    /// </summary>
    [KnownType(typeof(ProjectItem))]
    [KnownType(typeof(FolderItem))]
    [KnownType(typeof(ScriptItem))]
    [KnownType(typeof(AddressItem))]
    [DataContract]
    internal abstract class ProjectItem : INotifyPropertyChanged
    {
        [Browsable(false)]
        private FolderItem parent;

        [Browsable(false)]
        private String description;

        [Browsable(false)]
        private Boolean isActivated;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectItem" /> class.
        /// </summary>
        public ProjectItem() : this(String.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectItem" /> class.
        /// </summary>
        /// <param name="description">The description of the project item.</param>
        public ProjectItem(String description)
        {
            // Bypass setters/getters to avoid triggering any view updates in constructor
            this.description = description == null ? String.Empty : description;
            this.parent = null;
            this.IsActivated = false;
        }

        /// <summary>
        /// Occurs after a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets or sets the parent of this project item.
        /// </summary>
        [Browsable(false)]
        public FolderItem Parent
        {
            get
            {
                return this.parent;
            }

            set
            {
                this.parent = value;
                ProjectExplorerViewModel.GetInstance().HasUnsavedChanges = true;
                this.NotifyPropertyChanged(nameof(this.Parent));
            }
        }

        /// <summary>
        /// Gets or sets the description for this object.
        /// </summary>
        [DataMember]
        [Category("Properties"), DisplayName("Description"), Description("Description to be shown for the Project Items")]
        public String Description
        {
            get
            {
                return this.description;
            }

            set
            {
                this.description = value;
                ProjectExplorerViewModel.GetInstance().HasUnsavedChanges = true;
                this.NotifyPropertyChanged(nameof(this.Description));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not this item is activated.
        /// </summary>
        [Browsable(false)]
        public Boolean IsActivated
        {
            get
            {
                return this.CanActivate && this.isActivated;
            }

            set
            {
                if (!this.CanActivate)
                {
                    return;
                }

                this.isActivated = value;
                this.OnActivationChanged();
                this.NotifyPropertyChanged(nameof(this.IsActivated));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not this item can be activated.
        /// </summary>
        [Browsable(false)]
        public Boolean CanActivate
        {
            get
            {
                return this.IsActivatable();
            }
        }

        /// <summary>
        /// Gets or sets the time since this item was last activated.
        /// </summary>
        [Browsable(false)]
        private DateTime LastActivated { get; set; }

        /// <summary>
        /// Invoked when this object is deserialized.
        /// </summary>
        /// <param name="streamingContext">Streaming context</param>
        [OnDeserialized]
        public void OnDeserialized(StreamingContext streamingContext)
        {
        }

        /// <summary>
        /// Updates the project item.
        /// </summary>
        public abstract void Update();

        protected void NotifyPropertyChanged(String propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Deactivates this item without triggering the <see cref="OnActivationChanged" /> function.
        /// </summary>
        protected void ResetActivation()
        {
            this.isActivated = false;
            this.NotifyPropertyChanged(nameof(this.IsActivated));
        }

        protected virtual void OnActivationChanged()
        {
        }

        protected virtual Boolean IsActivatable()
        {
            return true;
        }
    }
    //// End class
}
//// End namespace