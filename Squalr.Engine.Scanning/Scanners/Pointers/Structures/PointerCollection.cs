namespace Squalr.Engine.Scanning.Scanners.Pointers.Structures
{
    using Squalr.Engine.DataTypes;
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
                TrackableTask<Snapshot> filter = PointerFilter.Filter(previousLevel, currentSnapshot, this.Radius, this.DataType);
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
                Int32 offset = pointerBase > currentPointer ? unchecked((Int32)(pointerBase - currentPointer)) : unchecked((Int32)(currentPointer - pointerBase));

                offsets.Add(offset);

                pointerBase = connectedPointer.BaseAddress;
                currentSnapshot = new Snapshot(new SnapshotRegion(connectedPointerRegion.ReadGroup, connectedPointerRegion.ReadGroupOffset + connectedPointer.ElementIndex, pointerSize));
            }

            /*
            Int32 pointerSize = this.DataType == DataType.UInt32 ? 4 : 8;
            // Randomly select a pointer from the 1st level (static)
            SnapshotRegion randomRootRegion = destinationSnapshot.SnapshotRegions[Random.Next(0, destinationSnapshot.SnapshotRegions.Length)];
            SnapshotElementIndexer randomPointerRoot = randomRootRegion[Random.Next(0, randomRootRegion.ElementCount)];
            Snapshot currentSnapshot = new Snapshot(new SnapshotRegion(randomRootRegion.ReadGroup, randomRootRegion.ReadGroupOffset + randomPointerRoot.ElementIndex, pointerSize));
            UInt64 currentPointer = this.DataType == DataType.UInt32 ? (UInt32)randomPointerRoot.LoadCurrentValue() : (UInt64)randomPointerRoot.LoadCurrentValue();

            // Prepare results
            UInt64 pointerBase = randomPointerRoot.BaseAddress;
            List<Int32> offsets = new List<Int32>();

            foreach (Snapshot nextLevelSnapshot in this.Levels.Skip(1))
            {
                TrackableTask<Snapshot> filter = PointerFilter.Filter(currentSnapshot, nextLevelSnapshot, this.Radius, this.DataType);
                Snapshot connectedPointerSnapshot = filter.Result;

                // Shouldnt happen, but safety check
                if (connectedPointerSnapshot.ByteCount <= 0)
                {
                    break;
                }

                // Again randomly take pointer paths
                SnapshotRegion connectedPointerRegion = connectedPointerSnapshot.SnapshotRegions[Random.Next(0, connectedPointerSnapshot.SnapshotRegions.Length)];
                SnapshotElementIndexer connectedPointer = connectedPointerRegion[Random.Next(0, connectedPointerRegion.ElementCount)];

                Int32 offset = currentPointer > connectedPointer.BaseAddress ? unchecked((Int32)(currentPointer - connectedPointer.BaseAddress)) : unchecked((Int32)(connectedPointer.BaseAddress - currentPointer));

                offsets.Add(offset);
                currentPointer = this.DataType == DataType.UInt32 ? (UInt32)connectedPointer.LoadCurrentValue() : (UInt64)connectedPointer.LoadCurrentValue();

                currentSnapshot = nextLevelSnapshot;
            }
            */

            return new Pointer(pointerBase, this.DataType, offsets);
        }
    }
    //// End class
}
//// End namespace
