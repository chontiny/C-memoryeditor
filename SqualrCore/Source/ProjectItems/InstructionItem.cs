namespace SqualrCore.Source.ProjectItems
{
    using SqualrCore.Content;
    using SqualrCore.Source.Controls;
    using SqualrCore.Source.Engine.VirtualMachines;
    using SqualrCore.Source.Utils;
    using SqualrCore.Source.Utils.Extensions;
    using SqualrCore.Source.Utils.TypeConverters;
    using System;
    using System.ComponentModel;
    using System.Runtime.Serialization;
    using System.Windows.Media.Imaging;

    [DataContract]
    public class InstructionItem : AddressItem
    {
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

        public InstructionItem() : base(null, "New Instruction")
        {

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

                // ProjectExplorerViewModel.GetInstance().ProjectItemStorage.HasUnsavedChanges = true;
                this.RaisePropertyChanged(nameof(this.InstructionBytes));
            }
        }

        /// <summary>
        /// Gets or sets the data type of the value at this address.
        /// </summary>
        [DataMember]
        [Browsable(true)]
        [RefreshProperties(RefreshProperties.All)]
        [SortedCategory(SortedCategory.CategoryType.Advanced), DisplayName("Instruction"), Description("The assembled instruction")]
        public String Instruction
        {
            get
            {
                return "Not Implemented";
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

                // ProjectExplorerViewModel.GetInstance().ProjectItemStorage.HasUnsavedChanges = true;
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
                // ProjectExplorerViewModel.GetInstance().ProjectItemStorage.HasUnsavedChanges = true;
                this.RaisePropertyChanged(nameof(this.ModuleOffset));
            }
        }

        protected override IntPtr ResolveAddress()
        {
            return AddressResolver.GetInstance().ResolveModule(this.ModuleName).Add(this.ModuleOffset);
        }
    }
    //// End class
}
//// End namespace