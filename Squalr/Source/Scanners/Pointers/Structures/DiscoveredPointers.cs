namespace Squalr.Source.Scanners.Pointers.Structures
{
    using SqualrCore.Source.Utils.Extensions;
    using System;
    using System.Collections.Generic;

    internal class DiscoveredPointers
    {
        public DiscoveredPointers()
        {
            this.PointerRoots = new List<PointerRoot>();
        }

        public IList<PointerRoot> PointerRoots { get; set; }

        public UInt64 Count
        {
            get
            {
                // TODO: Traverse to count, similar to indexing into a snapshot
                return this.PointerRoots.Count.ToUInt64();
            }
        }

        public void AddPointerRoot(PointerRoot pointerRoot)
        {
            this.PointerRoots.Add(pointerRoot);
        }
    }
}
