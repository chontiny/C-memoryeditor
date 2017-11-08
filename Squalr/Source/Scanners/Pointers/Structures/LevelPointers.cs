namespace Squalr.Source.Scanners.Pointers.Structures
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Structure for organizing all levels of pointer pools that a discovered pointer path may pass through.
    /// </summary>
    internal class LevelPointers
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LevelPointers" /> class.
        /// </summary>
        public LevelPointers()
        {
            this.PointerPools = new List<PointerPool>();
        }

        /// <summary>
        /// Gets or sets the list of all pointer pools, including the modules and destination.
        /// </summary>
        public IList<PointerPool> PointerPools { get; set; }

        /// <summary>
        /// Gets the pointer pool for all modules.
        /// </summary>
        public PointerPool ModulePointerPool
        {
            get
            {
                return this.PointerPools.First();
            }
        }

        /// <summary>
        /// Gets the pointer pool containing a single destination address.
        /// </summary>
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

        /// <summary>
        /// Gets the total number of levels, including the module and destination levels.
        /// </summary>
        public Int32 Count
        {
            get
            {
                return this.PointerPools.Count;
            }
        }

        /// <summary>
        /// Adds a new pointer pool level. This will be inserted as the first level.
        /// </summary>
        /// <param name="level">The new pointer level.</param>
        public void AddLevel(PointerPool level)
        {
            this.PointerPools.Insert(0, level);
        }
    }
    //// End class
}
//// End namespace