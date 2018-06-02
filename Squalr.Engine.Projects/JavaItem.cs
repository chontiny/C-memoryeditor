namespace Squalr.Engine.Projects
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    public class JavaItem : AddressItem
    {
        public JavaItem()
        {

        }

        public override void Update()
        {
        }

        protected override UInt64 ResolveAddress()
        {
            return 0;
        }
    }
    //// End class
}
//// End namespace