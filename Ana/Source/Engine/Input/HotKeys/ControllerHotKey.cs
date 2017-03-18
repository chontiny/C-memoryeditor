namespace Ana.Source.Engine.Input.HotKeys
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// A controller hotkey, which is activated by a given set of input.
    /// </summary>
    [DataContract]
    internal class ControllerHotkey : Hotkey
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ControllerHotkey" /> class.
        /// </summary>
        /// <param name="callBackFunction">The callback function for this hotkey.</param>
        public ControllerHotkey(Action callBackFunction = null) : base(callBackFunction)
        {
        }

        /// <summary>
        /// Determines if the current set of activation hotkeys are empty.
        /// </summary>
        /// <returns>True if there are hotkeys, otherwise false.</returns>
        public override Boolean HasHotkey()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Clones the hotkey.
        /// </summary>
        /// <returns>A clone of the hotkey.</returns>
        public override Hotkey Clone()
        {
            ControllerHotkey hotkey = new ControllerHotkey(this.CallBackFunction);
            return hotkey;
        }

        /// <summary>
        /// Gets the string representation of the hotkey inputs.
        /// </summary>
        /// <returns>The string representatio of hotkey inputs.</returns>
        public override String ToString()
        {
            return base.ToString();
        }

        /// <summary>
        /// Determines if this hotkey is able to be triggered.
        /// </summary>
        /// <returns>True if able to be triggered, otherwise false.</returns>
        protected override Boolean IsReady()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Activates this hotkey, triggering the callback function.
        /// </summary>
        protected override void Activate()
        {
            throw new NotImplementedException();
        }
    }
    //// End class
}
//// End namespace