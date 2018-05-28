namespace Squalr.Engine.Projects
{
    using Squalr.Engine.DataTypes;
    using Squalr.Engine.Utils;
    using Squalr.Engine.Utils.Extensions;
    using System;
    using System.ComponentModel;
    using System.Runtime.Serialization;

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
        private UInt64 moduleOffset;

        [Browsable(false)]
        private Byte[] precedingBytes;

        [Browsable(false)]
        private Byte[] instructionBytes;

        [Browsable(false)]
        private Byte[] followingBytes;

        public InstructionItem() : this(0, null, null, null)
        {
        }

        public InstructionItem(UInt64 BaseAddress, String moduleName, String instruction, Byte[] instructionBytes) : base(DataType.ByteArray, "New Instruction")
        {
            this.ModuleOffset = BaseAddress;
            this.ModuleName = moduleName;
            this.Instruction = instruction;
            this.InstructionBytes = instructionBytes;
        }

        /// <summary>
        /// Gets or sets the value at this address.
        /// </summary>
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
        public virtual Byte[] InstructionBytes
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
        public virtual String Instruction
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
        /// Gets or sets the data type of the value at this address.
        /// </summary>
        [DataMember]
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
        public virtual String ModuleName
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
        public virtual UInt64 ModuleOffset
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
        protected override UInt64 ResolveAddress()
        {
            return 0; // return AddressResolver.GetInstance().ResolveModule(this.ModuleName).Add(this.ModuleOffset);
        }
    }
    //// End class
}
//// End namespace