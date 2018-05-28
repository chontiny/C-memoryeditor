namespace Squalr.Source.ProjectExplorer.ProjectItems
{
    using Squalr.Engine.DataTypes;
    using Squalr.Engine.Projects;
    using Squalr.Source.Controls;
    using Squalr.Source.Utils.TypeConverters;
    using System;
    using System.ComponentModel;
    using System.Runtime.Serialization;

    /// <summary>
    /// Decorates the base project item class with annotations for use in the view.
    /// </summary>
    internal class InstructionItemDecorator : InstructionItem
    {
        /// <summary>
        /// Gets or sets the value at this address.
        /// </summary>
        [Browsable(true)]
        [TypeConverter(typeof(DynamicConverter))]
        [SortedCategory(SortedCategory.CategoryType.Common), DisplayName("Value"), Description("Value at the calculated address")]
        public override Object AddressValue
        {
            get
            {
                return base.AddressValue;
            }

            set
            {
                base.AddressValue = value;
            }
        }

        [DataMember]
        [Browsable(true)]
        [RefreshProperties(RefreshProperties.All)]
        [SortedCategory(SortedCategory.CategoryType.Advanced), DisplayName("Instruction Bytes"), Description("The bytes of the instruction")]
        public override Byte[] InstructionBytes
        {
            get
            {
                return base.InstructionBytes;
            }

            set
            {
                base.InstructionBytes = value;
            }
        }

        /// <summary>
        /// Gets or sets the disassembled instruction.
        /// </summary>
        [DataMember]
        [Browsable(true)]
        [RefreshProperties(RefreshProperties.All)]
        [SortedCategory(SortedCategory.CategoryType.Advanced), DisplayName("Instruction"), Description("The disassembled instruction")]
        public override String Instruction
        {
            get
            {
                return base.Instruction;
            }

            set
            {
                base.Instruction = value;
            }
        }

        /// <summary>
        /// Gets or sets the identifier for the base address of this object.
        /// </summary>
        [DataMember]
        [Browsable(true)]
        [RefreshProperties(RefreshProperties.All)]
        [SortedCategory(SortedCategory.CategoryType.Advanced), DisplayName("Module Name"), Description("The module to use as a base address")]
        public override String ModuleName
        {
            get
            {
                return base.ModuleName;
            }

            set
            {
                base.ModuleName = value;
            }
        }

        /// <summary>
        /// Gets or sets the base address of this object. This will be added as an offset from the resolved base identifier.
        /// </summary>
        [DataMember]
        [RefreshProperties(RefreshProperties.All)]
        [TypeConverter(typeof(AddressConverter))]
        [SortedCategory(SortedCategory.CategoryType.Advanced), DisplayName("Module Offset"), Description("The offset from the module address. If no module address, then this is the base address.")]
        public override UInt64 ModuleOffset
        {
            get
            {
                return base.ModuleOffset;
            }

            set
            {
                this.ModuleOffset = value;
            }
        }

        /// <summary>
        /// Gets or sets the data type of the value at this address.
        /// </summary>
        [DataMember]
        [Browsable(true)]
        [RefreshProperties(RefreshProperties.All)]
        [TypeConverter(typeof(DataTypeConverter))]
        [SortedCategory(SortedCategory.CategoryType.Advanced), DisplayName("Data Type"), Description("Data type of the calculated address")]
        public override DataType DataType
        {
            get
            {
                return base.DataType;
            }

            set
            {
                base.DataType = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the value at this address should be displayed as hex.
        /// </summary>
        [DataMember]
        [Browsable(true)]
        [RefreshProperties(RefreshProperties.All)]
        [SortedCategory(SortedCategory.CategoryType.Advanced), DisplayName("Value as Hex"), Description("Whether the value is displayed as hexedecimal")]
        public override Boolean IsValueHex
        {
            get
            {
                return base.IsValueHex;
            }

            set
            {
                base.IsValueHex = value;
            }
        }

        /// <summary>
        /// Gets the effective address after tracing all pointer offsets.
        /// </summary>
        [ReadOnly(true)]
        [TypeConverter(typeof(AddressConverter))]
        [SortedCategory(SortedCategory.CategoryType.Common), DisplayName("Calculated Address"), Description("The final computed address of this variable")]
        public override UInt64 CalculatedAddress
        {
            get
            {
                return base.CalculatedAddress;
            }

            protected set
            {
                base.CalculatedAddress = value;
            }
        }
    }
    //// End class
}
//// End namespace