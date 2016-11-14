namespace Ana.Source.Utils.HotkeyEditor
{
    using Docking;
    using Engine;
    using Engine.Input.Controller;
    using Engine.Input.HotKeys;
    using Engine.Input.Keyboard;
    using Engine.Input.Mouse;
    using Main;
    using Mvvm.Command;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;
    using System.Windows.Input;

    /// <summary>
    /// View model for the Script Editor
    /// </summary>
    internal class HotkeyEditorViewModel : ToolViewModel, IKeyboardObserver, IControllerObserver, IMouseObserver
    {
        /// <summary>
        /// The content id for the docking library associated with this view model
        /// </summary>
        public const String ToolContentId = nameof(HotkeyEditorViewModel);

        private List<IHotkey> hotkeys;

        private IHotkey activeHotkey;

        private KeyboardHotkey keyboardHotkey;

        /// <summary>
        /// Initializes a new instance of the <see cref="HotkeyEditorViewModel" /> class.
        /// </summary>
        public HotkeyEditorViewModel() : base("Hotkey Editor")
        {
            this.ContentId = HotkeyEditorViewModel.ToolContentId;
            this.AddHotkeyCommand = new RelayCommand(() => this.AddHotkey(), () => true);
            this.RemoveHotkeyCommand = new RelayCommand(() => this.RemoveSelectedHotkey(), () => true);
            this.ClearHotkeysCommand = new RelayCommand(() => this.ClearActiveHotkey(), () => true);
            this.keyboardHotkey = new KeyboardHotkey();
            this.AccessLock = new Object();
            this.InitializeListeners();

            Task.Run(() => MainViewModel.GetInstance().Subscribe(this));
        }

        public ICommand AddHotkeyCommand { get; private set; }

        public ICommand RemoveHotkeyCommand { get; private set; }

        public ICommand ClearHotkeysCommand { get; private set; }

        public ICommand UpdateActiveValueCommand { get; private set; }

        public IHotkey ActiveHotkey
        {
            get
            {
                return this.activeHotkey;
            }

            set
            {
                this.activeHotkey = value;
                this.RaisePropertyChanged(nameof(this.ActiveHotkey));
            }
        }

        public ObservableCollection<IHotkey> Hotkeys
        {
            get
            {
                lock (this.AccessLock)
                {
                    if (this.hotkeys == null)
                    {
                        this.hotkeys = new List<IHotkey>();
                    }

                    return new ObservableCollection<IHotkey>(this.hotkeys);
                }
            }

            set
            {
                lock (this.AccessLock)
                {
                    this.hotkeys = value == null ? new List<IHotkey>() : new List<IHotkey>(value);
                    this.RaisePropertyChanged(nameof(this.Hotkeys));
                }
            }
        }

        public Int32 SelectedHotkeyIndex { get; set; }

        private Object AccessLock { get; set; }

        /// <summary>
        /// Event received when a key is pressed
        /// </summary>
        /// <param name="key">The key that was pressed</param>
        public void OnKeyPress(SharpDX.DirectInput.Key key)
        {
            this.keyboardHotkey.ActivationKeys.Add(key);
            this.ActiveHotkey = this.keyboardHotkey;
        }

        /// <summary>
        /// Event received when a key is released
        /// </summary>
        /// <param name="key">The key that was released</param>
        public void OnKeyRelease(SharpDX.DirectInput.Key key)
        {
        }

        /// <summary>
        /// Event received when a key is down
        /// </summary>
        /// <param name="key">The key that is down</param>
        public void OnKeyDown(SharpDX.DirectInput.Key key)
        {
        }

        /// <summary>
        /// Event received when a set of keys are down
        /// </summary>
        /// <param name="pressedKeys">The down keys</param>
        public void OnUpdateAllDownKeys(HashSet<SharpDX.DirectInput.Key> pressedKeys)
        {
        }

        private void InitializeListeners()
        {
            EngineCore.GetInstance()?.Input?.GetKeyboardCapture().Subscribe(this);
            EngineCore.GetInstance()?.Input?.GetControllerCapture().Subscribe(this);
            EngineCore.GetInstance()?.Input?.GetMouseCapture().Subscribe(this);
        }

        private void AddHotkey()
        {
            lock (this.AccessLock)
            {
                if (this.ActiveHotkey != null)
                {
                    this.hotkeys.Add(this.ActiveHotkey);
                    this.ClearActiveHotkey();
                }
            }

            this.RaisePropertyChanged(nameof(this.Hotkeys));
        }

        private void RemoveSelectedHotkey()
        {
            Int32 removalIndex = this.SelectedHotkeyIndex;

            lock (this.AccessLock)
            {
                if (removalIndex < 0)
                {
                    removalIndex = 0;
                }

                if (removalIndex < this.hotkeys.Count)
                {
                    this.hotkeys.RemoveAt(removalIndex);
                }
            }

            this.RaisePropertyChanged(nameof(this.Hotkeys));
        }

        private void ClearActiveHotkey()
        {
            lock (this.AccessLock)
            {
                this.keyboardHotkey = new KeyboardHotkey();
                this.ActiveHotkey = this.keyboardHotkey;
            }

            this.RaisePropertyChanged(nameof(this.Hotkeys));
        }

        private void UpdateActiveValue(IHotkey hotkey)
        {
            this.ActiveHotkey = hotkey;
        }
    }
    //// End class
}
//// End namespace