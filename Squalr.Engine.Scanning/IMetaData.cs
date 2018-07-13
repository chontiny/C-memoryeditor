namespace Squalr.Engine.Scanning
{
    using Squalr.Engine.OS;
    using Squalr.Engine.Scanning.Snapshots;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// An interface for accessing statically available metadata.
    /// </summary>
    public interface IMetaData : IProcessObserver
    {
        IList<SnapshotRegion> GetDataSegments(UInt64 moduleBase, String modulePath);
    }
    //// End interface
}
//// End namespace