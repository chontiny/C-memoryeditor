namespace Squalr.Engine.Scanning.Scanners.Pointers.Structures
{
    using Squalr.Engine.Scanning.Snapshots;

    public class Level
    {
        public Level(Snapshot heapPointers, Snapshot staticPointers)
        {
            this.HeapPointers = heapPointers;
            this.StaticPointers = staticPointers;
        }

        public Snapshot HeapPointers { get; private set; }

        public Snapshot StaticPointers { get; private set; }
    }
    //// End class
}
//// End namespace