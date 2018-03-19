namespace Squalr.Source.Docking
{
    using GalaSoft.MvvmLight;
    using System;
    using System.Windows.Media;

    /// <summary>
    /// View model for dockable panes.
    /// </summary>
    public abstract class PaneViewModel : ViewModelBase
    {
        /// <summary>
        /// The pane title.
        /// </summary>
        private String title = null;

        /// <summary>
        /// Flag indicating whether or not the pane is selected.
        /// </summary>
        private Boolean isSelected = false;

        /// <summary>
        /// Flag indicating whether or not the pane is active.
        /// </summary>
        private Boolean isActive = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="PaneViewModel" /> class.
        /// </summary>
        public PaneViewModel()
        {
        }

        /// <summary>
        /// Gets or sets the title of the pane.
        /// </summary>
        public String Title
        {
            get
            {
                return this.title;
            }

            set
            {
                if (this.title != value)
                {
                    this.title = value;
                    this.RaisePropertyChanged(nameof(this.Title));
                }
            }
        }

        /// <summary>
        /// Gets or sets the icon associated with the pane.
        /// </summary>
        public ImageSource IconSource { get; protected set; }

        /// <summary>
        /// Gets the content id associated with the pane.
        /// </summary>
        public String ContentId
        {
            get
            {
                return this.GetType().Name;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the pane is selected.
        /// </summary>
        public Boolean IsSelected
        {
            get
            {
                return this.isSelected;
            }

            set
            {
                if (this.isSelected != value)
                {
                    this.isSelected = value;
                    this.RaisePropertyChanged(nameof(this.IsSelected));
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the pane is active.
        /// </summary>
        public Boolean IsActive
        {
            get
            {
                return this.isActive;
            }

            set
            {
                if (this.isActive != value)
                {
                    this.isActive = value;
                    this.RaisePropertyChanged(nameof(this.IsActive));
                }
            }
        }
    }
    //// End class
}
//// End namespace