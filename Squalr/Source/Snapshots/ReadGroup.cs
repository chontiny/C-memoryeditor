namespace Squalr.Source.Snapshots
{
    using SqualrCore.Source.Engine;
    using SqualrCore.Source.Engine.VirtualMemory;
    using SqualrCore.Source.Utils.Extensions;
    using System;
    using System.Collections.Generic;

    internal class ReadGroup : NormalizedRegion
    {
        private const UInt64 ChunkSize = 2048UL;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadGroup" /> class.
        /// </summary>
        /// <param name="baseAddress">The base address of this memory region.</param>
        /// <param name="regionSize">The size of this memory region.</param>
        public ReadGroup(IntPtr baseAddress, UInt64 regionSize) : base(baseAddress, regionSize)
        {
            this.LoadSnapshotRegions(new NormalizedRegion(baseAddress, regionSize));
        }

        /// <summary>
        /// Gets the most recently read values.
        /// </summary>
        public unsafe Byte[] CurrentValues { get; private set; }

        /// <summary>
        /// Gets the previously read values.
        /// </summary>
        public unsafe Byte[] PreviousValues { get; private set; }

        public IList<SnapshotRegion> SnapshotRegions;

        /// <summary>
        /// Reads all memory for this memory region.
        /// </summary>
        /// <returns>The bytes read from memory.</returns>
        public Boolean ReadAllMemory()
        {
            Boolean readSuccess;
            Byte[] newCurrentValues = EngineCore.GetInstance().VirtualMemory.ReadBytes(this.BaseAddress, this.RegionSize.ToInt32(), out readSuccess);

            if (readSuccess)
            {
                this.SetPreviousValues(this.CurrentValues);
                this.SetCurrentValues(newCurrentValues);
            }
            else
            {
                this.SetPreviousValues(null);
                this.SetCurrentValues(null);
            }

            return readSuccess;
        }

        /// <summary>
        /// Sets the current values of this region.
        /// </summary>
        /// <param name="newValues">The raw bytes of the values.</param>
        public void SetCurrentValues(Byte[] newValues)
        {
            this.CurrentValues = newValues;
        }

        /// <summary>
        /// Sets the previous values of this region.
        /// </summary>
        /// <param name="newValues">The raw bytes of the values.</param>
        public void SetPreviousValues(Byte[] newValues)
        {
            this.PreviousValues = newValues;
        }

        private void LoadSnapshotRegions(NormalizedRegion baseRegion)
        {
            this.SnapshotRegions = new List<SnapshotRegion>();

            foreach (NormalizedRegion region in baseRegion.ChunkNormalizedRegion(ReadGroup.ChunkSize))
            {
                this.SnapshotRegions.Add(new SnapshotRegion(this, region.BaseAddress, region.RegionSize));
            }
        }
    }
    //// End class
}
//// End namespace