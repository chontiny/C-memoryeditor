namespace SqualrStream.View
{
    using Source.Main;
    using SqualrStream.Properties;
    using SqualrStream.Source.Browse.TwitchLogin;
    using SqualrStream.Source.Editors.StreamIconEditor;
    using SqualrStream.Source.Library;
    using SqualrStream.Source.Navigation;
    using SqualrStream.Source.Store;
    using SqualrStream.Source.Stream;

    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    internal class ViewModelLocator : SqualrCore.View.ViewModelLocator
    {
        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
        }

        /// <summary>
        /// Gets the Settings view model.
        /// </summary>
        public SettingsViewModel SettingsViewModel
        {
            get
            {
                return SettingsViewModel.GetInstance();
            }
        }

        /// <summary>
        /// Gets the Main view model.
        /// </summary>
        public MainViewModel MainViewModel
        {
            get
            {
                return MainViewModel.GetInstance();
            }
        }

        /// <summary>
        /// Gets the Browser view model.
        /// </summary>
        public BrowseViewModel BrowseViewModel
        {
            get
            {
                return BrowseViewModel.GetInstance();
            }
        }

        /// <summary>
        /// Gets the Store view model.
        /// </summary>
        public StoreViewModel StoreViewModel
        {
            get
            {
                return StoreViewModel.GetInstance();
            }
        }

        /// <summary>
        /// Gets the Library view model.
        /// </summary>
        public LibraryViewModel LibraryViewModel
        {
            get
            {
                return LibraryViewModel.GetInstance();
            }
        }

        /// <summary>
        /// Gets the Twitch Login view model.
        /// </summary>
        public TwitchLoginViewModel TwitchLoginViewModel
        {
            get
            {
                return TwitchLoginViewModel.GetInstance();
            }
        }

        /// <summary>
        /// Gets the Stream Config view model.
        /// </summary>
        public StreamConfigViewModel StreamConfigViewModel
        {
            get
            {
                return StreamConfigViewModel.GetInstance();
            }
        }

        /// <summary>
        /// Gets the Stream Icon Editor view model.
        /// </summary>
        public StreamIconEditorViewModel StreamIconEditorViewModel
        {
            get
            {
                return StreamIconEditorViewModel.GetInstance();
            }
        }
    }
    //// End class
}
//// End namespace