namespace Ana.Source.Project
{
    using ProjectItems;

    /// <summary>
    /// Interface for a class which listens for changes in the the project explorer.
    /// </summary>
    internal interface IProjectExplorerObserver
    {
        /// <summary>
        /// Recieves an update of the project items in the project explorer.
        /// </summary>
        /// <param name="projectRoot">The project root.</param>
        void Update(ProjectRoot projectRoot);
    }
    //// End interface
}
//// End namespace