namespace Squalr.Source.SolutionExplorer
{
    using Squalr.Engine.Utils.DataStructures;
    using System;
    using System.ComponentModel;
    using System.IO;

    /// <summary>
    /// Enum to hold the Types of different file objects
    /// </summary>
    public enum ObjectType
    {
        MyComputer = 0,
        DiskDrive = 1,
        Directory = 2,
        File = 3
    }

    /// <summary>
    /// Class for containing the information about a Directory/File
    /// </summary>
    public class DirInfo : INotifyPropertyChanged
    {
        private FullyObservableCollection<DirInfo> subDirectories;

        private Boolean isExpanded;

        private Boolean isSelected;

        private String name;

        private String path;

        private String root;

        private String size;

        private String ext;

        private Int32 dirType;

        public DirInfo()
        {
            this.SubDirectories = new FullyObservableCollection<DirInfo>();
            this.SubDirectories.Add(new DirInfo("TempDir"));
        }

        public DirInfo(string directoryName)
        {
            this.Name = directoryName;
        }

        public DirInfo(DirectoryInfo dir) : this()
        {
            this.Name = dir.Name;
            this.Root = dir.Root.Name;
            this.Path = dir.FullName;
            this.DirType = (Int32)ObjectType.Directory;
        }

        public DirInfo(FileInfo fileobj)
        {
            this.Name = fileobj.Name;
            this.Path = fileobj.FullName;
            this.DirType = (Int32)ObjectType.File;
            this.Size = (fileobj.Length / 1024).ToString() + " KB";
            this.Ext = fileobj.Extension + " File";
        }

        public DirInfo(DriveInfo driveobj) : this()
        {
            if (driveobj.Name.EndsWith(@"\"))
            {
                this.Name = driveobj.Name.Substring(0, driveobj.Name.Length - 1);
            }
            else
            {
                this.Name = driveobj.Name;
            }

            this.Path = driveobj.Name;
            this.DirType = (Int32)ObjectType.DiskDrive;
        }

        public String Name
        {
            get
            {
                return this.name;
            }

            set
            {
                this.name = value;
                this.RaisePropertyChanged(nameof(this.Name));
            }
        }

        public String Path
        {
            get
            {
                return this.path;
            }

            set
            {
                this.path = value;
                this.RaisePropertyChanged(nameof(this.Path));
            }
        }

        public String Root
        {
            get
            {
                return this.root;
            }

            set
            {
                this.root = value;
                this.RaisePropertyChanged(nameof(this.Root));
            }
        }

        public String Size
        {
            get
            {
                return this.size;
            }

            set
            {
                this.size = value;
                this.RaisePropertyChanged(nameof(this.Size));
            }
        }

        public String Ext
        {
            get
            {
                return this.ext;
            }

            set
            {
                this.ext = value;
                this.RaisePropertyChanged(nameof(this.Ext));
            }
        }

        public Int32 DirType
        {
            get
            {
                return this.dirType;
            }

            set
            {
                this.dirType = value;
                this.RaisePropertyChanged(nameof(this.DirType));
            }
        }

        public FullyObservableCollection<DirInfo> SubDirectories
        {
            get
            {
                return this.subDirectories;
            }

            set
            {
                this.subDirectories = value;
                this.RaisePropertyChanged(nameof(this.SubDirectories));
            }
        }

        public Boolean IsExpanded
        {
            get
            {
                return this.isExpanded;
            }

            set
            {
                this.isExpanded = value;
                this.RaisePropertyChanged(nameof(this.IsExpanded));
            }
        }

        public Boolean IsSelected
        {
            get
            {
                return this.isSelected;
            }

            set
            {
                this.isSelected = value;
                this.RaisePropertyChanged(nameof(this.IsSelected));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Indicates that a given property in this project item has changed.
        /// </summary>
        /// <param name="propertyName">The name of the changed property.</param>
        protected void RaisePropertyChanged(String propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    //// End class
}
//// End namespace