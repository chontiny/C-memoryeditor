namespace Squalr.Engine.Projects
{
    using Squalr.Engine.Logging;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// Defines a directory in the project.
    /// </summary>
    public class DirectoryItem : ProjectItem
    {
        private IEnumerable<ProjectItem> childItems;

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectoryItem" /> class.
        /// </summary>
        public DirectoryItem(String filePath) : base(filePath)
        {
            this.FilePath = filePath;

            this.ChildItems = this.GetChildProjectItems();
            this.WatchForUpdates();
        }

        public static DirectoryItem FromDirectory(String directoryPath)
        {
            try
            {
                if (!Directory.Exists(directoryPath))
                {
                    throw new Exception("Directory does not exist: " + directoryPath);
                }

                return new DirectoryItem(directoryPath);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "Error loading file", ex);
                throw ex;
            }
        }

        public IEnumerable<ProjectItem> ChildItems
        {
            get
            {
                return this.childItems;
            }

            private set
            {
                this.childItems = value;
                this.RaisePropertyChanged(nameof(this.ChildItems));
            }
        }

        private FileSystemWatcher FileSystemWatcher { get; set; }

        public override void Update()
        {
        }

        public void AddChild(ProjectItem projectItem)
        {

        }

        /// <summary>
        /// Gets the list of files in the directory Name passed.
        /// </summary>
        /// <returns>Returns the List of File info for this directory.
        /// Return null if an exception is raised.</returns>
        public IEnumerable<ProjectItem> GetChildProjectItems()
        {
            IList<ProjectItem> projectItems = new List<ProjectItem>();

            try
            {
                foreach (FileInfo file in Directory.GetFiles(this.FilePath).Select(directoryFile => new FileInfo(directoryFile)))
                {
                    try
                    {
                        ProjectItem projectItem = ProjectItem.FromFile(file.FullName);

                        if (projectItem != null)
                        {
                            projectItems.Add(projectItem);
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
                IEnumerable<DirectoryInfo> subdirectories = Directory.GetDirectories(this.FilePath).Select(subdirectory => new DirectoryInfo(subdirectory));

                foreach (DirectoryInfo subdirectory in subdirectories)
                {
                    try
                    {
                        projectItems.Add(DirectoryItem.FromDirectory(subdirectory.FullName));
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

        private void WatchForUpdates()
        {
            this.FileSystemWatcher = new FileSystemWatcher(this.FilePath, "*.*");
            this.FileSystemWatcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            this.FileSystemWatcher.Changed += new FileSystemEventHandler(OnFilesOrDirectoriesChanged);
            this.FileSystemWatcher.EnableRaisingEvents = true;
        }

        private void OnFilesOrDirectoriesChanged(Object source, FileSystemEventArgs args)
        {
            switch (args.ChangeType)
            {
                case WatcherChangeTypes.Created:
                    break;
                case WatcherChangeTypes.Deleted:
                    break;
                case WatcherChangeTypes.Changed:
                    break;
                case WatcherChangeTypes.Renamed:
                    this.RaisePropertyChanged(nameof(this.ChildItems));
                    break;
            }
        }
    }
    //// End class
}
//// End namespace