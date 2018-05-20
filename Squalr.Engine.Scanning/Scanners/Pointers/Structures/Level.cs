namespace Squalr.Engine.Scanning.Scanners.Pointers.Structures
{
    using Squalr.Engine.Scanning.Snapshots;

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

        internal Snapshot HeapPointers { get; set; }

        internal Snapshot StaticPointers { get; set; }
    }
    //// End class
}
//// End namespace