namespace Squalr.Source.Snapshots
{
    using SqualrCore.Source.Engine;
    using SqualrCore.Source.Engine.VirtualMemory;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class ReadGroupManager : NormalizedRegion
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReadGroupManager" /> class.
        /// </summary>
        /// <param name="baseAddress">The base address of this memory region.</param>
        /// <param name="regionSize">The size of this memory region.</param>
        public ReadGroupManager(IntPtr baseAddress, UInt64 regionSize) : base(baseAddress, regionSize)
        {
            // Create one initial read group
            this.ReadGroups = new List<ReadGroup>() { new ReadGroup(this.BaseAddress, this.RegionSize) };

            this.RebuildReadGroups();
        }

        public IList<ReadGroup> ReadGroups;

        public IEnumerable<SnapshotRegion> SnapshotRegions
        {
            get
            {
                return this.ReadGroups?.SelectMany(readGroup => readGroup.SnapshotRegions);
            }
        }

        /// <summary>
        /// Rebuilds all read groups, enforcing alignment and SIMD aligned sizes.
        /// </summary>
        private void RebuildReadGroups()
        {
            // Get the size of vectors on the device
            Int32 vectorSize = EngineCore.GetInstance().Architecture.GetVectorSize();

            // TODO:
            // - Ensure vectorSize aligned start/end addres
            // - Ensure optimal readgroup placement
        }
    }
    //// End class
}
//// End namespace