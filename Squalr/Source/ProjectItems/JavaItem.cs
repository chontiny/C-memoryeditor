namespace Squalr.Source.ProjectItems
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    public class JavaItem : AddressItem
    {
        public JavaItem()
        {

        }

        protected override UInt64 ResolveAddress()
        {
            throw new NotImplementedException();
        }
    }
    //// End class
}
//// End namespace