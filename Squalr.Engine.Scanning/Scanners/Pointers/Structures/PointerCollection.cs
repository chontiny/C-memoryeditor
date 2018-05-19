namespace Squalr.Engine.Scanning.Scanners.Pointers.Structures
{
    using Squalr.Engine.DataTypes;
    using Squalr.Engine.Scanning.Scanners.Pointers.SearchKernels;
    using Squalr.Engine.Scanning.Snapshots;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// A class to contain the discovered pointers from a pointer scan.
    /// </summary>
    public class PointerCollection
    {
        private static Random Random = new Random();

        /// <summary>
        /// Initializes a new instance of the <see cref="PointerCollection" /> class.
        /// </summary>
        public PointerCollection(IList<Snapshot> levels, UInt32 radius, DataType dataType)
        {
            this.Levels = levels;
            this.Radius = radius;
            this.DataType = dataType;
        }

        private IList<Snapshot> Levels { get; set; }

        /// <summary>
        /// Gets the minimum number of pointers in the collection. In actuality there may be significantly more.
        /// </summary>
        public UInt64 AtLeastCount
        {
            get
            {
                return this.Levels.FirstOrDefault()?.ElementCount ?? 0;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private UInt32 Radius { get; set; }

        private DataType DataType { get; set; }

        /// <summary>
        /// Gets a random pointer from the pointer collection.
        /// </summary>
        /// <returns>A random discovered pointer, or null if unable to find one.</returns>
        public Pointer GetRandomPointer()
        {
            Snapshot currentSnapshot = this.Levels?.LastOrDefault();

            if (currentSnapshot == null)
            {
                return null;
            }

            Int32 pointerSize = this.DataType == DataType.UInt32 ? 4 : 8;
            SnapshotRegion destinationPointerRegion = currentSnapshot.SnapshotRegions[Random.Next(0, currentSnapshot.SnapshotRegions.Length)];
            SnapshotElementIndexer destinationPointerElement = destinationPointerRegion[Random.Next(0, destinationPointerRegion.ElementCount)];

            UInt64 pointerBase = destinationPointerElement.BaseAddress;
            List<Int32> offsets = new List<Int32>();

            foreach (Snapshot previousLevel in this.Levels.Reverse().Skip(1))
            {
                ISearchKernel searchKernel = SearchKernelFactory.GetSearchKernel(currentSnapshot, this.Radius);
                TrackableTask<Snapshot> filter = PointerFilter.Filter(previousLevel, searchKernel, currentSnapshot, this.Radius);
                Snapshot connectedPointerSnapshot = filter.Result;

                // Shouldnt happen, but safety check
                if (connectedPointerSnapshot.ByteCount <= 0)
                {
                    break;
                }

                // Again randomly take pointer paths
                SnapshotRegion connectedPointerRegion = connectedPointerSnapshot.SnapshotRegions[Random.Next(0, connectedPointerSnapshot.SnapshotRegions.Length)];
                SnapshotElementIndexer connectedPointer = connectedPointerRegion[Random.Next(0, connectedPointerRegion.ElementCount)];

                UInt64 currentPointer = this.DataType == DataType.UInt32 ? (UInt32)connectedPointer.LoadCurrentValue() : (UInt64)connectedPointer.LoadCurrentValue();
                Int32 offset = pointerBase > currentPointer ? unchecked((Int32)(pointerBase - currentPointer)) : -unchecked((Int32)(currentPointer - pointerBase));

                offsets.Add(offset);

                pointerBase = connectedPointer.BaseAddress;
                currentSnapshot = new Snapshot(new SnapshotRegion(connectedPointerRegion.ReadGroup, connectedPointerRegion.ReadGroupOffset + connectedPointer.ElementIndex, pointerSize));
            }

            return new Pointer(pointerBase, this.DataType, offsets.ToArray());
        }
    }
    //// End class
}
//// End namespace
