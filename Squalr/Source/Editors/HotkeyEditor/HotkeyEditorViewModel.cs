namespace Squalr.Source.Editors.HotkeyEditor
{
    using Docking;
    using Engine.Input.HotKeys;
    using Main;
    using Mvvm.Command;
    using System;
    using System.Threading;
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
        /// Singleton instance of the <see cref="HotkeyEditorViewModel" /> class.
        /// </summary>
        private static Lazy<HotkeyEditorViewModel> hotkeyEditorViewModelInstance = new Lazy<HotkeyEditorViewModel>(
                () => { return new HotkeyEditorViewModel(); },
                LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// Prevents a default instance of the <see cref="HotkeyEditorViewModel" /> class.
        /// </summary>
        private HotkeyEditorViewModel() : base("Hotkey Editor")
        {
            this.ContentId = HotkeyEditorViewModel.ToolContentId;
            this.ClearHotkeysCommand = new RelayCommand(() => this.ClearActiveHotkey(), () => true);
            this.AccessLock = new Object();

            Task.Run(() => MainViewModel.GetInstance().RegisterTool(this));
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

        /// <summary>
        /// Gets a singleton instance of the <see cref="HotkeyEditorViewModel" /> class.
        /// </summary>
        /// <returns>A singleton instance of the class.</returns>
        public static HotkeyEditorViewModel GetInstance()
        {
            return HotkeyEditorViewModel.hotkeyEditorViewModelInstance.Value;
        }

        public void SetActiveHotkey(Hotkey hotkey)
        {
            lock (this.AccessLock)
            {
                if (hotkey == null || hotkey is KeyboardHotkey)
                {
                    KeyboardHotkey keyboardHotkey = hotkey as KeyboardHotkey;

                    if (this.keyboardHotKeyBuilder == null)
                    {
                        this.keyboardHotKeyBuilder = new KeyboardHotkeyBuilder(this.OnHotkeysUpdated, keyboardHotkey);
                    }
                    else
                    {
                        this.keyboardHotKeyBuilder.SetHotkey(keyboardHotkey);
                    }

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