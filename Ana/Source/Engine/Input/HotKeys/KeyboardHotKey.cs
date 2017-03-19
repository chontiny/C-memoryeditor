namespace Ana.Source.Engine.Input.HotKeys
{
    using Keyboard;
    using SharpDX.DirectInput;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using Utils.Extensions;

    /// <summary>
    /// A keyboard hotkey, which is activated by a given set of input.
    /// </summary>
    [DataContract]
    internal class KeyboardHotkey : Hotkey, IObserver<KeyState>
    {
        /// <summary>
        /// The default delay in miliseconds between hotkey activations.
        /// </summary>
        private const Int32 DefaultActivationDelay = 50000;

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

            EngineCore.GetInstance().Input?.GetKeyboardCapture().WeakSubscribe(this);
        }

        /// <summary>
        /// Gets or sets the set of inputs corresponding to this hotkey.
        /// </summary>
        [DataMember]
        public HashSet<Key> ActivationKeys { get; set; }

        /// <summary>
        /// Invoked when this object is deserialized.
        /// </summary>
        /// <param name="streamingContext">Streaming context.</param>
        [OnDeserialized]
        public void OnDeserialized(StreamingContext streamingContext)
        {
            this.LastActivated = DateTime.MinValue;
            this.ActivationDelay = KeyboardHotkey.DefaultActivationDelay;

            EngineCore.GetInstance().Input?.GetKeyboardCapture().WeakSubscribe(this);
        }

        public void OnNext(KeyState value)
        {
            if (this.ActivationKeys.IsNullOrEmpty())
            {
                return;
            }

            foreach (Key key in value.DownKeys)
            {
                if (this.ActivationKeys.Any(x => key == x))
                {
                    this.LastActivated = DateTime.MinValue;
                }
            }

            if (this.IsReady() && this.ActivationKeys.All(x => value.PressedKeys.Contains(x)))
            {
                this.Activate();
            }
        }

        public void OnError(Exception error)
        {
        }

        public void OnCompleted()
        {
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
        /// Gets the string representation of the hotkey inputs.
        /// </summary>
        /// <returns>The string representation of hotkey inputs.</returns>
        public override String ToString()
        {
            String hotKeyString = String.Empty;

            if (this.ActivationKeys.IsNullOrEmpty())
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