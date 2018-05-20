namespace Squalr.Engine.Scanning.Scanners.Pointers.Structures
{
    using Squalr.Engine.Scanning.Snapshots;

    internal class Level
    {
        public Level()
        {
        }

        public Level(Snapshot heapPointers)
        {
            this.HeapPointers = heapPointers;
        }

        public Level(Snapshot heapPointers, Snapshot staticPointers)
        {
            this.HeapPointers = heapPointers;
            this.StaticPointers = staticPointers;
        }

        public Snapshot HeapPointers { get; set; }

        public Snapshot StaticPointers { get; set; }
    }
    //// End class
}
//// End namespace