namespace SqualrCore.Source.ProjectItems
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    public class DotNetItem : AddressItem
    {
        public DotNetItem(String name, Type type, String identifier)
        {

        }

        protected override IntPtr ResolveAddress()
        {
            throw new NotImplementedException();
        }
    }
    //// End class
}
//// End namespace