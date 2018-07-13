namespace Squalr.View
{
    using Squalr.Engine.DataTypes;
    using Squalr.Source.Controls;
    using Squalr.Source.Results;
    using Squalr.Source.Scanning;
    using System;
    using System.Threading.Tasks;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for ManualScanner.xaml.
    /// </summary>
    internal partial class ManualScanner : UserControl, IResultDataTypeObserver
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ManualScanner"/> class.
        /// </summary>
        public ManualScanner()
        {
            this.InitializeComponent();

            // Windows Forms hosting -- TODO: Phase this out
            this.ValueHexDecBox = new HexDecTextBox();
            this.ValueHexDecBox.TextChanged += this.ValueUpdated;
            this.valueHexDecBox.Children.Add(WinformsHostingHelper.CreateHostedControl(this.ValueHexDecBox));

            Task.Run(() => ScanResultsViewModel.GetInstance().Subscribe(this));
        }

        /// <summary>
        /// Gets the view model associated with this view.
        /// </summary>
        public ManualScannerViewModel ManualScannerViewModel
        {
            get
            {
                return this.DataContext as ManualScannerViewModel;
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
        public void Update(DataType activeType)
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
            this.ManualScannerViewModel.UpdateActiveValueCommand.Execute(this.ValueHexDecBox.GetValue());
        }
    }
    //// End class
}
//// End namespace