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
                    try
                    {
                        ProjectItem projectItem = ProjectItem.FromFile(file.FullName);

                        switch (projectItem)
                        {
                            case ProjectItem _ when projectItem is PointerItem:
                                projectItems.Add(new PointerItemView(projectItem as PointerItem));
                                break;
                            case ProjectItem _ when projectItem is ScriptItem:
                                projectItems.Add(new ScriptItemView(projectItem as ScriptItem));
                                break;
                            case ProjectItem _ when projectItem is InstructionItem:
                                projectItems.Add(new InstructionItemView(projectItem as InstructionItem));
                                break;
                            case ProjectItem _ when projectItem is DotNetItem:
                                projectItems.Add(new DotNetItemView(projectItem as DotNetItem));
                                break;
                            case ProjectItem _ when projectItem is JavaItem:
                                projectItems.Add(new JavaItemView(projectItem as JavaItem));
                                break;
                            default:
                                Logger.Log(LogLevel.Error, "Unknown project item type");
                                break;
                        }

                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, "Error reading project item", ex);
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
                    try
                    {
                        projectItems.Add(new DirectoryItemView(DirectoryItem.FromDirectory(subdirectory.FullName)));
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, "Error loading directory", ex);
                    }
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