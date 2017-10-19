namespace Squalr.Source.ProjectExplorer.ProjectItems
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    internal class JavaItem : AddressItem
    {
        public JavaItem()
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