namespace Squalr.Engine.Scanning.Scanners.Pointers.Structures
{
    using Squalr.Engine.DataTypes;
    using Squalr.Engine.Scanning.Snapshots;
    using Squalr.Engine.Utils.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;

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
            Snapshot rootSnapshot = this.Levels?.FirstOrDefault();

            if (rootSnapshot == null)
            {
                return null;
            }

            SnapshotElementIndexer randomPointerRoot = rootSnapshot[PointerCollection.RandomUInt64(0, rootSnapshot.ElementCount)];

            UInt64 pointerBase = this.DataType == DataType.UInt32 ? (UInt32)randomPointerRoot.LoadCurrentValue() : (UInt64)randomPointerRoot.LoadCurrentValue();
            List<Int32> offsets = new List<Int32>();

            UInt64 currentPointer = pointerBase;
            Snapshot currentSnapshot = rootSnapshot;

            foreach (Snapshot nextSnapshot in this.Levels.Skip(1))
            {
                UInt64 lowerBoundAddress = unchecked((UInt32)currentPointer.Subtract(this.Radius, wrapAround: false));
                UInt64 upperBoundAddress = unchecked((UInt32)currentPointer.Add(this.Radius, wrapAround: false));

                // Shuffle to randomize the next pointer path (note this is non-uniformly random and not ideal because regions can have multiple pointers)
                foreach (SnapshotRegion region in nextSnapshot.SnapshotRegions.Shuffle())
                {
                    IList<SnapshotRegion> childRegions = PointerCollection.FindChildRegions(lowerBoundAddress, upperBoundAddress, region);

                    if (childRegions.IsNullOrEmpty())
                    {
                        continue;
                    }

                    // Again randomly take pointer paths
                    SnapshotRegion childRegion = childRegions.Shuffle().First();
                    SnapshotElementIndexer randomPointerTarget = childRegion[Random.Next(0, childRegion.ElementCount)];

                    Int32 offset = currentPointer > randomPointerTarget.BaseAddress ? unchecked((Int32)(currentPointer - randomPointerTarget.BaseAddress)) : unchecked((Int32)(randomPointerTarget.BaseAddress - currentPointer));

                    offsets.Add(offset);
                    currentPointer = this.DataType == DataType.UInt32 ? (UInt32)randomPointerTarget.LoadCurrentValue() : (UInt64)randomPointerTarget.LoadCurrentValue();

                    break;
                }
            }

            return new Pointer(pointerBase, this.DataType, offsets);
        }

        private static IList<SnapshotRegion> FindChildRegions(UInt64 lowerBoundAddress, UInt64 upperBoundAddress, SnapshotRegion region)
        {
            SnapshotElementVectorComparer vectorComparer = new SnapshotElementVectorComparer(region: region);

            Vector<UInt32> lowerBound = new Vector<UInt32>(unchecked((UInt32)lowerBoundAddress));
            Vector<UInt32> upperBound = new Vector<UInt32>(unchecked((UInt32)upperBoundAddress));

            // Determines if the addess of an element falls within pointer range
            vectorComparer.SetCustomCompareAction(new Func<Vector<Byte>>(() =>
            {
                Vector<UInt32> currentAddress = new Vector<UInt32>(unchecked((UInt32)vectorComparer.CurrentAddress));

                return Vector.AsVectorByte(Vector.BitwiseAnd(
                        Vector.GreaterThanOrEqual(currentAddress, lowerBound),
                        Vector.LessThanOrEqual(currentAddress, upperBound)));
            }));

            return vectorComparer.Compare();
        }

        private static UInt64 RandomUInt64(UInt64 min, UInt64 max)
        {
            Byte[] buffer = new Byte[8];

            PointerCollection.Random.NextBytes(buffer);

            return BitConverter.ToUInt64(buffer, 0) % (max - min) + min;
        }
    }
    //// End class
}
//// End namespace