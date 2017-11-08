namespace Squalr.Source.Scanners.Pointers
{
    using GalaSoft.MvvmLight.CommandWpf;
    using SqualrCore.Source.Docking;
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
        /// The content id for the docking library associated with this view model.
        /// </summary>
        public const String ToolContentId = nameof(PointerScannerViewModel);

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
            this.ContentId = PointerScannerViewModel.ToolContentId;
            this.PointerScan = new PointerScan();

            this.SetAddressCommand = new RelayCommand<UInt64>((newValue) => this.TargetAddress = newValue, (newValue) => true);
            this.SetDepthCommand = new RelayCommand<UInt32>((newValue) => this.PointerDepth = newValue, (newValue) => true);
            this.SetPointerRadiusCommand = new RelayCommand<UInt32>((newValue) => this.PointerRadius = newValue, (newValue) => true);
            this.StartScanCommand = new RelayCommand(() => Task.Run(() => this.PointerScan.Start()), () => true);
            this.StopScanCommand = new RelayCommand(() => Task.Run(() => this.PointerScan.Cancel()), () => true);

            DockingViewModel.GetInstance().RegisterViewModel(this);
        }

        /// <summary>
        /// Gets a command to start the pointer scan.
        /// </summary>
        public ICommand StartScanCommand { get; private set; }

        /// <summary>
        /// Gets a command to stop the pointer scan.
        /// </summary>
        public ICommand StopScanCommand { get; private set; }

        /// <summary>
        /// Gets a command to set the active search value.
        /// </summary>
        public ICommand SetAddressCommand { get; private set; }

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
        /// Gets or sets the pointer depth of the scan.
        /// </summary>
        public UInt32 PointerDepth
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
        public UInt32 PointerRadius
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
        /// Gets a singleton instance of the <see cref="ChangeCounterViewModel"/> class.
        /// </summary>
        /// <returns>A singleton instance of the class.</returns>
        public static PointerScannerViewModel GetInstance()
        {
            return inputCorrelatorViewModelInstance.Value;
        }
    }
    //// End class
}
//// End namespace