namespace Squalr.Source.ProjectExplorer
{
    using ProjectItems;
    using System.Collections.Generic;

    /// <summary>
    /// Interface for a class which listens for changes in the the project explorer.
    /// </summary>
    internal interface IProjectExplorerObserver
    {
        /// <summary>
        /// Recieves an update of the project items in the project explorer upon structure changes.
        /// </summary>
        /// <param name="projectRoot">The project items.</param>
        void UpdateStructure(List<ProjectItem> projectItems);

        /// <summary>
        /// Recieves an update of the project items in the project explorer upon value changes.
        /// </summary>
        /// <param name="projectRoot">The project items.</param>
        void Update(List<ProjectItem> projectItems);
    }
    //// End interface
}
//// End namespace