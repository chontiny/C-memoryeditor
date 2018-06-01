namespace Squalr.Source.ProjectExplorer.ProjectItems
{
    using Squalr.Engine.Projects;
    using System;
    using System.ComponentModel;

    internal class ProjectItemView : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private ProjectItem projectItem;

        private Boolean isSelected;

        /// <summary>
        /// Indicates that a given property in this project item has changed.
        /// </summary>
        /// <param name="propertyName">The name of the changed property.</param>
        protected void RaisePropertyChanged(String propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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

        public virtual Boolean IsExpanded
        {
            get
            {
                return false;
            }

            set
            {
            }
        }

        public ProjectItem ProjectItem
        {
            get
            {
                return this.projectItem;
            }

            set
            {
                this.projectItem = value;
                this.RaisePropertyChanged(nameof(this.ProjectItem));
            }
        }
    }
    //// End class
}
//// End namespace