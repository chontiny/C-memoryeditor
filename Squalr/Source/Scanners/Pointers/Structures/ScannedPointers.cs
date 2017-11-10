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
    internal class ScannedPointers : DiscoveredPointers
    {
        /// <summary>
        /// The number of discovered pointers.
        /// </summary>
        private UInt64 count;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScannedPointers" /> class.
        /// </summary>
        public ScannedPointers()
        {
            this.PointerRoots = new List<PointerRoot>();
            this.count = 0;
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
                return this.count;
            }
        }

        /// <summary>
        /// Gets the pointers between the specified indicies.
        /// </summary>
        /// <param name="startIndex">The start index.</param>
        /// <param name="endIndex">The end index.</param>
        /// <returns>The pointers between the specified indicies.</returns>
        public override IEnumerable<PointerItem> GetPointers(UInt64 startIndex, UInt64 endIndex)
        {
            IEnumerator<PointerItem> enumerator = this.EnumeratePointers(startIndex, endIndex);

            while (enumerator.MoveNext())
            {
                if (enumerator.Current != null)
                {
                    yield return enumerator.Current;
                }
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
        /// Updates the number of total pointers.
        /// </summary>
        public void BuildCount()
        {
            this.count = this.CountLeaves();
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
            return this.EnumeratePointers();
        }

        /// <summary>
        /// Gets the enumerator for the discovered pointers.
        /// </summary>
        /// <param name="startIndex">The index at which to return non-null values.</param>
        /// <returns>The enumerator for the discovered pointers.</returns>
        private IEnumerator<PointerItem> EnumeratePointers(UInt64? startIndex = null, UInt64? endIndex = null)
        {
            SelectionIndicies indicies = new SelectionIndicies(startIndex, endIndex);

            foreach (PointerRoot root in this.PointerRoots)
            {
                foreach (PointerBranch branch in root)
                {
                    Stack<Int32> offsets = new Stack<Int32>();

                    foreach (PointerItem pointerItem in this.EnumerateBranches(root.BaseAddress, offsets, branch, indicies))
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
        /// <param name="startIndex">The index at which to return non-null values.</param>
        /// <param name="endIndex">The index at which to stop enumerating.</param>
        /// <returns>The full pointer path to the branch.</returns>
        private IEnumerable<PointerItem> EnumerateBranches(UInt64 baseAddress, Stack<Int32> offsets, PointerBranch branch, SelectionIndicies selectionIndicies)
        {
            offsets.Push(branch.Offset);

            // End index reached
            if (selectionIndicies.Finished)
            {
                yield break;
            }

            if (branch.Branches.Count <= 0)
            {
                PointerItem pointerItem;

                // Only create pointer items when in the range of the selection indicies. This is an optimization to prevent creating unneeded objects.
                if (selectionIndicies.IterateNext())
                {
                    pointerItem = new PointerItem(baseAddress.ToIntPtr(), typeof(Int32), "New Pointer", null, offsets.ToArray().Reverse());
                }
                else
                {
                    pointerItem = null;
                }

                yield return pointerItem;
            }
            else
            {
                foreach (PointerBranch childBranch in branch)
                {
                    foreach (PointerItem pointerItem in this.EnumerateBranches(baseAddress, offsets, childBranch, selectionIndicies))
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
                    this.CountLeaves(ref count, branch);
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
        private void CountLeaves(ref UInt64 count, PointerBranch branch)
        {
            if (branch.Branches.Count <= 0)
            {
                count = count + 1;
            }
            else
            {
                foreach (PointerBranch childBranch in branch)
                {
                    this.CountLeaves(ref count, childBranch);
                }
            }
        }

        private class SelectionIndicies
        {
            public SelectionIndicies(UInt64? startIndex, UInt64? endIndex)
            {
                // Ensure valid indicies
                if (startIndex != null)
                {
                    if (endIndex == null || endIndex < startIndex)
                    {
                        endIndex = startIndex;
                    }
                }

                this.StartIndex = startIndex;
                this.EndIndex = endIndex;

                this.HasIndicies = this.StartIndex != null;
            }

            public UInt64? StartIndex { get; private set; }

            public UInt64? EndIndex { get; private set; }

            public Boolean HasIndicies { get; set; }

            public Boolean Finished
            {
                get
                {
                    return this.HasIndicies && this.EndIndex <= 0;
                }
            }

            public Boolean IterateNext()
            {
                if (!this.HasIndicies)
                {
                    return true;
                }

                Boolean result = this.StartIndex == 0 && !this.Finished;

                if (this.StartIndex > 0)
                {
                    this.StartIndex--;
                }

                if (this.EndIndex > 0)
                {
                    this.EndIndex--;
                }

                return result;
            }
        }
    }
    //// End class
}
//// End namespace