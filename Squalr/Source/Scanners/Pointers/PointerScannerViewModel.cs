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
            this.PointerScan = new PointerScan(0x122840d0); // TODO: Remove temp debugging value

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