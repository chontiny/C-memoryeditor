namespace Ana.Source.StreamWeaver
{
    using Project.ProjectItems;
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    internal class OverlayItem
    {
        public OverlayItem(ProjectItem projectItem)
        {
            this.StreamCommand = projectItem?.StreamCommand;
            this.Category = projectItem?.Category ?? ProjectItem.ProjectItemCategory.None;
        }

        [DataMember]
        public String StreamCommand { get; private set; }

        [DataMember]
        public ProjectItem.ProjectItemCategory Category { get; private set; }
    }
    //// End class
}
//// End namespace