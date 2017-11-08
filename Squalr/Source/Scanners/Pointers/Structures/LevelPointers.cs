namespace Squalr.Source.Scanners.Pointers.Structures
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class LevelPointers
    {
        public LevelPointers()
        {
            this.PointerPools = new List<PointerPool>();
        }

        public IList<PointerPool> PointerPools { get; set; }

        public PointerPool ModulePointerPool
        {
            get
            {
                return this.PointerPools.First();
            }
        }

        public PointerPool DestinationPointerPool
        {
            get
            {
                return this.PointerPools.Last();
            }
        }

        /// <summary>
        /// Gets the heap pointer pools. This includes the destination pool (although technically it may not be in the heap).
        /// </summary>
        public IEnumerable<PointerPool> HeapPointerPools
        {
            get
            {
                // Take all pointer pools, excluding the module pointers
                return this.PointerPools.Skip(1).Take(this.PointerPools.Count - 1);
            }
        }

        public Int32 Count
        {
            get
            {
                return this.PointerPools.Count;
            }
        }

        public void AddLevel(PointerPool level)
        {
            this.PointerPools.Insert(0, level);
        }
    }
    //// End class
}
//// End namespace