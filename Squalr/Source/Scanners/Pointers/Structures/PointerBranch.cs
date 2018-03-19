namespace Squalr.Source.Scanners.Pointers.Structures
{
    using Squalr.Source.Utils.Extensions;
    using System;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// A structure that is the branch from a pointer root, and may contain other branches.
    /// </summary>
    internal class PointerBranch : IEnumerable<PointerBranch>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PointerBranch" /> class.
        /// </summary>
        /// <param name="offset">The offset of this branch.</param>
        public PointerBranch(Int32 offset)
        {
            this.Offset = offset;
            this.Branches = new List<PointerBranch>();
        }

        /// <summary>
        /// Gets or sets the offset of this branch.
        /// </summary>
        public Int32 Offset { get; set; }

        /// <summary>
        /// Gets or sets the list of child branches to this branch.
        /// </summary>
        public IList<PointerBranch> Branches { get; set; }

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