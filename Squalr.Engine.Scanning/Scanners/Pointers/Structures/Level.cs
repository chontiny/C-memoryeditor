namespace Squalr.Engine.Scanning.Scanners.Pointers.Structures
{
    using Squalr.Engine.Scanning.Snapshots;
    using System;

    public class Level
    {
        internal Level()
        {
        }

        internal Level(Snapshot heapPointers)
        {
            this.HeapPointers = heapPointers;
        }

        internal Level(Snapshot heapPointers, Snapshot staticPointers)
        {
            this.HeapPointers = heapPointers;
            this.StaticPointers = staticPointers;
        }

        public UInt64 BaseCount
        {
            get
            {
                return this.StaticPointers?.ElementCount ?? 0;
            }
        }

        public UInt64 PointerCount
        {
            get
            {
                return this.HeapPointers?.ElementCount ?? 0;
            }
        }

        internal Snapshot HeapPointers { get; set; }

        internal Snapshot StaticPointers { get; set; }
    }
    //// End class
}
//// End namespace