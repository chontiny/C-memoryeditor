namespace Squalr.Source.Scanners.Pointers.Structures
{
    using SqualrCore.Source.ProjectItems;
    using SqualrCore.Source.Utils.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// A class to contain the discovered pointers from a pointer scan.
    /// </summary>
    internal class ScannedPointers : IDiscoveredPointers
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScannedPointers" /> class.
        /// </summary>
        public ScannedPointers()
        {
            this.PointerRoots = new List<PointerRoot>();
        }

        /// <summary>
        /// Gets or sets the discovered pointer roots.
        /// </summary>
        public IList<PointerRoot> PointerRoots { get; set; }

        /// <summary>
        /// Gets the number of discovered pointers.
        /// </summary>
        public override UInt64 Count
        {
            get
            {
                return this.CountLeaves();
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
                foreach (PointerItem pointer in this)
                {
                    if (index > 0)
                    {
                        index--;
                    }
                    else
                    {
                        return pointer;
                    }
                }

                return base[index];
            }
        }

        /// <summary>
        /// Sorts the pointer roots by base address.
        /// </summary>
        public void Sort()
        {
            this.PointerRoots = new List<PointerRoot>(this.PointerRoots.OrderBy(root => root.BaseAddress));
        }

        /// <summary>
        /// Adds a new pointer root to the discovered pointers.
        /// </summary>
        /// <param name="pointerRoot">The pointer root.</param>
        public void AddPointerRoot(PointerRoot pointerRoot)
        {
            this.PointerRoots.Add(pointerRoot);
        }

        /// <summary>
        /// Gets the enumerator for the discovered pointers.
        /// </summary>
        /// <returns>The enumerator for the discovered pointers.</returns>
        public override IEnumerator<PointerItem> GetEnumerator()
        {
            foreach (PointerRoot root in this.PointerRoots)
            {
                foreach (PointerBranch branch in root)
                {
                    Stack<Int32> offsets = new Stack<Int32>();

                    foreach (PointerItem pointerItem in this.EnumerateBranches(root.BaseAddress, offsets, branch))
                    {
                        yield return pointerItem;
                    }
                }
            }
        }

        /// <summary>
        /// Enumerates the pointers of the specified pointer branch.
        /// </summary>
        /// <param name="baseAddress">The current base address.</param>
        /// <param name="offsets">The offsets leading to this branch.</param>
        /// <param name="branch">The current branch.</param>
        /// <returns>The full pointer path to the branch.</returns>
        private IEnumerable<PointerItem> EnumerateBranches(UInt64 baseAddress, Stack<Int32> offsets, PointerBranch branch)
        {
            offsets.Push(branch.Offset);

            if (branch.Branches.Count <= 0)
            {
                PointerItem pointerItem = new PointerItem(baseAddress.ToIntPtr(), typeof(Int32), "New Pointer", null, offsets.ToArray().Reverse());

                yield return pointerItem;
            }
            else
            {
                foreach (PointerBranch childBranch in branch)
                {
                    foreach (PointerItem pointerItem in this.EnumerateBranches(baseAddress, offsets, childBranch))
                    {
                        yield return pointerItem;
                    }
                }
            }

            offsets.Pop();
        }

        /// <summary>
        /// Counts the number of leaves for this pointer tree.
        /// </summary>
        /// <returns>The number of leaves on the pointer tree.</returns>
        private UInt64 CountLeaves()
        {
            UInt64 count = 0;

            foreach (PointerRoot root in this.PointerRoots)
            {
                foreach (PointerBranch branch in root)
                {
                    count = this.CountLeaves(count, branch);
                }
            }

            return count;
        }

        /// <summary>
        /// Helper function to count the number of leaves on a pointer tree branch.
        /// </summary>
        /// <param name="count">The count so far.</param>
        /// <param name="branch">The current pointer tree branch.</param>
        /// <returns>The count updated with the number of leaves from this branch.</returns>
        private UInt64 CountLeaves(UInt64 count, PointerBranch branch)
        {
            if (branch.Branches.Count <= 0)
            {
                return count + 1;
            }
            else
            {
                foreach (PointerBranch childBranch in branch)
                {
                    count = this.CountLeaves(count, childBranch);
                }

                return count;
            }
        }
    }
    //// End class
}
//// End namespace