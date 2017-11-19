namespace Squalr.Source.SnapshotsV2
{
    using SqualrCore.Source.Engine;
    using SqualrCore.Source.Engine.VirtualMemory;
    using SqualrCore.Source.Utils.Extensions;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    internal class MemoryRegion : NormalizedRegion, IEnumerable<SnapshotRegion>
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

        /// <summary>
        /// Reads all memory for this memory region.
        /// </summary>
        /// <param name="keepValues">Whether or not to keep the values returned as current values.</param>
        /// <returns>The bytes read from memory.</returns>
        public void ReadAllMemory(Boolean keepValues)
        {
            this.ReadGroups?.ForEach(group => group.ReadAllMemory(keepValues));
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

        public IEnumerator<SnapshotRegion> GetEnumerator()
        {
            return this.ReadGroups?.SelectMany(snapshotRegion => snapshotRegion).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.ReadGroups?.SelectMany(snapshotRegion => snapshotRegion).GetEnumerator();
        }
    }
    //// End class
}
//// End namespace