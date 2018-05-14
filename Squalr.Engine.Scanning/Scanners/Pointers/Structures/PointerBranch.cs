namespace Squalr.Engine.Scanning.Scanners.Pointers.Structures
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// A structure that is the branch from a pointer root, and may contain other branches.
    /// </summary>
    public class PointerBranch : IEnumerable<PointerBranch>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PointerBranch" /> class.
        /// </summary>
        /// <param name="offset">The offset of this branch.</param>
        public PointerBranch(Int32 offset, UInt64 pointer)
        {
            this.Offset = offset;
            this.Branches = new List<PointerBranch>();
        }

        /// <summary>
        /// Gets or sets the offset of this branch.
        /// </summary>
        public Int32 Offset { get; set; }

        /// <summary>
        /// Gets or sets the address to which this branch points
        /// </summary>
        public UInt64 Pointer { get; set; }

        /// <summary>
        /// Gets or sets the list of child branches to this branch.
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