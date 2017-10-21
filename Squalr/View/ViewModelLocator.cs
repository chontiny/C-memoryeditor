namespace Squalr.View
{
    using Source.DotNetExplorer;
    using Source.Editors.HotkeyEditor;
    using Source.Editors.ScriptEditor;
    using Source.Editors.TextEditor;
    using Source.Editors.ValueEditor;
    using Source.Main;
    using Source.ProjectExplorer;
    using Source.Results.PointerScanResults;
    using Source.Results.ScanResults;
    using Source.Scanners.ChangeCounter;
    using Source.Scanners.InputCorrelator;
    using Source.Scanners.LabelThresholder;
    using Source.Scanners.ManualScanner;
    using Source.Scanners.PointerScanner;
    using Source.Scanners.ValueCollector;
    using Source.Snapshots;
    using Squalr.Properties;
    using Squalr.Source.Browse;
    using Squalr.Source.Browse.Library;
    using Squalr.Source.Browse.Store;
    using Squalr.Source.Browse.StreamConfig;
    using Squalr.Source.Browse.TwitchLogin;
    using Squalr.Source.Editors.StreamIconEditor;

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
        /// Gets the Change Counter view model.
        /// </summary>
        public ChangeCounterViewModel ChangeCounterViewModel
        {
            get
            {
                return ChangeCounterViewModel.GetInstance();
            }
        }

        /// <summary>
        /// Gets the Input Correlator view model.
        /// </summary>
        public InputCorrelatorViewModel InputCorrelatorViewModel
        {
            get
            {
                return InputCorrelatorViewModel.GetInstance();
            }
        }

        /// <summary>
        /// Gets the Label Thresholder view model.
        /// </summary>
        public LabelThresholderViewModel LabelThresholderViewModel
        {
            get
            {
                return LabelThresholderViewModel.GetInstance();
            }
        }

        /// <summary>
        /// Gets the Manual Scanner view model.
        /// </summary>
        public ManualScannerViewModel ManualScannerViewModel
        {
            get
            {
                return ManualScannerViewModel.GetInstance();
            }
        }

        /// <summary>
        /// Gets the Pointer Scanner view model.
        /// </summary>
        public PointerScannerViewModel PointerScannerViewModel
        {
            get
            {
                return PointerScannerViewModel.GetInstance();
            }
        }

        /// <summary>
        /// Gets the Snapshot Manager view model.
        /// </summary>
        public SnapshotManagerViewModel SnapshotManagerViewModel
        {
            get
            {
                return SnapshotManagerViewModel.GetInstance();
            }
        }

        /// <summary>
        /// Gets the Scan Results view model.
        /// </summary>
        public ScanResultsViewModel ScanResultsViewModel
        {
            get
            {
                return ScanResultsViewModel.GetInstance();
            }
        }

        /// <summary>
        /// Gets the Pointer Scan Results view model.
        /// </summary>
        public PointerScanResultsViewModel PointerScanResultsViewModel
        {
            get
            {
                return PointerScanResultsViewModel.GetInstance();
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
        /// Gets the .Net Explorer view model.
        /// </summary>
        public DotNetExplorerViewModel DotNetExplorerViewModel
        {
            get
            {
                return DotNetExplorerViewModel.GetInstance();
            }
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
        /// Gets the Project Explorer view model.
        /// </summary>
        public ProjectExplorerViewModel ProjectExplorerViewModel
        {
            get
            {
                return ProjectExplorerViewModel.GetInstance();
            }
        }

        /// <summary>
        /// Gets the Project Explorer view model.
        /// </summary>
        public ValueCollectorViewModel ValueCollectorViewModel
        {
            get
            {
                return ValueCollectorViewModel.GetInstance();
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

        /// <summary>
        /// Gets the Script Editor view model.
        /// </summary>
        public ScriptEditorViewModel ScriptEditorViewModel
        {
            get
            {
                return ScriptEditorViewModel.GetInstance();
            }
        }

        /// <summary>
        /// Gets the Text Editor view model.
        /// </summary>
        public TextEditorViewModel TextEditorViewModel
        {
            get
            {
                return TextEditorViewModel.GetInstance();
            }
        }

        /// <summary>
        /// Gets the Value Editor view model.
        /// </summary>
        public ValueEditorViewModel ValueEditorViewModel
        {
            get
            {
                return ValueEditorViewModel.GetInstance();
            }
        }

        /// <summary>
        /// Gets a Offset Editor view model.
        /// </summary>
        public OffsetEditorViewModel OffsetEditorViewModel
        {
            get
            {
                return OffsetEditorViewModel.GetInstance();
            }
        }

        /// <summary>
        /// Gets a Hotkey Editor view model.
        /// </summary>
        public HotkeyEditorViewModel HotkeyEditorViewModel
        {
            get
            {
                return HotkeyEditorViewModel.GetInstance();
            }
        }
    }
    //// End class
}
//// End namespace