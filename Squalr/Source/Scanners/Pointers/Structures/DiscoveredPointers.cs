namespace Squalr.Source.Scanners.Pointers.Structures
{
    using Squalr.Source.ProjectItems;
    using System;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// An abstract class for accessing the discovered pointers from a pointer scan or rescan.
    /// </summary>
    internal abstract class DiscoveredPointers : IEnumerable<PointerItem>
    {
        /// <summary>
        /// Gets the number of discovered pointers.
        /// </summary>
        public abstract UInt64 Count { get; }

        /// <summary>
        /// Gets the pointers between the specified indicies.
        /// </summary>
        /// <param name="startIndex">The start index.</param>
        /// <param name="endIndex">The end index.</param>
        /// <returns>The pointers between the specified indicies.</returns>
        public abstract IEnumerable<PointerItem> GetPointers(UInt64 startIndex, UInt64 endIndex);

        /// <summary>
        /// Gets the enumerator for the discovered pointers.
        /// </summary>
        /// <returns>The enumerator for the discovered pointers.</returns>
        public abstract IEnumerator<PointerItem> GetEnumerator();

        /// <summary>
        /// Gets the enumerator for the discovered pointers.
        /// </summary>
        /// <returns>The enumerator for the discovered pointers.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
    //// End class
}
//// End namespace