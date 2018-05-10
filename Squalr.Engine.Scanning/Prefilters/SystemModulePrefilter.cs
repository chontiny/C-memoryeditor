namespace Squalr.Source.Prefilters
{
    using Squalr.Engine.Scanning.Snapshots;

    /// <summary>
    /// A prefilter to remove system modules from a snapshot.
    /// </summary>
    internal class SystemModulePrefilter : ISnapshotPrefilter
    {
        /// <summary>
        /// Creates an instance of the <see cref="ISnapshotPrefilter" /> class.
        /// </summary>
        public SystemModulePrefilter()
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