namespace Squalr.Source.Scanners.Pointers.Structures
{
    using Squalr.Engine.Memory;
    using Squalr.Source.Snapshots;
    using Squalr.Source.Utils.Extensions;
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// A class that contains a collection of pointers.
    /// </summary>
    internal class PointerPool : IEnumerable<KeyValuePair<UInt64, UInt64>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PointerPool" /> class.
        /// </summary>
        public PointerPool()
        {
            this.Pointers = new ConcurrentDictionary<UInt64, UInt64>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PointerPool" /> class.
        /// </summary>
        /// <param name="pointers">The initial pointers</param>
        public PointerPool(params KeyValuePair<UInt64, UInt64>[] pointers)
        {
            this.Pointers = new ConcurrentDictionary<UInt64, UInt64>(pointers);
        }

        /// <summary>
        /// Gets or sets the collection of pointers in this pool.
        /// </summary>
        public ConcurrentDictionary<UInt64, UInt64> Pointers { get; set; }

        /// <summary>
        /// Gets the number of pointers in this pool.
        /// </summary>
        public Int32 Count
        {
            get
            {
                return this.Pointers.Count;
            }
        }

        /// <summary>
        /// Gets the collection of pointer addresses in this pool.
        /// </summary>
        public ICollection<UInt64> PointerAddresses
        {
            get
            {
                return this.Pointers.Keys;
            }
        }

        /// <summary>
        /// Gets the collection of pointer destinations in this pool.
        /// </summary>
        public ICollection<UInt64> PointerDestinations
        {
            get
            {
                return this.Pointers.Values;
            }
        }

        /// <summary>
        /// Indexer to allow the retrieval of the pointer destination given the pointer source.
        /// </summary>
        /// <param name="key">The pointer.</param>
        /// <returns>Returns the pointer destination at the specified index.</returns>
        public UInt64 this[UInt64 key]
        {
            get
            {
                return this.Pointers[key];
            }

            set
            {
                this.Pointers[key] = value;
            }
        }

        public Snapshot ToSnapshot(Int32 pointerRadius)
        {
            IList<NormalizedRegion> levelRegions = new List<NormalizedRegion>();
            IList<ReadGroup> levelReadGroups = new List<ReadGroup>();

            foreach (KeyValuePair<UInt64, UInt64> pointer in this)
            {
                levelRegions.Add(new NormalizedRegion(pointer.Key.ToIntPtr().Subtract(pointerRadius, wrapAround: false), pointerRadius));
            }

            foreach (NormalizedRegion region in NormalizedRegion.MergeAndSortRegions(levelRegions))
            {
                levelReadGroups.Add(new ReadGroup(region.BaseAddress, region.RegionSize));
            }

            Snapshot pointerPoolSnapshot = new Snapshot(null, levelReadGroups);

            return pointerPoolSnapshot;
        }

        /// <summary>
        /// Returns an interator to the pointers in this collection.
        /// </summary>
        /// <returns>An interator to the pointers in this collection.</returns>
        public IEnumerator GetEnumerator()
        {
            return ((IEnumerable)this.Pointers).GetEnumerator();
        }

        /// <summary>
        /// Returns an interator to the pointers in this collection.
        /// </summary>
        /// <returns>An interator to the pointers in this collection.</returns>
        IEnumerator<KeyValuePair<UInt64, UInt64>> IEnumerable<KeyValuePair<UInt64, UInt64>>.GetEnumerator()
        {
            return this.Pointers.GetEnumerator();
        }

        /// <summary>
        /// Finds the offsets between a pointer and the pointers of this level.
        /// </summary>
        /// <param name="pointerDestination">The pointer destination.</param>
        /// <param name="pointerRadius">How far to search in each direction from the given pointer.</param>
        /// <returns>The list of valid offsets from the destination pointer that point to a pointer in this pool.</returns>
        public IEnumerable<Int32> FindOffsets(UInt64 pointerDestination, Int32 pointerRadius)
        {
            return this.PointerAddresses
                .Select(x => x)
                .Where(x => (x > pointerDestination - unchecked((UInt32)pointerRadius)) && (x < pointerDestination + unchecked((UInt32)pointerRadius)))
                .Select(x => x > pointerDestination ? (x - pointerDestination).ToInt32() : -(pointerDestination - x).ToInt32());
        }
    }
    //// End class
}
//// End namespace