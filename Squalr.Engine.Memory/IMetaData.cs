namespace Squalr.Engine.Memory
{
    using Squalr.Engine.OS;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// An interface for accessing statically available metadata.
    /// </summary>
    internal interface IMetaData : IProcessObserver
    {
        IList<NormalizedRegion> GetDataSegments(UInt64 moduleBase, String modulePath);
    }
    //// End interface
}
//// End namespace