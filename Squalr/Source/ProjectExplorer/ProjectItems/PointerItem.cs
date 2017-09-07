namespace Squalr.Source.ProjectExplorer.ProjectItems
{
    using Controls;
    using Editors.OffsetEditor;
    using Engine;
    using Engine.AddressResolver;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Linq;
    using System.Runtime.Serialization;
    using Utils.Extensions;
    using Utils.TypeConverters;

    /// <summary>
    /// Defines an address that can be added to the project explorer.
    /// </summary>
    [DataContract]
    internal class PointerItem : AddressItem
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
        public PointerItem() : this(IntPtr.Zero, typeof(Int32), "New Address")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AddressItem" /> class.
        /// </summary>
        /// <param name="baseAddress">The base address. This will be added as an offset from the resolved base identifier.</param>
        /// <param name="elementType">The data type of the value at this address.</param>
        /// <param name="description">The description of this address.</param>
        /// <param name="resolveType">The identifier type for this address item.</param>
        /// <param name="moduleName">The identifier for the base address of this object.</param>
        /// <param name="pointerOffsets">The pointer offsets of this address item.</param>
        /// <param name="isValueHex">A value indicating whether the value at this address should be displayed as hex.</param>
        /// <param name="value">The value at this address. If none provided, it will be figured out later. Used here to allow immediate view updates upon creation.</param>
        public PointerItem(
            IntPtr baseAddress,
            Type elementType,
            String description = "New Address",
            String moduleName = null,
            IEnumerable<Int32> pointerOffsets = null,
            Boolean isValueHex = false,
            String value = null)
            : base(elementType, description, isValueHex, value)
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
                ProjectExplorerViewModel.GetInstance().HasUnsavedChanges = true;
                this.NotifyPropertyChanged(nameof(this.ModuleName));
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
                ProjectExplorerViewModel.GetInstance().HasUnsavedChanges = true;
                this.NotifyPropertyChanged(nameof(this.ModuleOffset));
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
                ProjectExplorerViewModel.GetInstance().HasUnsavedChanges = true;
                this.NotifyPropertyChanged(nameof(this.PointerOffsets));
            }
        }

        /// <summary>
        /// Update event for this project item. Resolves addresses and values.
        /// </summary>
        public override void Update()
        {
            this.CalculatedAddress = this.ResolveAddress();

            if (this.IsActivated)
            {
                // Freeze current value if this entry is activated
                Object value = this.AddressValue;
                if (value != null && value.GetType() == this.DataType)
                {
                    this.WriteValue(value);
                }
            }
            else
            {
                // Otherwise we read as normal (bypass value setter and set value directly to avoid a write-back to memory)
                Boolean readSuccess;
                this.addressValue = EngineCore.GetInstance()?.OperatingSystem?.Read(this.DataType, this.CalculatedAddress, out readSuccess);
            }
        }

        /// <summary>
        /// Clones the project item.
        /// </summary>
        /// <returns>The clone of the project item.</returns>
        public override ProjectItem Clone()
        {
            PointerItem clone = new PointerItem();
            clone.description = this.Description;
            clone.parent = this.Parent;
            clone.extendedDescription = this.ExtendedDescription;
            clone.moduleName = this.moduleName;
            clone.moduleOffset = this.moduleOffset;
            clone.pointerOffsets = this.pointerOffsets?.ToArray();
            clone.dataType = this.dataType;
            clone.addressValue = this.addressValue;
            clone.isValueHex = this.isValueHex;
            clone.calculatedAddress = this.calculatedAddress;

            return clone;
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
                if (EngineCore.GetInstance().Processes.IsOpenedProcess32Bit())
                {
                    pointer = EngineCore.GetInstance().OperatingSystem.Read<Int32>(pointer, out successReading).ToIntPtr();
                }
                else
                {
                    pointer = EngineCore.GetInstance().OperatingSystem.Read<Int64>(pointer, out successReading).ToIntPtr();
                }

                pointer = pointer.Add(offset);

                if (pointer == IntPtr.Zero || !successReading)
                {
                    pointer = IntPtr.Zero;
                    break;
                }
            }

            return pointer;
        }
    }
    //// End class
}
//// End namespace