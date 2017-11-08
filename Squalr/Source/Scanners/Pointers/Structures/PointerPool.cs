namespace Squalr.Source.Scanners.Pointers.Structures
{
    using SqualrCore.Source.Utils.Extensions;
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
        public PointerPool()
        {
            this.Pointers = new ConcurrentDictionary<UInt64, UInt64>();
        }

        public ConcurrentDictionary<UInt64, UInt64> Pointers { get; set; }

        public Int32 Count
        {
            get
            {
                return this.Pointers.Count;
            }
        }

        public UInt64 this[UInt64 key]
        {
            get
            {
                return Pointers[key];
            }
            set
            {
                this.Pointers[key] = value;
            }
        }

        public ICollection<UInt64> PointerAddresses
        {
            get
            {
                return this.Pointers.Keys;
            }
        }

        public ICollection<UInt64> PointerDestinations
        {
            get
            {
                return this.Pointers.Values;
            }
        }

        public IEnumerator GetEnumerator()
        {
            return ((IEnumerable)Pointers).GetEnumerator();
        }

        IEnumerator<KeyValuePair<UInt64, UInt64>> IEnumerable<KeyValuePair<UInt64, UInt64>>.GetEnumerator()
        {
            return Pointers.GetEnumerator();
        }

        /// <summary>
        /// Finds the offsets between a pointer and the pointers of this level.
        /// </summary>
        /// <param name="previousPointerLevel"></param>
        public IEnumerable<Int32> FindOffsets(UInt64 pointer, UInt32 pointerRadius)
        {
            return this
                .PointerAddresses
                .Select(x => x)
                .Where(x => (x > pointer - pointerRadius) && (x < pointer + pointerRadius))
                .Select(x => (x > pointer) ? (x - pointer).ToInt32() : -((pointer - x).ToInt32()));
        }
    }
    //// End class
}
//// End namespace