namespace Squalr.Source.Scanners.Pointers.Structures
{
    using SqualrCore.Source.Output;
    using SqualrCore.Source.ProjectItems;
    using SqualrCore.Source.Utils.Extensions;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    internal class DiscoveredPointers : IEnumerable<PointerItem>
    {
        public DiscoveredPointers()
        {
            this.PointerRoots = new List<PointerRoot>();
        }

        public IList<PointerRoot> PointerRoots { get; set; }

        public UInt64 Count
        {
            get
            {
                return this.CountLeaves();
            }
        }

        public void Sort()
        {
            this.PointerRoots = new List<PointerRoot>(this.PointerRoots.OrderBy(root => root.BaseAddress));
        }

        public void AddPointerRoot(PointerRoot pointerRoot)
        {
            this.PointerRoots.Add(pointerRoot);
        }

        /// <summary>
        /// Indexer to allow the retrieval of the element at the specified index.
        /// </summary>
        /// <param name="index">The index of the pointer.</param>
        /// <returns>Returns the pointer at the specified index.</returns>
        public PointerItem this[UInt64 index]
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

                OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Fatal, "Invalid discovered pointer index");
                return null;
            }
        }

        /// <summary>
        /// Gets the enumerator for the discovered pointers.
        /// </summary>
        /// <returns>The enumerator for the discovered pointers.</returns>
        public IEnumerator<PointerItem> GetEnumerator()
        {
            foreach (PointerRoot root in this.PointerRoots)
            {
                foreach (PointerBranch branch in root)
                {
                    Stack<Int32> offsets = new Stack<Int32>();

                    foreach (PointerItem pointerItem in EnumerateBranches(root.BaseAddress, offsets, branch))
                    {
                        yield return pointerItem;
                    }
                }
            }
        }

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

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
