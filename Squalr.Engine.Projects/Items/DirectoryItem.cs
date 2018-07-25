namespace Squalr.Engine.Projects.Items
{
    using Squalr.Engine.Logging;
    using Squalr.Engine.Utils.DataStructures;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// Defines a directory in the project.
    /// </summary>
    public class DirectoryItem : ProjectItem
    {
        private FullyObservableCollection<ProjectItem> childItems;

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectoryItem" /> class.
        /// </summary>
        public DirectoryItem(String directoryPath) : base(directoryPath)
        {
            // Bypass setters to avoid re-saving
            this.name = (new DirectoryInfo(directoryPath)).Name;

            this.childItems = this.BuildChildren();
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

        public FullyObservableCollection<ProjectItem> ChildItems
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
            IEnumerable<ProjectItem> children = this.ChildItems?.ToArray();

            if (children != null)
            {
                foreach (ProjectItem child in this.ChildItems)
                {
                    child.Update();
                }
            }
        }

        public void AddChild(ProjectItem projectItem)
        {
            try
            {
                projectItem.Parent = this;
                this.ChildItems.Add(projectItem);
                projectItem.Save();
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "Unable to add project item", ex);
            }
        }

        public void RemoveChild(ProjectItem projectItem)
        {
            try
            {
                if (this.ChildItems.Contains(projectItem))
                {
                    projectItem.Parent = null;
                    this.ChildItems.Remove(projectItem);
                    
                    if (projectItem is DirectoryItem)
                    {
                        Directory.Delete(projectItem.FullPath, recursive: true);
                    }
                    else
                    {
                        File.Delete(projectItem.FullPath);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "Unable to delete project item", ex);
                // TODO: Probably do a full refresh at this point due to possible de-synchronization
            }
        }

        /// <summary>
        /// Gets the list of files in the directory Name passed.
        /// </summary>
        /// <returns>Returns the List of File info for this directory.
        /// Return null if an exception is raised.</returns>
        public FullyObservableCollection<ProjectItem> BuildChildren()
        {
            FullyObservableCollection<ProjectItem> projectItems = new FullyObservableCollection<ProjectItem>();

            try
            {
                IEnumerable<DirectoryInfo> subdirectories = Directory.GetDirectories(this.FullPath).Select(subdirectory => new DirectoryInfo(subdirectory));

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

            try
            {
                foreach (FileInfo file in Directory.GetFiles(this.FullPath).Select(directoryFile => new FileInfo(directoryFile)))
                {
                    try
                    {
                        ProjectItem projectItem = ProjectItem.FromFile(file.FullName, this);

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

            return projectItems;
        }

        public override void Save()
        {
            try
            {
                if (!Directory.Exists(this.FullPath))
                {
                    Directory.CreateDirectory(this.FullPath);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, "Error creating directory within project.", ex);
            }
        }

        private void WatchForUpdates()
        {
            this.FileSystemWatcher = new FileSystemWatcher(this.FullPath, "*.*");
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