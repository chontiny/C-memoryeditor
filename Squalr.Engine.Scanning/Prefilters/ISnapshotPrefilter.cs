namespace Squalr.Source.Prefilters
{
    using Squalr.Engine.Scanning.Snapshots;

    /// <summary>
    /// Interface defining methods that a snapshot prefilter must implement.
    /// </summary>
    internal interface ISnapshotPrefilter
    {
        /// <summary>
        /// Applies a prefilter to a given snapshot.
        /// </summary>
        /// <param name="snapshot">The snapshot to prefilter.</param>
        /// <returns>The new prefiltered snapshot.</returns>
        Snapshot Apply(Snapshot snapshot);
    }
    //// End interface
}
//// End namespace