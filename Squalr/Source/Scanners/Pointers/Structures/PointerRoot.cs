namespace Squalr.Source.Scanners.Pointers.Structures
{
    using Squalr.Source.Utils.Extensions;
    using System;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// A structure that contains many pointer paths in the form of a tree structure.
    /// </summary>
    internal class PointerRoot : IEnumerable<PointerBranch>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PointerRoot" /> class.
        /// </summary>
        /// <param name="baseAddress">The base address of this pointer tree.</param>
        public PointerRoot(UInt64 baseAddress)
        {
            this.BaseAddress = baseAddress;
            this.Branches = new List<PointerBranch>();
        }

        /// <summary>
        /// Gets or sets the base address of this pointer tree.
        /// </summary>
        public UInt64 BaseAddress { get; set; }

        /// <summary>
        /// Gets or sets the list of pointer branches stemming from this root.
        /// </summary>
        private IList<PointerBranch> Branches { get; set; }

        /// <summary>
        /// Takes the given offsets and creates branches from them.
        /// </summary>
        /// <param name="offsets">The pointer offsets.</param>
        public void AddOffsets(IEnumerable<Int32> offsets)
        {
            if (offsets.IsNullOrEmpty())
            {
                return;
            }

            foreach (Int32 offset in offsets)
            {
                this.Branches.Add(new PointerBranch(offset));
            }
        }

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