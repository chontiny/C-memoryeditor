namespace Squalr.Engine.Scanning.Scanners.Pointers.Structures
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    internal class BinaryHeap<T> : IEnumerable<T>
    {
        /// <summary>
        /// Removes the need to instantiate with an IComparer if the default will suffice.
        /// </summary>
        public BinaryHeap() : this(Comparer<T>.Default) { }

        /// <summary>
        /// Contructs an instance with the appropriate IComparer
        /// </summary>
        /// <param name="comparer"></param>
        public BinaryHeap(IComparer<T> comparer)
        {
            this.Comparer = comparer;
            this.Items = new List<T>();
        }

        protected IComparer<T> Comparer { get; set; }

        /// <summary>
        /// Holds all the items in the heap.
        /// </summary>
        protected List<T> Items { get; set; }

        public Int32 Count
        {
            get
            {
                return this.Items.Count;
            }
        }

        public virtual void Insert(T newItem)
        {
            Int32 index = Count;

            // Add the new item to the bottom of the heap.
            Items.Add(newItem);

            // Until the new item is greater than its parent item, swap the two
            while (index > 0 && Comparer.Compare(Items[(index - 1) / 2], newItem) > 0)
            {
                Items[index] = Items[(index - 1) / 2];

                index = (index - 1) / 2;
            }

            // The new index in the list is the appropriate location for the new item
            this.Items[index] = newItem;
        }

        public T Last()
        {
            return this.Items[this.Items.Count - 1];
        }

        public T[] ToArray()
        {
            return this.Items.ToArray();
        }

        public void Clear()
        {
            this.Items.Clear();
        }

        public virtual IEnumerator GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            foreach (T element in this.Items)
            {
                yield return element;
            }
        }
    }
    //// End class
}
//// End namespace