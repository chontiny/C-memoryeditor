namespace Squalr.Source.SnapshotsV2
{
    using SqualrCore.Source.Engine;
    using SqualrCore.Source.Engine.VirtualMemory;
    using SqualrCore.Source.Utils.Extensions;
    using System;
    using System.Collections.Generic;

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

        /// <summary>
        /// Gets the number of regions contained in this snapshot.
        /// </summary>
        /// <returns>The number of regions contained in this snapshot.</returns>
        public Int32 RegionCount
        {
            get
            {
                return this.ReadGroups?.Count ?? 0;
            }
        }

        /// <summary>
        /// Gets the total number of bytes contained in this snapshot.
        /// </summary>
        public UInt64 ByteCount
        {
            get
            {
                return this.ReadGroups?.Sum(x => x.ByteCount) ?? 0UL;
            }
        }

        /// <summary>
        /// Gets the number of individual elements contained in this snapshot.
        /// </summary>
        /// <returns>The number of individual elements contained in this snapshot.</returns>
        public UInt64 ElementCount
        {
            get
            {
                return this.ReadGroups?.Sum(region => region.ElementCount) ?? 0UL;
            }
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
        }
    }
    //// End class
}
//// End namespace