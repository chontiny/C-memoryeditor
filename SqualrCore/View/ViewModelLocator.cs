namespace SqualrCore.View
{
    using SqualrCore.Properties;
    using SqualrCore.Source.ActionScheduler;
    using SqualrCore.Source.ChangeLog;
    using SqualrCore.Source.Docking;
    using SqualrCore.Source.Editors.HotkeyEditor;
    using SqualrCore.Source.Editors.ScriptEditor;
    using SqualrCore.Source.Editors.TextEditor;
    using SqualrCore.Source.Editors.ValueEditor;
    using SqualrCore.Source.Main;
    using SqualrCore.Source.Output;
    using SqualrCore.Source.ProcessSelector;
    using SqualrCore.Source.PropertyViewer;

    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public abstract class ViewModelLocator
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
        internal MainViewModel MainViewModel
        {
            get
            {
                return MainViewModel.GetInstance();
            }
        }

        /// <summary>
        /// Gets the Docking view model.
        /// </summary>
        public DockingViewModel DockingViewModel
        {
            get
            {
                return DockingViewModel.GetInstance();
            }
        }

        /// <summary>
        /// Gets the Action Scheduler view model.
        /// </summary>
        public ActionSchedulerViewModel ActionSchedulerViewModel
        {
            get
            {
                return ActionSchedulerViewModel.GetInstance();
            }
        }

        /// <summary>
        /// Gets the Process Selector view model.
        /// </summary>
        public ProcessSelectorViewModel ProcessSelectorViewModel
        {
            get
            {
                return ProcessSelectorViewModel.GetInstance();
            }
        }

        /// <summary>
        /// Gets the Property Viewer view model.
        /// </summary>
        public PropertyViewerViewModel PropertyViewerViewModel
        {
            get
            {
                return PropertyViewerViewModel.GetInstance();
            }
        }

        /// <summary>
        /// Gets the Settings view model.
        /// </summary>
        internal SettingsViewModel SettingsViewModel
        {
            get
            {
                return SettingsViewModel.GetInstance();
            }
        }

        /// <summary>
        /// Gets a Output view model.
        /// </summary>
        public OutputViewModel OutputViewModel
        {
            get
            {
                return OutputViewModel.GetInstance();
            }
        }

        /// <summary>
        /// Gets a Change Log view model. Note: Not a singleton, will create a new object.
        /// </summary>
        public ChangeLogViewModel ChangeLogViewModel
        {
            get
            {
                return ChangeLogViewModel.GetInstance();
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