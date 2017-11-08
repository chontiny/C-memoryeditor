namespace Squalr.Source.Scanners.Pointers.Structures
{
    using SqualrCore.Source.ProjectItems;
    using SqualrCore.Source.Utils.Extensions;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// A class to contain the discovered pointers from a pointer rescan.
    /// </summary>
    internal class ValidatedPointers : IDiscoveredPointers
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValidatedPointers" /> class.
        /// </summary>
        public ValidatedPointers()
        {
            this.Pointers = new List<PointerItem>();
        }

        /// <summary>
        /// Gets or sets the discovered pointer roots.
        /// </summary>
        public IList<PointerItem> Pointers { get; set; }

        /// <summary>
        /// Gets the number of discovered pointers.
        /// </summary>
        public override UInt64 Count
        {
            get
            {
                return this.Pointers.Count.ToUInt64();
            }
        }

        /// <summary>
        /// Indexer to allow the retrieval of the element at the specified index.
        /// </summary>
        /// <param name="index">The index of the pointer.</param>
        /// <returns>Returns the pointer at the specified index.</returns>
        public override PointerItem this[UInt64 index]
        {
            get
            {
                if (unchecked((Int32)index) < this.Pointers.Count)
                {
                    return this.Pointers[unchecked((Int32)index)];
                }

                return base[index];
            }
        }

        /// <summary>
        /// Gets the enumerator for the discovered pointers.
        /// </summary>
        /// <returns>The enumerator for the discovered pointers.</returns>
        public override IEnumerator<PointerItem> GetEnumerator()
        {
            return this.Pointers.GetEnumerator();
        }
    }
    //// End class
}
//// End namespace