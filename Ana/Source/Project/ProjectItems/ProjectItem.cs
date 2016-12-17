namespace Ana.Source.Project.ProjectItems
{
    using Engine;
    using Engine.Input.Controller;
    using Engine.Input.HotKeys;
    using Engine.Input.Keyboard;
    using Engine.Input.Mouse;
    using SharpDX.DirectInput;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Linq;
    using System.Runtime.Serialization;
    using Utils.HotkeyEditor;
    using Utils.TypeConverters;

    /// <summary>
    /// A base class for all project items that can be added to the project explorer
    /// </summary>
    [KnownType(typeof(ProjectItem))]
    [KnownType(typeof(FolderItem))]
    [KnownType(typeof(ScriptItem))]
    [KnownType(typeof(AddressItem))]
    [KnownType(typeof(IHotkey))]
    [KnownType(typeof(KeyboardHotkey))]
    [KnownType(typeof(ControllerHotkey))]
    [KnownType(typeof(MouseHotKey))]
    [DataContract]
    internal abstract class ProjectItem : INotifyPropertyChanged, IKeyboardObserver, IControllerObserver, IMouseObserver
    {
        [Browsable(false)]
        private const Int32 HotkeyDelay = 400;

        [Browsable(false)]
        private FolderItem parent;

        [Browsable(false)]
        private String description;

        [Browsable(false)]
        private Boolean isActivated;

        [Browsable(false)]
        private IEnumerable<IHotkey> hotkeys;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectItem" /> class
        /// </summary>
        public ProjectItem() : this(String.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectItem" /> class
        /// </summary>
        /// <param name="description">The description of the project item</param>
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
        /// Gets or sets the parent of this project item
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
        /// Gets or sets the description for this object
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
        /// Gets or sets hot keys that activate this project item
        /// </summary>
        [DataMember]
        [TypeConverter(typeof(HotkeyConverter))]
        [Editor(typeof(HotkeyEditorModel), typeof(UITypeEditor))]
        [Category("Properties"), DisplayName("HotKeys"), Description("Hot key to activate item")]
        public IEnumerable<IHotkey> Hotkeys
        {
            get
            {
                return this.hotkeys;
            }

            set
            {
                this.hotkeys = value;
                this.UpdateHotkeyListeners();
                this.NotifyPropertyChanged(nameof(this.Hotkeys));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not this item is activated
        /// </summary>
        [Browsable(false)]
        public Boolean IsActivated
        {
            get
            {
                return this.isActivated;
            }

            set
            {
                this.isActivated = value;
                this.OnActivationChanged();
                this.NotifyPropertyChanged(nameof(this.IsActivated));
            }
        }

        /// <summary>
        /// Gets or sets the time since this item was last activated
        /// </summary>
        [Browsable(false)]
        private DateTime LastActivated { get; set; }

        /// <summary>
        /// Invoked when this object is deserialized
        /// </summary>
        /// <param name="streamingContext">Streaming context</param>
        [OnDeserialized]
        public void OnDeserialized(StreamingContext streamingContext)
        {
        }

        /// <summary>
        /// Updates the project item
        /// </summary>
        public abstract void Update();

        /// <summary>
        /// Event received when a key is pressed
        /// </summary>
        /// <param name="key">The key that was pressed</param>
        public void OnKeyPress(Key key)
        {
        }

        /// <summary>
        /// Event received when a key is down
        /// </summary>
        /// <param name="key">The key that is down</param>
        public void OnKeyDown(Key key)
        {
        }

        /// <summary>
        /// Event received when a key is released
        /// </summary>
        /// <param name="key">The key that was released</param>
        public void OnKeyRelease(Key key)
        {
            // Reset hotkey delay if any of the hotkey keys are released
            if (this.Hotkeys.Where(x => x.GetType().IsAssignableFrom(typeof(KeyboardHotkey))).Cast<KeyboardHotkey>().Any(x => x.ActivationKeys.Any(y => key == y)))
            {
                this.LastActivated = DateTime.MinValue;
            }
        }

        /// <summary>
        /// Event received when a set of keys are down
        /// </summary>
        /// <param name="pressedKeys">The down keys</param>
        public void OnUpdateAllDownKeys(HashSet<Key> pressedKeys)
        {
            if ((DateTime.Now - this.LastActivated).TotalMilliseconds < ProjectItem.HotkeyDelay)
            {
                return;
            }

            // If any of our keyboard hotkeys include the current set of pressed keys, trigger activation/deactivation
            if (this.Hotkeys.Where(x => x.GetType().IsAssignableFrom(typeof(KeyboardHotkey))).Cast<KeyboardHotkey>().Any(x => x.ActivationKeys.All(y => pressedKeys.Contains(y))))
            {
                this.LastActivated = DateTime.Now;
                this.IsActivated = !this.IsActivated;
            }
        }

        protected void NotifyPropertyChanged(String propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnActivationChanged()
        {
        }

        private void UpdateHotkeyListeners()
        {
            // Determine if any hotkeys we have are keyboard events
            if (this.Hotkeys != null && this.Hotkeys.Any(x => x.GetType().IsAssignableFrom(typeof(KeyboardHotkey))))
            {
                EngineCore.GetInstance().Input.GetKeyboardCapture().Subscribe(this);
            }
            else
            {
                EngineCore.GetInstance().Input.GetKeyboardCapture().Unsubscribe(this);
            }

            // Determine if any hotkeys we have are controller events
            if (this.Hotkeys != null && this.Hotkeys.Any(x => x.GetType().IsAssignableFrom(typeof(ControllerHotkey))))
            {
                EngineCore.GetInstance().Input.GetControllerCapture().Subscribe(this);
            }
            else
            {
                EngineCore.GetInstance().Input.GetControllerCapture().Unsubscribe(this);
            }

            // Determine if any hotkeys we have are mouse events
            if (this.Hotkeys != null && this.Hotkeys.Any(x => x.GetType().IsAssignableFrom(typeof(MouseHotKey))))
            {
                EngineCore.GetInstance().Input.GetMouseCapture().Subscribe(this);
            }
            else
            {
                EngineCore.GetInstance().Input.GetMouseCapture().Unsubscribe(this);
            }
        }
    }
    //// End class
}
//// End namespace