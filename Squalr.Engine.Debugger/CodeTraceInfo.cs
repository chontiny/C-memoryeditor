namespace Squalr.Engine.Debuggers
{
    using Squalr.Engine.Architecture;
    using System;
    using System.Collections.Generic;

    public class CodeTraceInfo
    {
        public CodeTraceInfo()
        {
            this.IntRegisters = new Dictionary<String, UInt64>();
        }

        public Instruction Instruction { get; set; }

        public Dictionary<String, UInt64> IntRegisters { get; set; }
    }
    //// End interface
}
//// End namespace
