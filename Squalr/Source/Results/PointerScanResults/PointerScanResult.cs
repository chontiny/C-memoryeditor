namespace Squalr.Source.Results.PointerScanResults
{
    using System;
    using System.ComponentModel;

    /// <summary>
    /// A scan result object that can be displayed to the user and added to the project explorer.
    /// </summary>
    internal class PointerScanResult : INotifyPropertyChanged
    {
        /// <summary>
        /// The abase address of the pointer scan result.
        /// </summary>
        private IntPtr baseAddress;

        /// <summary>
        /// The value of the scan result.
        /// </summary>
        private String elementValue;

        /// <summary>
        /// The offsets of the pointer scan result.
        /// </summary>
        private String offsets;

        /// <summary>
        /// Initializes a new instance of the <see cref="PointerScanResult" /> class.
        /// </summary>
        /// <param name="baseAddress">The base address.</param>
        /// <param name="offsets">The current scan offsets.</param>
        public PointerScanResult(IntPtr baseAddress, String offsets)
        {
            this.BaseAddress = baseAddress;
            this.Offsets = offsets;
        }

        /// <summary>
        /// Occurs after a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets or sets the base address of the pointer scan result.
        /// </summary>
        public IntPtr BaseAddress
        {
            get
            {
                return this.baseAddress;
            }

            set
            {
                this.baseAddress = value;

                this.RaisePropertyChanged(nameof(this.BaseAddress));
            }
        }

        /// <summary>
        /// Gets or sets the value of the scan result.
        /// </summary>
        public String ElementValue
        {
            get
            {
                return this.elementValue;
            }

            set
            {
                this.elementValue = value;

                this.RaisePropertyChanged(nameof(this.ElementValue));
            }
        }

        /// <summary>
        /// Gets or sets the value of the pointer scan result.
        /// </summary>
        public String Offsets
        {
            get
            {
                return this.offsets;
            }

            set
            {
                this.offsets = value;

                this.RaisePropertyChanged(nameof(this.Offsets));
            }
        }

        /// <summary>
        /// Indicates that a given property in this project item has changed.
        /// </summary>
        /// <param name="propertyName">The name of the changed property.</param>
        protected void RaisePropertyChanged(String propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    //// End class
}
//// End namespace