namespace Squalr.Engine.Projects.Items
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    public class JavaItem : AddressItem
    {
        /// <summary>
        /// The extension for this project item type.
        /// </summary>
        public new const String Extension = ".jvm";

        public JavaItem()
        {

        }

        /// <summary>
        /// Gets the extension for this project item.
        /// </summary>
        public override String GetExtension()
        {
            return JavaItem.Extension;
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