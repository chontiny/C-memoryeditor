namespace Squalr.Source.Snapshots
{
    using SqualrCore.Source.Engine;
    using SqualrCore.Source.Engine.VirtualMemory;
    using SqualrCore.Source.Utils.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class MemoryRegion : NormalizedRegion
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryRegion" /> class.
        /// </summary>
        public MemoryRegion() : this(IntPtr.Zero, 0UL)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryRegion" /> class.
        /// </summary>
        /// <param name="normalizedRegion">The region on which to base this memory region.</param>
        public MemoryRegion(NormalizedRegion normalizedRegion) :
            this(normalizedRegion == null ? IntPtr.Zero : normalizedRegion.BaseAddress, normalizedRegion == null ? 0 : normalizedRegion.RegionSize)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryRegion" /> class.
        /// </summary>
        /// <param name="baseAddress">The base address of this memory region.</param>
        /// <param name="regionSize">The size of this memory region.</param>
        public MemoryRegion(IntPtr baseAddress, UInt64 regionSize) : base(baseAddress, regionSize)
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
        /// Reads all memory for this memory region.
        /// </summary>
        /// <returns>The bytes read from memory.</returns>
        public void ReadAllMemory()
        {
            this.ReadGroups?.ForEach(group => group.ReadAllMemory());
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