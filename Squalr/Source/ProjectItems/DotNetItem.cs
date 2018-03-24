namespace Squalr.Source.ProjectItems
{
    using Squalr.Engine.Memory;
    using Squalr.Source.Controls;
    using System;
    using System.ComponentModel;
    using System.Runtime.Serialization;

    [DataContract]
    public class DotNetItem : AddressItem
    {
        public DotNetItem(String name, Type type, String identifier) : base(type, name)
        {
            this.Identifier = identifier;
        }

        /// <summary>
        /// Gets or sets the identifier for this .NET object.
        /// </summary>
        [DataMember]
        [Browsable(true)]
        [RefreshProperties(RefreshProperties.All)]
        [SortedCategory(SortedCategory.CategoryType.Common), DisplayName("Value as Hex"), Description("Whether the value is displayed as hexedecimal")]
        public String Identifier { get; set; }

        /// <summary>
        /// Resolves the address of this object.
        /// </summary>
        /// <returns>The base address of this object.</returns>
        protected override IntPtr ResolveAddress()
        {
            return AddressResolver.GetInstance().ResolveDotNetObject(this.Identifier);
        }
    }
    //// End class
}
//// End namespace