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

        public override ProjectItem Clone()
        {
            throw new NotImplementedException();
        }


        protected override IntPtr ResolveAddress()
        {
            throw new NotImplementedException();
        }
    }
    //// End class
}
//// End namespace