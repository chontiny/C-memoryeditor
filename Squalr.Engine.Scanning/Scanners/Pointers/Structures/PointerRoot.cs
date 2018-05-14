namespace Squalr.Engine.Scanning.Scanners.Pointers.Structures
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// A structure that contains many pointer paths in the form of a tree structure.
    /// </summary>
    public class PointerRoot : IEnumerable<PointerBranch>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PointerRoot" /> class.
        /// </summary>
        /// <param name="baseAddress">The base address of this pointer tree.</param>
        public PointerRoot(UInt64 baseAddress, UInt64 pointer)
        {
            this.BaseAddress = baseAddress;
            this.Pointer = pointer;
            this.Branches = new List<PointerBranch>();
        }

        /// <summary>
        /// Gets or sets the base address of this pointer tree.
        /// </summary>
        public UInt64 BaseAddress { get; private set; }

        /// <summary>
        /// Gets or sets the base address of this pointer tree.
        /// </summary>
        public UInt64 Pointer { get; private set; }

        /// <summary>
        /// Gets or sets the list of pointer branches stemming from this root.
        /// </summary>
        public IEnumerable<PointerBranch> Branches { get; set; }

        /// <summary>
        /// Returns an interator to the branches in this collection.
        /// </summary>
        /// <returns>An interator to the branches in this collection.</returns>
        public IEnumerator<PointerBranch> GetEnumerator()
        {
            return this.Branches.GetEnumerator();
        }

        /// <summary>
        /// Returns an interator to the branches in this collection.
        /// </summary>
        /// <returns>An interator to the branches in this collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.Branches.GetEnumerator();
        }
    }
    //// End class
}
//// End namespace