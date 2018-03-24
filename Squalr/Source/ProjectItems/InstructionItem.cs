namespace Squalr.Source.ProjectItems
{
    using Squalr.Content;
    using Squalr.Engine.DataTypes;
    using Squalr.Engine.Memory;
    using Squalr.Engine.Utils;
    using Squalr.Source.Controls;
    using Squalr.Source.Utils.Extensions;
    using Squalr.Source.Utils.TypeConverters;
    using System;
    using System.ComponentModel;
    using System.Runtime.Serialization;
    using System.Windows.Media.Imaging;

    [DataContract]
    public class InstructionItem : AddressItem
    {
        /// <summary>
        /// The disassembled instruction.
        /// </summary>
        [Browsable(false)]
        public String instruction;

        /// <summary>
        /// The identifier for the base address of this object.
        /// </summary>
        [Browsable(false)]
        private String moduleName;

        /// <summary>
        /// The base address of this object. This will be added as an offset from the resolved base identifier.
        /// </summary>
        [Browsable(false)]
        private IntPtr moduleOffset;

        [Browsable(false)]
        private Byte[] precedingBytes;

        [Browsable(false)]
        private Byte[] instructionBytes;

        [Browsable(false)]
        private Byte[] followingBytes;

        public InstructionItem() : this(IntPtr.Zero, null, null, null)
        {
        }

        public InstructionItem(IntPtr BaseAddress, String moduleName, String instruction, Byte[] instructionBytes) : base(DataType.ArrayOfBytes, "New Instruction")
        {
            this.ModuleOffset = BaseAddress;
            this.ModuleName = moduleName;
            this.Instruction = instruction;
            this.InstructionBytes = instructionBytes;
        }

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
                return this.addressValue;
            }

            set
            {
                // Assemble and write bytes
                this.RaisePropertyChanged(nameof(this.AddressValue));
            }
        }

        /// <summary>
        /// Gets or sets the data type of the value at this address.
        /// </summary>
        [DataMember]
        [Browsable(false)]
        public Byte[] PrecedingBytes
        {
            get
            {
                return this.precedingBytes;
            }

            set
            {
                if (this.precedingBytes == value)
                {
                    return;
                }

                this.precedingBytes = value;
                this.RaisePropertyChanged(nameof(this.PrecedingBytes));
            }
        }

        /// <summary>
        /// Gets or sets the data type of the value at this address.
        /// </summary>
        [DataMember]
        [Browsable(true)]
        [RefreshProperties(RefreshProperties.All)]
        [SortedCategory(SortedCategory.CategoryType.Advanced), DisplayName("Instruction Bytes"), Description("The bytes of the instruction")]
        public Byte[] InstructionBytes
        {
            get
            {
                return this.instructionBytes;
            }

            set
            {
                if (this.instructionBytes == value)
                {
                    return;
                }

                this.instructionBytes = value;

                this.RaisePropertyChanged(nameof(this.InstructionBytes));
            }
        }

        /// <summary>
        /// Gets or sets the disassembled instruction.
        /// </summary>
        [DataMember]
        [Browsable(true)]
        [RefreshProperties(RefreshProperties.All)]
        [SortedCategory(SortedCategory.CategoryType.Advanced), DisplayName("Instruction"), Description("The disassembled instruction")]
        public String Instruction
        {
            get
            {
                return this.instruction;
            }

            set
            {
                this.instruction = value;
                this.RaisePropertyChanged(nameof(this.Instruction));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the value at this address should be displayed as hex.
        /// </summary>
        [DataMember]
        [Browsable(false)]
        public override Boolean IsValueHex
        {
            get
            {
                return true;
            }

            set
            {
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the value at this address should be displayed as hex.
        /// </summary>
        [DataMember]
        [Browsable(false)]
        public override DataType DataType
        {
            get
            {
                return null;
            }

            set
            {
            }
        }

        /// <summary>
        /// Gets the image associated with this project item.
        /// </summary>
        public override BitmapSource Icon
        {
            get
            {
                return Images.Cpu;
            }
        }

        /// <summary>
        /// Gets or sets the data type of the value at this address.
        /// </summary>
        [DataMember]
        [Browsable(false)]
        public Byte[] FollowingBytes
        {
            get
            {
                return this.followingBytes;
            }

            set
            {
                if (this.followingBytes == value)
                {
                    return;
                }

                this.followingBytes = value;
                this.RaisePropertyChanged(nameof(this.FollowingBytes));
            }
        }

        /// <summary>
        /// Gets or sets the identifier for the base address of this object.
        /// </summary>
        [DataMember]
        [Browsable(true)]
        [RefreshProperties(RefreshProperties.All)]
        [SortedCategory(SortedCategory.CategoryType.Advanced), DisplayName("Module Name"), Description("The module to use as a base address")]
        public String ModuleName
        {
            get
            {
                return this.moduleName;
            }

            set
            {
                if (this.moduleName == value)
                {
                    return;
                }

                this.moduleName = value == null ? String.Empty : value;

                this.RaisePropertyChanged(nameof(this.ModuleName));
            }
        }

        /// <summary>
        /// Gets or sets the base address of this object. This will be added as an offset from the resolved base identifier.
        /// </summary>
        [DataMember]
        [RefreshProperties(RefreshProperties.All)]
        [TypeConverter(typeof(AddressConverter))]
        [SortedCategory(SortedCategory.CategoryType.Advanced), DisplayName("Module Offset"), Description("The offset from the module address. If no module address, then this is the base address.")]
        public IntPtr ModuleOffset
        {
            get
            {
                return this.moduleOffset;
            }

            set
            {
                if (this.moduleOffset == value)
                {
                    return;
                }

                this.CalculatedAddress = value;
                this.moduleOffset = value;
                this.RaisePropertyChanged(nameof(this.ModuleOffset));
                this.RaisePropertyChanged(nameof(this.AddressSpecifier));
            }
        }

        /// <summary>
        /// Gets the address specifier for this address. If a static address, this is 'ModuleName + offset', otherwise this is an address string.
        /// </summary>
        [Browsable(false)]
        public String AddressSpecifier
        {
            get
            {
                if (!this.ModuleName.IsNullOrEmpty())
                {
                    return this.ModuleName + "+" + Conversions.ToHex(this.ModuleOffset, formatAsAddress: true);
                }
                else
                {
                    return Conversions.ToHex(this.ModuleOffset, formatAsAddress: true);
                }
            }
        }

        /// <summary>
        /// Resolves the address of this instruction.
        /// </summary>
        /// <returns>The base address of this instruction.</returns>
        protected override IntPtr ResolveAddress()
        {
            return AddressResolver.GetInstance().ResolveModule(this.ModuleName).Add(this.ModuleOffset);
        }
    }
    //// End class
}
//// End namespace