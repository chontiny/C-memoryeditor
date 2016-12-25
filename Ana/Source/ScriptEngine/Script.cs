namespace Ana.Source.ScriptEngine
{
    using System;
    using System.ComponentModel;
    using System.Runtime.Serialization;

    /// <summary>
    /// Defines a script that can leverage the engine to execute a cheat
    /// </summary>
    [DataContract]
    internal class Script
    {
        [Browsable(false)]
        private String payload;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptEngine.Script" /> class
        /// </summary>
        public Script()
        {
            this.Payload = String.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptEngine.Script" /> class
        /// </summary>
        /// <param name="payload">The raw script text</param>
        public Script(String payload)
        {
            this.Payload = payload;
        }

        [DataMember]
        [Browsable(false)]
        public String Payload
        {
            get
            {
                return this.payload;
            }

            set
            {
                this.payload = value == null ? String.Empty : value;
            }
        }
    }
    //// End class
}
//// End namespace