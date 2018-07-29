namespace Squalr.View
{
    using Squalr.Engine.DataTypes;
    using Squalr.Source.Controls;
    using Squalr.Source.Results;
    using Squalr.Source.Scanning;
    using System;
    using System.Threading.Tasks;
    using System.Windows;

    /// <summary>
    /// Interaction logic for MainWindow.xaml.
    /// </summary>
    internal partial class MainWindow : Window, IResultDataTypeObserver
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ManualScanner"/> class.
        /// </summary>
        public MainWindow()
        {
            this.InitializeComponent();

            this.HexDecBoxViewModel = this.ValueHexDecBox.DataContext as HexDecBoxViewModel;
            this.HexDecBoxViewModel.PropertyChanged += HexDecBoxViewModelPropertyChanged;

            Task.Run(() => ScanResultsViewModel.GetInstance().Subscribe(this));
        }

        private HexDecBoxViewModel HexDecBoxViewModel { get; set; }

        /// <summary>
        /// Updates the active type.
        /// </summary>
        /// <param name="activeType">The new active type.</param>
        public void Update(DataType activeType)
        {
            this.HexDecBoxViewModel.ElementType = activeType;
        }

        private void HexDecBoxViewModelPropertyChanged(Object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(HexDecBoxViewModel.Text))
            {
                ManualScannerViewModel.GetInstance().UpdateActiveValueCommand.Execute(this.HexDecBoxViewModel.GetValue());
            }
        }
    }
    //// End class
}
//// End namespace