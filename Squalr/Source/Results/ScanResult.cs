namespace Squalr.Source.Results
{
    using SqualrCore.Source.ProjectItems;
    using System;
    using System.ComponentModel;

    /// <summary>
    /// A scan result object that can be displayed to the user and added to the project explorer.
    /// </summary>
    internal class ScanResult : PointerItem
    {
        /// <summary>
        /// The previous value of the scan result.
        /// </summary>
        private Object previousValue;

        /// <summary>
        /// The label of the scan result.
        /// </summary>
        private String label;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScanResult" /> class.
        /// </summary>
        /// <param name="baseAddress">The base address. This will be added as an offset from the resolved base identifier.</param>
        /// <param name="dataType">The data type of the value at this address.</param>
        /// <param name="value">The initial value of this result.</param>
        /// <param name="previousValue">The previous scan value.</param>
        /// <param name="label">The label of this result.</param>
        public ScanResult(String moduleName, IntPtr baseAddress, Type dataType, Object value, Object previousValue, String label) : base(baseAddress, dataType, moduleName: moduleName, value: value)
        {
            this.PreviousValue = previousValue;
            this.Label = label;
        }

        /// <summary>
        /// Gets or sets the previous value of the scan result.
        /// </summary>
        [Browsable(false)]
        public Object PreviousValue
        {
            get
            {
                return this.previousValue;
            }

            set
            {
                this.previousValue = value;
                this.RaisePropertyChanged(nameof(this.PreviousValue));
            }
        }

        /// <summary>
        /// Gets or sets the label of the scan result.
        /// </summary>
        [Browsable(false)]
        public String Label
        {
            get
            {
                return this.label;
            }

            set
            {
                this.label = value;
                this.RaisePropertyChanged(nameof(this.Label));
            }
        }
    }
    //// End class
}
//// End namespace