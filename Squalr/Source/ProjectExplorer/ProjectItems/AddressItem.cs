namespace Squalr.Source.ProjectExplorer.ProjectItems
{
    using Controls;
    using Engine;
    using System;
    using System.ComponentModel;
    using System.Runtime.Serialization;
    using Utils;
    using Utils.TypeConverters;

    /// <summary>
    /// Defines an address that can be added to the project explorer.
    /// </summary>
    [DataContract]
    internal abstract class AddressItem : ProjectItem
    {
        /// <summary>
        /// The data type at this address.
        /// </summary>
        [Browsable(false)]
        protected DataType dataType;

        /// <summary>
        /// The value at this address.
        /// </summary>
        [Browsable(false)]
        protected Object addressValue;

        /// <summary>
        /// A value indicating whether the value at this address should be displayed as hex.
        /// </summary>
        [Browsable(false)]
        protected Boolean isValueHex;

        /// <summary>
        /// The effective address after tracing all pointer offsets.
        /// </summary>
        [Browsable(false)]
        protected IntPtr calculatedAddress;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddressItem" /> class.
        /// </summary>
        public AddressItem() : this(typeof(Int32), "New Address")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AddressItem" /> class.
        /// </summary>
        /// <param name="baseAddress">The base address. This will be added as an offset from the resolved base identifier.</param>
        /// <param name="dataType">The data type of the value at this address.</param>
        /// <param name="description">The description of this address.</param>
        /// <param name="baseIdentifier">The identifier for the base address of this object.</param>
        /// <param name="offsets">The pointer offsets of this address item.</param>
        /// <param name="isValueHex">A value indicating whether the value at this address should be displayed as hex.</param>
        /// <param name="value">The value at this address. If none provided, it will be figured out later. Used here to allow immediate view updates upon creation.</param>
        public AddressItem(
            Type dataType,
            String description = "New Address",
            Boolean isValueHex = false,
            String value = null)
            : base(description)
        {
            // Bypass setters to avoid running setter code
            this.dataType = dataType;
            this.isValueHex = isValueHex;

            if (!this.isValueHex && CheckSyntax.CanParseValue(dataType, value))
            {
                this.addressValue = Conversions.ParsePrimitiveStringAsPrimitive(dataType, value);
            }
            else if (this.isValueHex && CheckSyntax.CanParseHex(dataType, value))
            {
                this.addressValue = Conversions.ParseHexStringAsPrimitiveString(dataType, value);
            }
        }

        /// <summary>
        /// Gets or sets the data type of the value at this address.
        /// </summary>
        [DataMember]
        [RefreshProperties(RefreshProperties.All)]
        [TypeConverter(typeof(DataTypeConverter))]
        [SortedCategory(SortedCategory.CategoryType.Advanced), DisplayName("Data Type"), Description("Data type of the calculated address")]
        public DataType DataType
        {
            get
            {
                return this.dataType;
            }

            set
            {
                if (this.dataType == value)
                {
                    return;
                }

                this.dataType = value;

                // Clear our current address value
                this.addressValue = null;

                ProjectExplorerViewModel.GetInstance().ProjectItemStorage.HasUnsavedChanges = true;
                this.NotifyPropertyChanged(nameof(this.DataType));
            }
        }

        /// <summary>
        /// Gets or sets the value at this address.
        /// </summary>
        [TypeConverter(typeof(DynamicConverter))]
        [SortedCategory(SortedCategory.CategoryType.Common), DisplayName("Value"), Description("Value at the calculated address")]
        public Object AddressValue
        {
            get
            {
                return this.addressValue;
            }

            set
            {
                this.addressValue = value;
                this.WriteValue(value);
                this.NotifyPropertyChanged(nameof(this.AddressValue));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the value at this address should be displayed as hex.
        /// </summary>
        [DataMember]
        [RefreshProperties(RefreshProperties.All)]
        [SortedCategory(SortedCategory.CategoryType.Advanced), DisplayName("Value as Hex"), Description("Whether the value is displayed as hexedecimal")]
        public Boolean IsValueHex
        {
            get
            {
                return this.isValueHex;
            }

            set
            {
                if (this.isValueHex == value)
                {
                    return;
                }

                this.isValueHex = value;
                ProjectExplorerViewModel.GetInstance().ProjectItemStorage.HasUnsavedChanges = true;
                this.NotifyPropertyChanged(nameof(this.IsValueHex));
            }
        }

        /// <summary>
        /// Gets the effective address after tracing all pointer offsets.
        /// </summary>
        [ReadOnly(true)]
        [TypeConverter(typeof(AddressConverter))]
        [SortedCategory(SortedCategory.CategoryType.Common), DisplayName("Calculated Address"), Description("The final computed address of this variable")]
        public IntPtr CalculatedAddress
        {
            get
            {
                return this.calculatedAddress;
            }

            protected set
            {
                if (this.calculatedAddress == value)
                {
                    return;
                }

                this.calculatedAddress = value;
                this.NotifyPropertyChanged(nameof(this.CalculatedAddress));
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
        /// Resolves the address of this object.
        /// </summary>
        /// <returns>The base address of this object.</returns>
        protected abstract IntPtr ResolveAddress();

        /// <summary>
        /// Writes a value to the computed address of this item.
        /// </summary>
        /// <param name="newValue">The value to write.</param>
        protected virtual void WriteValue(Object newValue)
        {
            if (newValue == null)
            {
                return;
            }

            EngineCore.GetInstance()?.OperatingSystem?.Write(this.DataType, this.CalculatedAddress, newValue);
        }
    }
    //// End class
}
//// End namespace