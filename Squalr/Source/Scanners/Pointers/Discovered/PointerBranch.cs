namespace Squalr.Source.Scanners.Pointers.Discovered
{
    using System;
    using System.Collections.Generic;

    internal class PointerBranch
    {
        public PointerBranch(Int32 offset)
        {
            this.Offset = offset;
            this.Branches = new List<PointerBranch>();
        }

        public Int32 Offset { get; set; }

        public IList<PointerBranch> Branches { get; set; }
    }
}
