namespace Squalr.Source.Prefilters
{
    using Squalr.Engine.Scanning.Snapshots;

    /// <summary>
    /// A prefilter to only include regions within heaps.
    /// </summary>
    internal class HeapPrefilter : ISnapshotPrefilter
    {
        /// <summary>
        /// Creates an instance of the <see cref="HeapPrefilter" /> class.
        /// </summary>
        public HeapPrefilter()
        {
        }

        public Snapshot Apply(Snapshot snapshot)
        {
            // TODO: Not implemented yet

            return snapshot;
        }
    }
    //// End class
}
//// End namespace