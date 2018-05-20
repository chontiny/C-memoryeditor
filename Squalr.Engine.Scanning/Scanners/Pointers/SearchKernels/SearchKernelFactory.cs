namespace Squalr.Engine.Scanning.Scanners.Pointers.SearchKernels
{
    using Squalr.Engine.Scanning.Snapshots;
    using System;

    internal class SearchKernelFactory
    {
        public static IVectorSearchKernel GetSearchKernel(Snapshot boundsSnapshot, UInt32 radius)
        {
            if (boundsSnapshot.SnapshotRegions.Length < 64)
            {
                // Linear is fast for small region sizes
                return new LinearSearchKernel(boundsSnapshot, radius);
            }
            else
            {
                return new SpanSearchKernel(boundsSnapshot, radius);
            }
        }
    }
    //// End class
}
//// End namespace