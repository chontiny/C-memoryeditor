namespace Ana.Source.Engine.Input.HotKeys
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    /// <summary>
    /// A controller hotkey, which is activated by a given set of input
    /// </summary>
    [DataContract]
    internal class ControllerHotkey : IHotkey
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ControllerHotkey" /> class
        /// </summary>
        public ControllerHotkey()
        {
            this.ActivationKeys = new HashSet<Int32>();
        }

        /// <summary>
        /// Gets or sets the set of inputs corresponding to this hotkey
        /// </summary>
        [DataMember]
        public HashSet<Int32> ActivationKeys { get; set; }

        /// <summary>
        /// Gets the string representation of the hotkey inputs
        /// </summary>
        /// <returns>The string representatio of hotkey inputs</returns>
        public override String ToString()
        {
            return base.ToString();
        }
    }
    //// End class
}
//// End namespace