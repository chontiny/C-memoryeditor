namespace Squalr.Source.Scanners.Pointers.Structures
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class LevelPointers
    {
        public LevelPointers()
        {
            this.Pointers = new List<PointerPool>();
        }

        public IList<PointerPool> Pointers { get; set; }

        public PointerPool ModulePointers
        {
            get
            {
                return this.Pointers.Last();
            }
        }

        public PointerPool DestinationPointer
        {
            get
            {
                return this.Pointers.First();
            }
        }

        public IEnumerable<PointerPool> HeapPointerLevels
        {
            get
            {
                // Take all pointer pools, excluding the module pointers and destination pointer
                return this.Pointers.Skip(1).Take(this.Pointers.Count - 2);
            }
        }

        public Int32 Count
        {
            get
            {
                return this.Pointers.Count;
            }
        }

        public void AddLevel(PointerPool level)
        {
            this.Pointers.Add(level);
        }

        /// <summary>
        /// Finds the offsets between all pointers of a previous level, and the pointers of this level.
        /// </summary>
        /// <param name="previousPointerLevel"></param>
        public void FindOffsets(PointerPool previousPointerLevel)
        {

        }
    }
    //// End class
}
//// End namespace