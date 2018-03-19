using System;

namespace Squalr.Engine.Scripting
{
    /// <summary>
    /// Instance of a single script.
    /// </summary>
    public class Script
    {
        public String Text { get; set; }

        public String Name { get; set; }

        public Boolean IsActivated { get; set; }
    }
    //// End class
}
//// End namespace