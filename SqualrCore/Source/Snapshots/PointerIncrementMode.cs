namespace SqualrCore.Source.Snapshots
{
    /// <summary>
    /// Enums determining which pointers need to be updated every iteration.
    /// </summary>
    public enum PointerIncrementMode
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
        CurrentOnly,

        /// <summary>
        /// Increment all pointers except the previous value pointer.
        /// </summary>
        NoPrevious,
    }
    //// End enum
}
//// End namespace