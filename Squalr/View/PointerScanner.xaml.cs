namespace Squalr.View
{
    using Source.Results.ScanResults;
    using Squalr.Source.Scanners.Pointers;
    using SqualrCore.Source.Controls;
    using SqualrCore.Source.Utils;
    using System;
    using System.Threading.Tasks;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for PointerScanner.xaml.
    /// </summary>
    internal partial class PointerScanner : UserControl, IScanResultsObserver
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ManualScanner"/> class.
        /// </summary>
        public PointerScanner()
        {
            this.InitializeComponent();

            // Windows Forms hosting -- TODO: Phase this out
            this.ValueHexDecBox = new HexDecTextBox();
            this.ValueHexDecBox.IsHex = true;
            this.ValueHexDecBox.ElementType = typeof(UInt64);
            this.ValueHexDecBox.TextChanged += this.ValueUpdated;
            this.valueHexDecBox.Children.Add(WinformsHostingHelper.CreateHostedControl(this.ValueHexDecBox));

            Task.Run(() => ScanResultsViewModel.GetInstance().Subscribe(this));
        }

        /// <summary>
        /// Gets the view model associated with this view.
        /// </summary>
        public PointerScannerViewModel PointerScannerViewModel
        {
            get
            {
                return this.DataContext as PointerScannerViewModel;
            }
        }

        /// <summary>
        /// Gets or sets the value hex dec box used to display the current value being edited.
        /// </summary>
        private HexDecTextBox ValueHexDecBox { get; set; }

        /// <summary>
        /// Updates the active type.
        /// </summary>
        /// <param name="activeType">The new active type.</param>
        public void Update(Type activeType)
        {
            this.ValueHexDecBox.ElementType = activeType;
        }

        /// <summary>
        /// Invoked when the current value is changed, and informs the viewmodel.
        /// </summary>
        /// <param name="sender">Sending object.</param>
        /// <param name="e">Event args.</param>
        private void ValueUpdated(Object sender, EventArgs e)
        {
            Object value = this.ValueHexDecBox.GetValue();
            this.PointerScannerViewModel.SetAddressCommand.Execute(value == null ? 0 : Conversions.ParsePrimitiveStringAsPrimitive(typeof(UInt64), value.ToString()));
        }
    }
    //// End class
}
//// End namespace