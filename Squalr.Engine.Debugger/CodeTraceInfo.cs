namespace Squalr.Engine.Debugger
{
    using System;
    using System.Collections.Generic;

    public class CodeTraceInfo
    {
        public CodeTraceInfo()
        {
            this.IntRegisters = new Dictionary<String, UInt64>();
        }

        public UInt64 Address { get; set; }

        public String Instruction { get; set; }

        public Dictionary<String, UInt64> IntRegisters { get; set; }
    }
    //// End interface
}
//// End namespace
