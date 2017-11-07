namespace Squalr.Source.Scanners.Pointers.Structures
{
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

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
    }
    //// End class
}
//// End namespace