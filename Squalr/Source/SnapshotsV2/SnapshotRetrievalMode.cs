namespace Squalr.Source.SnapshotsV2
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
    //// End enum
}
//// End namespace