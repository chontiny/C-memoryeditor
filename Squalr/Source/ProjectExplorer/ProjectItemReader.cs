namespace Squalr.Source.ProjectExplorer
{
    using Squalr.Engine.Logging;
    using Squalr.Engine.Projects;
    using Squalr.Source.ProjectExplorer.ProjectItems;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// Class to get file system information.
    /// </summary>
    internal class ProjectItemReader
    {
        /// <summary>
        /// Gets the list of files in the directory Name passed.
        /// </summary>
        /// <param name="directory">The Directory to get the files from.</param>
        /// <returns>Returns the List of File info for this directory.
        /// Return null if an exception is raised.</returns>
        public static IEnumerable<ProjectItemView> GetChildFiles(String directory)
        {
            IList<ProjectItemView> projectItems = new List<ProjectItemView>();

            try
            {
                IEnumerable<FileInfo> files = Directory.GetFiles(directory).Select(subdirectories => new FileInfo(subdirectories));

                foreach (FileInfo file in files)
                {
                    if (file.Extension == ".address")
                    {
                        projectItems.Add(new PointerItemView(PointerItem.FromFile(file.FullName)));
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "Error fetching files", ex);
            }

            try
            {
                IEnumerable<DirectoryInfo> subdirectories = Directory.GetDirectories(directory).Select(subdirectory => new DirectoryInfo(subdirectory));

                foreach (DirectoryInfo subdirectory in subdirectories)
                {
                    projectItems.Add(new DirectoryItemView(new DirectoryItem(subdirectory.FullName)));
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "Error fetching directories", ex);
            }

            return projectItems;
        }


        /// <summary>
        /// Gets the list of directories.
        /// </summary>
        /// <param name="directory">The Directory to get the files from.</param>
        /// <returns>Returns the List of directories info for this directory.
        /// Return null if an exception is raised.</returns>
        public static IList<DirectoryInfo> GetChildDirectories(String directory)
        {

            return new List<DirectoryInfo>();
        }
    }
}