namespace Ana.Source.Engine.Input.HotKeys
{
    using Keyboard;
    using SharpDX.DirectInput;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;

    /// <summary>
    /// A keyboard hotkey, which is activated by a given set of input.
    /// </summary>
    [DataContract]
    internal class KeyboardHotkey : Hotkey, IKeyboardObserver
    {
        /// <summary>
        /// The default delay in miliseconds between hotkey activations
        /// </summary>
        private const Int32 DefaultActivationDelay = 150;

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyboardHotkey" /> class.
        /// </summary>
        /// <param name="callBackFunction">The callback function for this hotkey.</param>
        /// <param name="activationKeys">Initial activation keys.</param>
        public KeyboardHotkey(Action callBackFunction = null, params Key[] activationKeys) : base(callBackFunction)
        {
            this.ActivationKeys = new HashSet<Key>(activationKeys);
            this.LastActivated = DateTime.MinValue;
            this.ActivationDelay = KeyboardHotkey.DefaultActivationDelay;

            EngineCore.GetInstance().Input?.GetKeyboardCapture().Subscribe(this);
        }

        /// <summary>
        /// Gets or sets the set of inputs corresponding to this hotkey.
        /// </summary>
        [DataMember]
        public HashSet<Key> ActivationKeys { get; set; }

        /// <summary>
        /// Invoked when this object is deserialized.
        /// </summary>
        /// <param name="streamingContext">Streaming context</param>
        [OnDeserialized]
        public void OnDeserialized(StreamingContext streamingContext)
        {
            this.LastActivated = DateTime.MinValue;
            this.ActivationDelay = KeyboardHotkey.DefaultActivationDelay;

            EngineCore.GetInstance().Input?.GetKeyboardCapture().Subscribe(this);
        }

        /// <summary>
        /// Determines if the current set of activation hotkeys are empty.
        /// </summary>
        /// <returns>True if there are hotkeys, otherwise false.</returns>
        public override Boolean HasHotkey()
        {
            return this.ActivationKeys == null ? false : this.ActivationKeys.Count > 0;
        }

        /// <summary>
        /// Clones the hotkey.
        /// </summary>
        /// <returns>A clone of the hotkey.</returns>
        public override Hotkey Clone()
        {
            KeyboardHotkey hotkey = new KeyboardHotkey(this.CallBackFunction);
            hotkey.ActivationKeys = new HashSet<Key>(this.ActivationKeys);
            return hotkey;
        }

        /// <summary>
        /// Event received when a key is pressed.
        /// </summary>
        /// <param name="key">The key that was pressed.</param>
        public void OnKeyPress(Key key)
        {
        }

        /// <summary>
        /// Event received when a key is released.
        /// </summary>
        /// <param name="key">The key that was released.</param>
        public void OnKeyRelease(Key key)
        {
            if (DateTime.Now - this.LastActivated > TimeSpan.FromMilliseconds(this.ActivationDelay))
            {
                this.LastActivated = DateTime.Now;
            }
        }

        /// <summary>
        /// Event received when a key is down.
        /// </summary>
        /// <param name="key">The key that is down.</param>
        public void OnKeyDown(Key key)
        {
        }

        /// <summary>
        /// Event received when a set of keys are down.
        /// </summary>
        /// <param name="pressedKeys">The down keys.</param>
        public void OnUpdateAllDownKeys(HashSet<Key> pressedKeys)
        {
            if (this.IsReady() && this.ActivationKeys.All(x => pressedKeys.Contains(x)))
            {
                this.Activate();
            }
        }

        /// <summary>
        /// Gets the string representation of the hotkey inputs.
        /// </summary>
        /// <returns>The string representatio of hotkey inputs.</returns>
        public override String ToString()
        {
            String hotKeyString = String.Empty;

            if (this.ActivationKeys == null)
            {
                return hotKeyString;
            }

            foreach (Key key in this.ActivationKeys)
            {
                hotKeyString += key.ToString() + "+";
            }

            return hotKeyString.TrimEnd('+');
        }

        /// <summary>
        /// Activates this hotkey, triggering the callback function.
        /// </summary>
        protected override void Activate()
        {
            this.LastActivated = DateTime.Now;
            base.Activate();
        }

        /// <summary>
        /// Determines if this hotkey is able to be triggered.
        /// </summary>
        /// <returns>True if able to be triggered, otherwise false.</returns>
        protected override Boolean IsReady()
        {
            if (DateTime.Now - this.LastActivated > TimeSpan.FromMilliseconds(this.ActivationDelay))
            {
                return true;
            }

            return false;
        }
    }
    //// End class
}
//// End namespace