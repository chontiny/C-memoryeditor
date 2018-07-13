using System;

namespace Squalr.Engine.Scripting
{
    public class ModAttribute : Attribute
    {
        public String Name { get; set; }

        public InputType InputType { get; set; }
    }
    //// End class
}
//// End namespace