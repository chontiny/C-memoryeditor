namespace Squalr.Source.ProjectItems
{
    using Editors.OffsetEditor;
    using Squalr.Content;
    using Squalr.Engine;
    using Squalr.Engine.DataTypes;
    using Squalr.Engine.Memory;
    using Squalr.Engine.Processes;
    using Squalr.Engine.Utils;
    using Squalr.Source.Controls;
    using Squalr.Source.Utils.Extensions;
    using Squalr.Source.Utils.TypeConverters;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Windows.Media.Imaging;

    /// <summary>
    /// Defines an address that can be added to the project explorer.
    /// </summary>
    [DataContract]
    public class PointerItem : AddressItem
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

        /// <summary>
        /// The pointer offsets of this address item.
        /// </summary>
        [Browsable(false)]
        private IEnumerable<Int32> pointerOffsets;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddressItem" /> class.
        /// </summary>
        public PointerItem() : this(IntPtr.Zero, DataType.Int32, "New Address")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AddressItem" /> class.
        /// </summary>
        /// <param name="baseAddress">The base address. This will be added as an offset from the resolved base identifier.</param>
        /// <param name="dataType">The data type of the value at this address.</param>
        /// <param name="description">The description of this address.</param>
        /// <param name="resolveType">The identifier type for this address item.</param>
        /// <param name="moduleName">The identifier for the base address of this object.</param>
        /// <param name="pointerOffsets">The pointer offsets of this address item.</param>
        /// <param name="isValueHex">A value indicating whether the value at this address should be displayed as hex.</param>
        /// <param name="value">The value at this address. If none provided, it will be figured out later. Used here to allow immediate view updates upon creation.</param>
        public PointerItem(
            IntPtr baseAddress,
            Type dataType,
            String description = "New Address",
            String moduleName = null,
            IEnumerable<Int32> pointerOffsets = null,
            Boolean isValueHex = false,
            Object value = null)
            : base(dataType, description, isValueHex, value)
        {
            // Bypass setters to avoid running setter code
            this.moduleOffset = baseAddress;
            this.moduleName = moduleName;
            this.pointerOffsets = pointerOffsets;
        }

        /// <summary>
        /// Gets or sets the identifier for the base address of this object.
        /// </summary>
        [DataMember]
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
                this.RaisePropertyChanged(nameof(this.IsStatic));
                this.RaisePropertyChanged(nameof(this.AddressSpecifier));
                this.RaisePropertyChanged(nameof(this.Icon));
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
                this.RaisePropertyChanged(nameof(this.AddressSpecifier));
            }
        }

        /// <summary>
        /// Gets or sets the pointer offsets of this address item.
        /// </summary>
        [DataMember]
        [RefreshProperties(RefreshProperties.All)]
        [TypeConverter(typeof(OffsetConverter))]
        [Editor(typeof(OffsetEditorModel), typeof(UITypeEditor))]
        [SortedCategory(SortedCategory.CategoryType.Advanced), DisplayName("Pointer Offsets"), Description("The pointer offsets used to calculate the final address")]
        public IEnumerable<Int32> PointerOffsets
        {
            get
            {
                return this.pointerOffsets;
            }

            set
            {
                if (this.pointerOffsets != null && this.pointerOffsets.SequenceEqual(value))
                {
                    return;
                }

                this.pointerOffsets = value;
                // ProjectExplorerViewModel.GetInstance().ProjectItemStorage.HasUnsavedChanges = true;
                this.RaisePropertyChanged(nameof(this.PointerOffsets));
                this.RaisePropertyChanged(nameof(this.IsPointer));
                this.RaisePropertyChanged(nameof(this.AddressSpecifier));
                this.RaisePropertyChanged(nameof(this.Icon));
            }
        }

        /// <summary>
        /// Gets the image associated with this project item.
        /// </summary>
        [Browsable(false)]
        public override BitmapSource Icon
        {
            get
            {
                if (this.IsPointer)
                {
                    return Images.LetterP;
                }
                else if (this.IsStatic)
                {
                    return Images.LetterS;
                }

                return Images.CollectValues;
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
                if (this.IsStatic)
                {
                    if (this.ModuleOffset.ToInt64() >= 0)
                    {
                        return this.ModuleName + " + " + Conversions.ToHex(this.ModuleOffset, formatAsAddress: false);
                    }
                    else
                    {
                        return this.ModuleName + " - " + Conversions.ParsePrimitiveAsHexString(DataType.IntPtr, this.ModuleOffset, signHex: true).TrimStart('-');
                    }
                }
                else if (this.IsPointer)
                {
                    return Conversions.ToHex(this.CalculatedAddress);
                }
                else
                {
                    return Conversions.ToHex(this.ModuleOffset);
                }
            }
        }

        /// <summary>
        /// Gets a value indicating if this pointer/address is static.
        /// </summary>
        [Browsable(false)]
        public Boolean IsStatic
        {
            get
            {
                return !this.ModuleName.IsNullOrEmpty();
            }
        }

        /// <summary>
        /// Gets a value indicating if this object is a true pointer and not just an address.
        /// </summary>
        [Browsable(false)]
        public Boolean IsPointer
        {
            get
            {
                return !this.PointerOffsets.IsNullOrEmpty();
            }
        }

        /// <summary>
        /// Resolves the address of an address, pointer, or managed object.
        /// </summary>
        /// <returns>The base address of this object.</returns>
        protected override IntPtr ResolveAddress()
        {
            IntPtr pointer = AddressResolver.GetInstance().ResolveModule(this.ModuleName);
            Boolean successReading = true;

            pointer = pointer.Add(this.ModuleOffset);

            if (this.PointerOffsets == null || this.PointerOffsets.Count() == 0)
            {
                return pointer;
            }

            foreach (Int32 offset in this.PointerOffsets)
            {
                if (ProcessAdapterFactory.GetProcessAdapter().IsOpenedProcess32Bit())
                {
                    pointer = Eng.GetInstance().VirtualMemory.Read<Int32>(pointer, out successReading).ToIntPtr();
                }
                else
                {
                    pointer = Eng.GetInstance().VirtualMemory.Read<Int64>(pointer, out successReading).ToIntPtr();
                }

                if (pointer == IntPtr.Zero || !successReading)
                {
                    pointer = IntPtr.Zero;
                    break;
                }

                pointer = pointer.Add(offset);
            }

            return pointer;
        }
    }
    //// End class
}
//// End namespace