namespace Squalr.Source.Scanners.Pointers.Structures
{
    using SqualrCore.Source.Output;
    using SqualrCore.Source.ProjectItems;
    using System;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// An abstract class for accessing the discovered pointers from a pointer scan or rescan
    /// </summary>
    internal abstract class IDiscoveredPointers : IEnumerable<PointerItem>
    {
        /// <summary>
        /// Gets the number of discovered pointers.
        /// </summary>
        public abstract UInt64 Count { get; }

        /// <summary>
        /// Indexer to allow the retrieval of the element at the specified index.
        /// </summary>
        /// <param name="index">The index of the pointer.</param>
        /// <returns>Returns the pointer at the specified index.</returns>
        public virtual PointerItem this[UInt64 index]
        {
            get
            {
                OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Fatal, "Invalid discovered pointer index");
                return null;
            }
        }

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