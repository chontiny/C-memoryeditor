namespace Squalr.Source.StreamWeaver.Overlay
{
    using ProjectExplorer.ProjectItems;
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    internal class OverlayItem
    {
        public OverlayItem(ProjectItem projectItem)
        {
            this.StreamCommand = projectItem?.StreamCommand;
            this.StreamIconPath = projectItem?.StreamIconPath;
            this.Category = projectItem?.Category ?? ProjectItem.ProjectItemCategory.Miscellaneous;
        }

        [DataMember]
        public String StreamCommand { get; private set; }

        [DataMember]
        public String StreamIconPath { get; private set; }

        [DataMember]
        public ProjectItem.ProjectItemCategory Category { get; private set; }
    }
    //// End class
}
//// End namespace