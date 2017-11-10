namespace Squalr.Source.Snapshots
{
    using System;

    [Flags]
    internal enum SnapshotRetrievalMode
    {
        FromActiveSnapshot,
        FromActiveSnapshotOrPrefilter,
        FromSettings,
        FromUserModeMemory,
        FromHeap,
        FromStack,
    }
}
