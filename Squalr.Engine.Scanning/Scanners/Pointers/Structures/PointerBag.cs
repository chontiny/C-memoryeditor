namespace Squalr.Engine.Scanning.Scanners.Pointers.Structures
{
    using Squalr.Engine.DataTypes;
    using Squalr.Engine.Scanning.Snapshots;
    using Squalr.Engine.Utils.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// A class to contain the discovered pointers from a pointer scan.
    /// </summary>
    public class PointerBag
    {
        private static Random Random = new Random();

        /// <summary>
        /// Initializes a new instance of the <see cref="PointerBag" /> class.
        /// </summary>
        internal PointerBag(IList<Level> levels, UInt32 radius, DataType dataType)
        {
            this.Levels = levels;
            this.Radius = radius;
            this.DataType = dataType;
        }

        private IList<Level> Levels { get; set; }

        /// <summary>
        /// Gets the minimum number of pointers in the collection. In actuality there may be significantly more.
        /// </summary>
        public UInt64 AtLeastCount
        {
            get
            {
                return 0;
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
            Snapshot currentSnapshot = this.Levels?.FirstOrDefault()?.StaticPointers;

            if (currentSnapshot == null)
            {
                return null;
            }

            ExtractedPointer pointer = this.ExtractRandomPointer(currentSnapshot);

            UInt64 pointerSize = (UInt64)(this.DataType == DataType.UInt32 ? 4 : 8);
            UInt64 pointerBase = pointer.BaseAddress;
            List<Int32> offsets = new List<Int32>();

            foreach (Level level in this.Levels.Skip(1))
            {
                IEnumerable<Int32> shuffledOffsets = Enumerable.Range(-(Int32)this.Radius, (Int32)(this.Radius * 2) + 1).Shuffle();

                // Brute force all possible offsets in a random order to find the next path (this guarantees uniform path probabilities)
                foreach (Int32 nextRandomOffset in shuffledOffsets)
                {
                    UInt64 newDestination = nextRandomOffset < 0 ? pointer.Destination.Subtract(-nextRandomOffset, wrapAround: false) : pointer.Destination.Add(nextRandomOffset, wrapAround: false);
                    SnapshotRegion snapshotRegion = level.HeapPointers.SnapshotRegions.Select(x => x).Where(y => newDestination >= y.BaseAddress && newDestination <= y.EndAddress).FirstOrDefault();

                    if (snapshotRegion != null)
                    {
                        // We may have sampled an offset that results in a mis-aligned index, so just randomly take an element from this snapshot rather than using the random offset
                        SnapshotElementIndexer randomElement = snapshotRegion[Random.Next(0, snapshotRegion.ElementCount)];
                        Int32 alignedOffset = pointer.Destination >= randomElement.BaseAddress ? -((Int32)(pointer.Destination - randomElement.BaseAddress)) : ((Int32)(randomElement.BaseAddress - pointer.Destination));

                        pointer = this.ExtractPointerFromElement(randomElement);
                        offsets.Add(alignedOffset);
                        break;
                    }
                }
            }

            return new Pointer(pointerBase, this.DataType, offsets.ToArray());
        }

        private ExtractedPointer ExtractRandomPointer(Snapshot snapshot)
        {
            SnapshotRegion extractedRegion = snapshot.SnapshotRegions[Random.Next(0, snapshot.SnapshotRegions.Length)];
            SnapshotElementIndexer extractedElement = extractedRegion[Random.Next(0, extractedRegion.ElementCount)];

            return this.ExtractPointerFromElement(extractedElement);
        }

        private ExtractedPointer ExtractPointerFromElement(SnapshotElementIndexer element)
        {
            return new ExtractedPointer(element.BaseAddress, element.HasCurrentValue() ? (this.DataType == DataType.UInt32 ? (UInt32)element.LoadCurrentValue() : (UInt64)element.LoadCurrentValue()) : 0);
        }

        private class ExtractedPointer
        {
            public ExtractedPointer(UInt64 address, UInt64 destination)
            {
                this.BaseAddress = address;
                this.Destination = destination;
            }

            public UInt64 BaseAddress { get; private set; }

            public UInt64 Destination { get; private set; }
        }
    }
    //// End class
}
//// End namespace
