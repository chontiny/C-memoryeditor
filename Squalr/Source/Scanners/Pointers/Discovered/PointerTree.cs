namespace Squalr.Source.Scanners.Pointers.Discovered
{
    using System;
    using System.Collections.Generic;

    internal class PointerTree
    {
        public PointerTree(UInt64 baseAddress)
        {
            this.BaseAddress = baseAddress;
            this.Offsets = new List<PointerBranch>();
        }

        public UInt64 BaseAddress { get; set; }

        public IList<PointerBranch> Offsets { get; set; }
    }
}
