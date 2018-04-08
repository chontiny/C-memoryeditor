namespace Squalr.Engine.Debugger
{
    using System;
    using System.Collections.Generic;

    public class CodeTraceInfo
    {
        public UInt64 Address { get; set; }

        public Dictionary<String, UInt64> IntRegisters { get; set; }
    }
    //// End interface
}
//// End namespace
