namespace Squalr.Source.Browse
{
    using GalaSoft.MvvmLight.Command;
    using Squalr.Source.Docking;
    using Squalr.Source.Main;
    using System;
    using System.Threading;
    using System.Windows.Input;

    /// <summary>
    /// View model for the Browser.
    /// </summary>
    internal class BrowseViewModel : ToolViewModel
    {
        /// <summary>
        /// The content id for the docking library associated with this view model.
        /// </summary>
        public const String ToolContentId = nameof(BrowseViewModel);

        /// <summary>
        /// Singleton instance of the <see cref="BrowseViewModel" /> class.
        /// </summary>
        private static Lazy<BrowseViewModel> browseViewModelInstance = new Lazy<BrowseViewModel>(
                () => { return new BrowseViewModel(); },
                LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// A value indicating whether the store is visible.
        /// </summary>
        private Boolean isStoreVisible;

        /// <summary>
        /// A value indicating whether the library is visible.
        /// </summary>
        private Boolean isLibraryVisible;

        /// <summary>
        /// A value indicating whether the stream is visible.
        /// </summary>
        private Boolean isStreamVisible;

        /// <summary>
        /// Prevents a default instance of the <see cref="BrowseViewModel" /> class from being created.
        /// </summary>
        private BrowseViewModel() : base("Browse")
        {
            this.ContentId = BrowseViewModel.ToolContentId;

            this.OpenStoreCommand = new RelayCommand(() => this.IsStoreVisible = true, () => true);
            this.OpenLibraryCommand = new RelayCommand(() => this.IsLibraryVisible = true, () => true);
            this.OpenStreamCommand = new RelayCommand(() => this.IsStreamVisible = true, () => true);

            // Open the store as the default option
            this.OpenStoreCommand.Execute(null);

            MainViewModel.GetInstance().RegisterTool(this);
        }

        /// <summary>
        /// Gets a singleton instance of the <see cref="BrowseViewModel"/> class.
        /// </summary>
        /// <returns>A singleton instance of the class.</returns>
        public static BrowseViewModel GetInstance()
        {
            return browseViewModelInstance.Value;
        }

        /// <summary>
        /// Gets a command to open the store.
        /// </summary>
        public ICommand OpenStoreCommand { get; private set; }

        /// <summary>
        /// Gets a command to open the library.
        /// </summary>
        public ICommand OpenLibraryCommand { get; private set; }

        /// <summary>
        /// Gets a command to open the stream config.
        /// </summary>
        public ICommand OpenStreamCommand { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether the store is visible.
        /// </summary>
        public Boolean IsStoreVisible
        {
            get
            {
                return this.isStoreVisible;
            }

            set
            {
                if (value == true)
                {
                    this.HideAllSections();
                }

                this.isStoreVisible = value;
                this.RaisePropertyChanged(nameof(this.IsStoreVisible));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the library is visible.
        /// </summary>
        public Boolean IsLibraryVisible
        {
            get
            {
                return this.isLibraryVisible;
            }

            set
            {
                if (value == true)
                {
                    this.HideAllSections();
                }

                this.isLibraryVisible = value;
                this.RaisePropertyChanged(nameof(this.IsLibraryVisible));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the stream is visible.
        /// </summary>
        public Boolean IsStreamVisible
        {
            get
            {
                return this.isStreamVisible;
            }

            set
            {
                if (value == true)
                {
                    this.HideAllSections();
                }

                this.isStreamVisible = value;
                this.RaisePropertyChanged(nameof(this.IsStreamVisible));
            }
        }

        /// <summary>
        /// Hides all visible sections.
        /// </summary>
        private void HideAllSections()
        {
            if (this.IsStoreVisible)
            {
                this.IsStoreVisible = false;
            }

            if (this.IsLibraryVisible)
            {
                this.IsLibraryVisible = false;
            }

            if (this.IsStreamVisible)
            {
                this.IsStreamVisible = false;
            }
        }
    }
    //// End class
}
//// End namespace