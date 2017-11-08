namespace Squalr.Source.Scanners.Pointers.Discovered
{
    using SqualrCore.Source.Utils.Extensions;
    using System;
    using System.Collections;
    using System.Collections.Generic;

    internal class PointerRoot : IEnumerable<PointerBranch>
    {
        public PointerRoot(UInt64 baseAddress)
        {
            this.BaseAddress = baseAddress;
            this.Branches = new List<PointerBranch>();
        }

        public UInt64 BaseAddress { get; set; }

        private IList<PointerBranch> Branches { get; set; }

        public void AddOffsets(IEnumerable<Int32> offsets)
        {
            if (offsets.IsNullOrEmpty())
            {
                return;
            }

            foreach (Int32 offset in offsets)
            {
                this.Branches.Add(new PointerBranch(offset));
            }
        }

        public IEnumerator<PointerBranch> IterateLeaves()
        {
            return this.Branches.GetEnumerator();
        }

        public IEnumerator<PointerBranch> GetEnumerator()
        {
            return this.Branches.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.Branches.GetEnumerator();
        }
    }
}
