namespace Squalr.Source.SnapshotsV2
{
    using SqualrCore.Source.Engine;
    using SqualrCore.Source.Engine.VirtualMemory;
    using SqualrCore.Source.Utils.Extensions;
    using System;
    using System.Collections;
    using System.Collections.Generic;

    internal class ReadGroup : NormalizedRegion, IEnumerable<SnapshotRegion>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReadGroup" /> class.
        /// </summary>
        public ReadGroup() : this(IntPtr.Zero, 0UL)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadGroup" /> class.
        /// </summary>
        /// <param name="normalizedRegion">The region on which to base this memory region.</param>
        public ReadGroup(NormalizedRegion normalizedRegion) :
            this(normalizedRegion == null ? IntPtr.Zero : normalizedRegion.BaseAddress, normalizedRegion == null ? 0 : normalizedRegion.RegionSize)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadGroup" /> class.
        /// </summary>
        /// <param name="baseAddress">The base address of this memory region.</param>
        /// <param name="regionSize">The size of this memory region.</param>
        public ReadGroup(IntPtr baseAddress, UInt64 regionSize) : base(baseAddress, regionSize)
        {
            this.SnapshotRegions = new List<SnapshotRegion>() { new SnapshotRegion(this.BaseAddress, this.RegionSize) };
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
        /// <param name="keepValues">Whether or not to keep the values returned as current values.</param>
        /// <returns>The bytes read from memory.</returns>
        public void ReadAllMemory(Boolean keepValues)
        {
            Byte[] newCurrentValues = EngineCore.GetInstance().VirtualMemory.ReadBytes(this.BaseAddress, this.RegionSize.ToInt32(), out _);

            if (keepValues)
            {
                this.SetPreviousValues(this.CurrentValues);
                this.SetCurrentValues(newCurrentValues);
            }
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

        public IEnumerator<SnapshotRegion> GetEnumerator()
        {
            return SnapshotRegions.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return SnapshotRegions.GetEnumerator();
        }
    }
    //// End class
}
//// End namespace