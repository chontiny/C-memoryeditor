namespace Squalr.Source.ProjectExplorer.ProjectItems
{
    using Squalr.Engine.Projects;
    using System;

    /// <summary>
    /// Decorates the base project item class with annotations for use in the view.
    /// </summary>
    internal class DirectoryItemDecorator : DirectoryItem
    {
        public DirectoryItemDecorator(String path) : base(path)
        {
        }
    }
    //// End class
}
//// End namespace