namespace Squalr.Source.Scanners.Pointers
{
    using GalaSoft.MvvmLight.CommandWpf;
    using Squalr.Source.Docking;
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Input;

    /// <summary>
    /// View model for the Input Correlator.
    /// </summary>
    internal class PointerScannerViewModel : ToolViewModel
    {
        /// <summary>
        /// Gets the default pointer scan depth.
        /// </summary>
        public const Int32 DefaultPointerScanDepth = 3;

        /// <summary>
        /// Gets the default pointer scan radius.
        /// </summary>
        public const Int32 DefaultPointerScanRadius = 2048;

        /// <summary>
        /// Gets the maximum pointer scan depth.
        /// </summary>
        public const Int32 MaximumPointerScanDepth = 7;

        /// <summary>
        /// Singleton instance of the <see cref="PointerScannerViewModel" /> class.
        /// </summary>
        private static Lazy<PointerScannerViewModel> inputCorrelatorViewModelInstance = new Lazy<PointerScannerViewModel>(
                () => { return new PointerScannerViewModel(); },
                LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// Prevents a default instance of the <see cref="PointerScannerViewModel" /> class from being created.
        /// </summary>
        private PointerScannerViewModel() : base("Pointer Scanner")
        {
            this.PointerScan = new PointerScan();
            this.PointerRescan = new PointerRescan();
            this.PointerValueRescan = new PointerValueRescan();
            this.PointerValidationScan = new PointerValidationScan();

            this.SetPointerScanAddressCommand = new RelayCommand<UInt64>((newValue) => this.TargetAddress = newValue, (newValue) => true);
            this.SetPointerRescanAddressCommand = new RelayCommand<UInt64>((newValue) => this.RescanAddress = newValue, (newValue) => true);
            this.SetPointerRescanValueCommand = new RelayCommand<Object>((newValue) => this.RescanValue = newValue, (newValue) => true);
            this.SetDepthCommand = new RelayCommand<Int32>((newValue) => this.PointerDepth = newValue, (newValue) => true);
            this.SetPointerRadiusCommand = new RelayCommand<Int32>((newValue) => this.PointerRadius = newValue, (newValue) => true);
            this.StartScanCommand = new RelayCommand(() => Task.Run(() => this.StartScan()), () => true);
            this.StartPointerRescanCommand = new RelayCommand(() => Task.Run(() => this.StartPointerRescan()), () => true);
            this.StartPointerValueRescanCommand = new RelayCommand(() => Task.Run(() => this.StartPointerValueRescan()), () => true);
            this.StartPointerValidationScanCommand = new RelayCommand(() => Task.Run(() => this.StartPointerValidationScan()), () => true);

            DockingViewModel.GetInstance().RegisterViewModel(this);
        }

        /// <summary>
        /// Gets a command to start the pointer scan on a specific project item.
        /// </summary>
        public ICommand PointerScanCommand { get; private set; }

        /// <summary>
        /// Gets a command to start the pointer scan.
        /// </summary>
        public ICommand StartScanCommand { get; private set; }

        /// <summary>
        /// Gets a command to start the pointer rescan.
        /// </summary>
        public ICommand StartPointerRescanCommand { get; private set; }

        /// <summary>
        /// Gets a command to start the pointer value rescan.
        /// </summary>
        public ICommand StartPointerValueRescanCommand { get; private set; }

        /// <summary>
        /// Gets a command to start the scan to remove invalid pointers.
        /// </summary>
        public ICommand StartPointerValidationScanCommand { get; private set; }

        /// <summary>
        /// Gets a command to set the pointer scan address.
        /// </summary>
        public ICommand SetPointerScanAddressCommand { get; private set; }

        /// <summary>
        /// Gets a command to set the pointer rescan address.
        /// </summary>
        public ICommand SetPointerRescanAddressCommand { get; private set; }

        /// <summary>
        /// Gets a command to set the pointer rescan value.
        /// </summary>
        public ICommand SetPointerRescanValueCommand { get; private set; }

        /// <summary>
        /// Gets a command to set the scan depth.
        /// </summary>
        public ICommand SetDepthCommand { get; private set; }

        /// <summary>
        /// Gets a command to set the pointer radius for the scan.
        /// </summary>
        public ICommand SetPointerRadiusCommand { get; private set; }

        /// <summary>
        /// Gets or sets the target scan address.
        /// </summary>
        public UInt64 TargetAddress
        {
            get
            {
                return this.PointerScan.TargetAddress;
            }

            set
            {
                this.PointerScan.TargetAddress = value;
                this.RaisePropertyChanged(nameof(this.TargetAddress));
            }
        }

        /// <summary>
        /// Gets or sets the target rescan address.
        /// </summary>
        public UInt64 RescanAddress
        {
            get
            {
                return this.PointerRescan.TargetAddress;
            }

            set
            {
                this.PointerRescan.TargetAddress = value;
                this.RaisePropertyChanged(nameof(this.PointerRescan));
            }
        }

        /// <summary>
        /// Gets or sets the target rescan value.
        /// </summary>
        public Object RescanValue
        {
            get
            {
                return this.PointerValueRescan.Value;
            }

            set
            {
                this.PointerValueRescan.Value = value;
                this.RaisePropertyChanged(nameof(this.RescanValue));
            }
        }

        /// <summary>
        /// Gets or sets the pointer depth of the scan.
        /// </summary>
        public Int32 PointerDepth
        {
            get
            {
                return this.PointerScan.PointerDepth;
            }

            set
            {
                this.PointerScan.PointerDepth = value;
                this.RaisePropertyChanged(nameof(this.PointerDepth));
            }
        }

        /// <summary>
        /// Gets or sets the pointer radius of the scan.
        /// </summary>
        public Int32 PointerRadius
        {
            get
            {
                return this.PointerScan.PointerRadius;
            }

            set
            {
                this.PointerScan.PointerRadius = value;
                this.RaisePropertyChanged(nameof(this.PointerRadius));
            }
        }

        /// <summary>
        /// Gets or sets the pointer scan task.
        /// </summary>
        private PointerScan PointerScan { get; set; }

        /// <summary>
        /// Gets or sets the pointer rescan task.
        /// </summary>
        private PointerRescan PointerRescan { get; set; }

        /// <summary>
        /// Gets or sets the pointer value rescan task.
        /// </summary>
        private PointerValueRescan PointerValueRescan { get; set; }

        /// <summary>
        /// Gets or sets the pointer validation scan task.
        /// </summary>
        private PointerValidationScan PointerValidationScan { get; set; }

        /// <summary>
        /// Gets a singleton instance of the <see cref="ChangeCounterViewModel"/> class.
        /// </summary>
        /// <returns>A singleton instance of the class.</returns>
        public static PointerScannerViewModel GetInstance()
        {
            return inputCorrelatorViewModelInstance.Value;
        }

        /// <summary>
        /// Starts the pointer scan.
        /// </summary>
        private void StartScan()
        {
            this.PointerScan.Start();
        }

        /// <summary>
        /// Starts the pointer address rescan.
        /// </summary>
        private void StartPointerRescan()
        {
            this.PointerRescan.Start();
        }

        /// <summary>
        /// Starts the pointer value rescan.
        /// </summary>
        private void StartPointerValueRescan()
        {
            this.PointerValueRescan.Start();
        }

        /// <summary>
        /// Starts the pointer validation scan.
        /// </summary>
        private void StartPointerValidationScan()
        {
            this.PointerValidationScan.Start();
        }
    }
    //// End class
}
//// End namespace