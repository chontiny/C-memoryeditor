namespace Squalr.Engine.Projects
{
    using System;
    using System.IO;

    public class Project
    {
        public Project(String projectFilePath)
        {
            this.WatchFileSystem(projectFilePath);
        }

        private FileSystemWatcher FileSystemWatcher { get; set; }

        public void Rename(String newProjectName)
        {

        }

        private void WatchFileSystem(String projectRootPath)
        {
            this.FileSystemWatcher = new FileSystemWatcher
            {
                Path = projectRootPath,
                Filter = "*.*",
                NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
                EnableRaisingEvents = true,
            };

            this.FileSystemWatcher.Changed += new FileSystemEventHandler(this.OnFileSystemChanged);
        }

        private void OnFileSystemChanged(Object source, FileSystemEventArgs args)
        {
            switch (args.ChangeType)
            {
                case WatcherChangeTypes.Changed:
                    break;
                case WatcherChangeTypes.Created:
                    break;
                case WatcherChangeTypes.Deleted:
                    break;
                case WatcherChangeTypes.Renamed:
                    break;
                default:
                    break;
            }
        }
    }
    //// End class
}
//// End namespace