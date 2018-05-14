using System;
using System.Collections.Generic;

namespace Squalr.Engine.Scanning.Scanners.Pointers.Structures
{
    public class Pointer
    {
        public Pointer(
            UInt64 baseAddress,
            Type dataType,
            IEnumerable<Int32> offsets = null)
        {
            this.BaseAddress = baseAddress;
            this.Offsets = offsets;
        }

        public UInt64 BaseAddress { get; set; }

        public IEnumerable<Int32> Offsets { get; private set; }

        public void Update()
        {
        }
    }
    //// End class
}
//// End namespace