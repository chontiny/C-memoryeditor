namespace Squalr.Engine.Projects
{
    using Squalr.Engine.Memory;
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    public class DotNetItem : AddressItem
    {
        public DotNetItem()
        {
        }

        public DotNetItem(String name, Type type, String identifier) : base(type, name)
        {
            this.Identifier = identifier;
        }

        /// <summary>
        /// Gets or sets the identifier for this .NET object.
        /// </summary>
        [DataMember]
        public virtual String Identifier { get; set; }

        /// <summary>
        /// Resolves the address of this object.
        /// </summary>
        /// <returns>The base address of this object.</returns>
        protected override UInt64 ResolveAddress()
        {
            return AddressResolver.GetInstance().ResolveDotNetObject(this.Identifier);
        }
    }
    //// End class
}
//// End namespace