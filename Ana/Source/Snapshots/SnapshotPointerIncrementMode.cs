namespace Ana.Source.Snapshots
{
    /// <summary>
    /// Enums determining which pointers need to be updated every iteration.
    /// </summary>
    public enum SnapshotPointerIncrementMode
    {
        /// <summary>
        /// Increment all pointers.
        /// </summary>
        AllPointers,

        /// <summary>
        /// Only increment current and previous value pointers.
        /// </summary>
        ValuesOnly,

        /// <summary>
        /// Only increment label pointers.
        /// </summary>
        LabelsOnly,

        /// <summary>
        /// Only increment current value pointer.
        /// </summary>
        CurrentValueOnly,

        /// <summary>
        /// Increment all pointers except the previous value pointer.
        /// </summary>
        NoPreviousValue,
    }
    //// End enum
}
//// End namespace