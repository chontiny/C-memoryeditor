namespace Squalr.Engine.Scanning.Scanners.Pointers.SearchKernels
{
    using Squalr.Engine.Scanning.Snapshots;
    using System;
    using System.Numerics;

    internal interface ISearchKernel
    {
        Func<Vector<Byte>> GetSearchKernel(SnapshotElementVectorComparer snapshotElementVectorComparer);
    }
    //// End interface
}
//// End namespace