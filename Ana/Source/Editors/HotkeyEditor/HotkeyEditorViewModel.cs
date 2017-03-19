namespace Ana.Source.Editors.HotkeyEditor
{
    using Docking;
    using Engine.Input.HotKeys;
    using Main;
    using Mvvm.Command;
    using System;
    using System.Threading.Tasks;
    using System.Windows.Input;

    /// <summary>
    /// View model for the Script Editor.
    /// </summary>
    internal class HotkeyEditorViewModel : ToolViewModel
    {
        /// <summary>
        /// The content id for the docking library associated with this view model.
        /// </summary>
        public const String ToolContentId = nameof(HotkeyEditorViewModel);

        /// <summary>
        /// The keyboard hotkey being constructed.
        /// </summary>
        private KeyboardHotkeyBuilder keyboardHotKeyBuilder;

        /// <summary>
        /// Initializes a new instance of the <see cref="HotkeyEditorViewModel" /> class.
        /// </summary>
        public HotkeyEditorViewModel() : base("Hotkey Editor")
        {
            this.ContentId = HotkeyEditorViewModel.ToolContentId;
            this.ClearHotkeysCommand = new RelayCommand(() => this.ClearActiveHotkey(), () => true);
            this.keyboardHotKeyBuilder = new KeyboardHotkeyBuilder(this.OnHotkeysUpdated);
            this.AccessLock = new Object();

            Task.Run(() => MainViewModel.GetInstance().Subscribe(this));
        }

        /// <summary>
        /// Gets a command to clear the hotkeys collection.
        /// </summary>
        public ICommand ClearHotkeysCommand { get; private set; }

        /// <summary>
        /// Gets or sets the active hotkey being edited.
        /// </summary>
        public HotkeyBuilder ActiveHotkey
        {
            get
            {
                return this.keyboardHotKeyBuilder;
            }
        }

        /// <summary>
        /// Gets or sets the lock for the hotkey collection access.
        /// </summary>
        private Object AccessLock { get; set; }

        public void SetActiveHotkey(Hotkey hotkey)
        {
            lock (this.AccessLock)
            {
                if (hotkey is KeyboardHotkey)
                {
                    KeyboardHotkey keyboardHotkey = hotkey as KeyboardHotkey;

                    this.keyboardHotKeyBuilder = new KeyboardHotkeyBuilder(this.OnHotkeysUpdated, keyboardHotkey);
                    this.RaisePropertyChanged(nameof(this.ActiveHotkey));
                }
            }
        }

        /// <summary>
        /// Clears the active hotkey value.
        /// </summary>
        private void ClearActiveHotkey()
        {
            lock (this.AccessLock)
            {
                this.keyboardHotKeyBuilder.ClearHotkeys();
            }
        }

        /// <summary>
        /// Event triggered when the hotkeys are updated for this keyboard hotkey.
        /// </summary>
        private void OnHotkeysUpdated()
        {
            this.RaisePropertyChanged(nameof(this.ActiveHotkey));
        }
    }
    //// End class
}
//// End namespace