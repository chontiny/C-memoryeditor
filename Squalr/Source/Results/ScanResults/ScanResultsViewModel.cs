namespace Squalr.Source.Results.ScanResults
{
    using GalaSoft.MvvmLight.CommandWpf;
    using Snapshots;
    using Squalr.Properties;
    using Squalr.Source.ProjectExplorer;
    using SqualrCore.Content;
    using SqualrCore.Source.Docking;
    using SqualrCore.Source.Engine;
    using SqualrCore.Source.PropertyViewer;
    using SqualrCore.Source.Utils;
    using SqualrCore.Source.Utils.Extensions;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using System.Windows.Media.Imaging;

    /// <summary>
    /// View model for the scan results.
    /// </summary>
    internal class ScanResultsViewModel : ToolViewModel, ISnapshotObserver
    {
        /// <summary>
        /// The content id for the docking library associated with this view model.
        /// </summary>
        public const String ToolContentId = nameof(ScanResultsViewModel);

        /// <summary>
        /// The number of elements to display on each page.
        /// </summary>
        private const Int32 PageSize = 64;

        /// <summary>
        /// Singleton instance of the <see cref="ScanResultsViewModel" /> class.
        /// </summary>
        private static Lazy<ScanResultsViewModel> scanResultsViewModelInstance = new Lazy<ScanResultsViewModel>(
                () => { return new ScanResultsViewModel(); },
                LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// The active data type for the scan results.
        /// </summary>
        private Type activeType;

        /// <summary>
        /// The current page of scan results.
        /// </summary>
        private UInt64 currentPage;

        /// <summary>
        /// The total number of addresses.
        /// </summary>
        private UInt64 addressCount;

        /// <summary>
        /// The total number of bytes in memory.
        /// </summary>
        private UInt64 byteCount;

        /// <summary>
        /// The addresses on the current page.
        /// </summary>
        private ObservableCollection<ScanResult> addresses;

        /// <summary>
        /// The selected scan results.
        /// </summary>
        private IEnumerable<ScanResult> selectedScanResults;

        /// <summary>
        /// Prevents a default instance of the <see cref="ScanResultsViewModel" /> class from being created.
        /// </summary>
        private ScanResultsViewModel() : base("Scan Results")
        {
            this.ContentId = ScanResultsViewModel.ToolContentId;
            this.ChangeTypeCommand = new RelayCommand<Type>((type) => Task.Run(() => this.ChangeType(type)), (type) => true);
            this.SelectScanResultsCommand = new RelayCommand<Object>((selectedItems) => this.SelectedScanResults = (selectedItems as IList)?.Cast<ScanResult>(), (selectedItems) => true);
            this.FirstPageCommand = new RelayCommand(() => Task.Run(() => this.FirstPage()), () => true);
            this.LastPageCommand = new RelayCommand(() => Task.Run(() => this.LastPage()), () => true);
            this.PreviousPageCommand = new RelayCommand(() => Task.Run(() => this.PreviousPage()), () => true);
            this.NextPageCommand = new RelayCommand(() => Task.Run(() => this.NextPage()), () => true);
            this.AddScanResultCommand = new RelayCommand<ScanResult>((scanResult) => Task.Run(() => this.AddScanResult(scanResult)), (scanResult) => true);
            this.AddScanResultsCommand = new RelayCommand<Object>((selectedItems) => Task.Run(() => this.AddScanResults(this.SelectedScanResults)), (selectedItems) => true);
            this.ScanResultsObservers = new List<IScanResultsObserver>();
            this.ObserverLock = new Object();
            this.ActiveType = typeof(Int32);
            this.addresses = new ObservableCollection<ScanResult>();

            SnapshotManager.GetInstance().Subscribe(this);
            DockingViewModel.GetInstance().RegisterViewModel(this);

            this.UpdateScanResults();
        }

        /// <summary>
        /// Gets the command to change the active data type.
        /// </summary>
        public ICommand ChangeTypeCommand { get; private set; }

        /// <summary>
        /// Gets or sets the command to select scan results.
        /// </summary>
        public ICommand SelectScanResultsCommand { get; private set; }

        /// <summary>
        /// Gets the command to go to the first page.
        /// </summary>
        public ICommand FirstPageCommand { get; private set; }

        /// <summary>
        /// Gets the command to go to the last page.
        /// </summary>
        public ICommand LastPageCommand { get; private set; }

        /// <summary>
        /// Gets the command to go to the previous page.
        /// </summary>
        public ICommand PreviousPageCommand { get; private set; }

        /// <summary>
        /// Gets the command to go to the next page.
        /// </summary>
        public ICommand NextPageCommand { get; private set; }

        /// <summary>
        /// Gets the command to add a scan result to the project explorer.
        /// </summary>
        public ICommand AddScanResultCommand { get; private set; }

        /// <summary>
        /// Gets the command to add all selected scan results to the project explorer.
        /// </summary>
        public ICommand AddScanResultsCommand { get; private set; }

        /// <summary>
        /// Gets or sets the selected scan results.
        /// </summary>
        public IEnumerable<ScanResult> SelectedScanResults
        {
            get
            {
                return this.selectedScanResults;
            }

            set
            {
                this.selectedScanResults = value;
                PropertyViewerViewModel.GetInstance().SetTargetObjects(this.SelectedScanResults?.ToArray());
            }
        }

        /// <summary>
        /// Gets or sets the active scan results data type.
        /// </summary>
        public Type ActiveType
        {
            get
            {
                return this.activeType;
            }

            set
            {
                this.activeType = value;
                this.NotifyObservers();
                this.RaisePropertyChanged(nameof(this.ActiveType));
                this.RaisePropertyChanged(nameof(this.ActiveTypeName));
                this.RaisePropertyChanged(nameof(this.ActiveTypeImage));
            }
        }

        /// <summary>
        /// Gets the name associated with the active data type.
        /// </summary>
        public String ActiveTypeName
        {
            get
            {
                return Conversions.TypeToName(this.ActiveType);
            }
        }

        /// <summary>
        /// Gets the image associated with the active data type.
        /// </summary>
        public BitmapSource ActiveTypeImage
        {
            get
            {
                switch (Type.GetTypeCode(this.ActiveType))
                {
                    case TypeCode.SByte:
                        return Images.BlueBlocks1;
                    case TypeCode.Int16:
                        return Images.BlueBlocks2;
                    case TypeCode.Int32:
                        return Images.BlueBlocks4;
                    case TypeCode.Int64:
                        return Images.BlueBlocks8;
                    case TypeCode.Byte:
                        return Images.PurpleBlocks1;
                    case TypeCode.UInt16:
                        return Images.PurpleBlocks2;
                    case TypeCode.UInt32:
                        return Images.PurpleBlocks4;
                    case TypeCode.UInt64:
                        return Images.PurpleBlocks8;
                    case TypeCode.Single:
                        return Images.OrangeBlocks4;
                    case TypeCode.Double:
                        return Images.OrangeBlocks8;
                    default:
                        return null;
                }
            }
        }

        /// <summary>
        /// Gets or sets the total number of addresses found.
        /// </summary>
        public UInt64 CurrentPage
        {
            get
            {
                return this.currentPage;
            }

            set
            {
                this.currentPage = value;
                this.LoadScanResults();
                this.RaisePropertyChanged(nameof(this.CurrentPage));
            }
        }

        /// <summary>
        /// Gets the total number of addresses found.
        /// </summary>
        public UInt64 PageCount
        {
            get
            {
                return this.ResultCount == 0 ? 0 : (this.ResultCount - 1) / ScanResultsViewModel.PageSize;
            }
        }

        /// <summary>
        /// Gets or sets the total number of bytes found.
        /// </summary>
        public UInt64 ByteCount
        {
            get
            {
                return this.byteCount;
            }

            set
            {
                this.byteCount = value;
                this.RaisePropertyChanged(nameof(this.ByteCount));
            }
        }

        /// <summary>
        /// Gets or sets the total number of addresses found.
        /// </summary>
        public UInt64 ResultCount
        {
            get
            {
                return this.addressCount;
            }

            set
            {
                this.addressCount = value;
                this.RaisePropertyChanged(nameof(this.ResultCount));
                this.RaisePropertyChanged(nameof(this.PageCount));
            }
        }

        /// <summary>
        /// Gets the address elements.
        /// </summary>
        public ObservableCollection<ScanResult> Addresses
        {
            get
            {
                return this.addresses;
            }
        }

        /// <summary>
        /// Gets or sets the lock for accessing observers.
        /// </summary>
        private Object ObserverLock { get; set; }

        /// <summary>
        /// Gets or sets objects observing changes in the active snapshot.
        /// </summary>
        private List<IScanResultsObserver> ScanResultsObservers { get; set; }

        /// <summary>
        /// Gets a singleton instance of the <see cref="ScanResultsViewModel"/> class.
        /// </summary>
        /// <returns>A singleton instance of the class.</returns>
        public static ScanResultsViewModel GetInstance()
        {
            return ScanResultsViewModel.scanResultsViewModelInstance.Value;
        }

        /// <summary>
        /// Subscribes the given object to changes in the active snapshot.
        /// </summary>
        /// <param name="snapshotObserver">The object to observe active snapshot changes.</param>
        public void Subscribe(IScanResultsObserver snapshotObserver)
        {
            lock (this.ObserverLock)
            {
                if (!this.ScanResultsObservers.Contains(snapshotObserver))
                {
                    this.ScanResultsObservers.Add(snapshotObserver);
                    snapshotObserver.Update(this.ActiveType);
                }
            }
        }

        /// <summary>
        /// Unsubscribes the given object from changes in the active snapshot.
        /// </summary>
        /// <param name="snapshotObserver">The object to observe active snapshot changes.</param>
        public void Unsubscribe(IScanResultsObserver snapshotObserver)
        {
            lock (this.ObserverLock)
            {
                if (this.ScanResultsObservers.Contains(snapshotObserver))
                {
                    this.ScanResultsObservers.Remove(snapshotObserver);
                }
            }
        }

        /// <summary>
        /// Recieves an update of the active snapshot.
        /// </summary>
        /// <param name="snapshot">The active snapshot.</param>
        public void Update(Snapshot snapshot)
        {
            this.ResultCount = snapshot == null ? 0 : snapshot.ElementCount;
            this.ByteCount = snapshot == null ? 0 : snapshot.ByteCount;
            this.CurrentPage = 0;
        }

        /// <summary>
        /// Loads the results for the current page.
        /// </summary>
        private void LoadScanResults()
        {
            Snapshot snapshot = SnapshotManager.GetInstance().GetActiveSnapshot(createIfNone: false);
            ObservableCollection<ScanResult> newAddresses = new ObservableCollection<ScanResult>();

            if (snapshot == null)
            {
                this.addresses = newAddresses;
                this.RaisePropertyChanged(nameof(this.Addresses));
                return;
            }

            UInt64 startIndex = Math.Min(ScanResultsViewModel.PageSize * this.CurrentPage, snapshot.ElementCount);
            UInt64 endIndex = Math.Min((ScanResultsViewModel.PageSize * this.CurrentPage) + ScanResultsViewModel.PageSize, snapshot.ElementCount);

            for (UInt64 index = startIndex; index < endIndex; index++)
            {
                SnapshotElementIterator element = snapshot[index];
                String label = String.Empty;

                if (element.GetElementLabel() != null)
                {
                    label = element.GetElementLabel().ToString();
                }

                String currentValue = String.Empty;
                if (element.HasCurrentValue())
                {
                    currentValue = element.GetCurrentValue().ToString();
                }

                String previousValue = String.Empty;
                if (element.HasPreviousValue())
                {
                    previousValue = element.GetPreviousValue().ToString();
                }

                newAddresses.Add(new ScanResult(element.BaseAddress, currentValue, previousValue, label));
            }

            this.addresses = newAddresses;
            this.RaisePropertyChanged(nameof(this.Addresses));

            // Ensure results are visible
            this.IsVisible = true;
            this.IsSelected = true;
            this.IsActive = true;
        }

        /// <summary>
        /// Updates the values for the current scan results.
        /// </summary>
        private void UpdateScanResults()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    Boolean readSuccess;
                    this.Addresses.ForEach(x => x.ElementValue = EngineCore.GetInstance().VirtualMemory.Read(this.ActiveType, x.ElementAddress, out readSuccess).ToString());
                    this.RaisePropertyChanged(nameof(this.Addresses));

                    Thread.Sleep(SettingsViewModel.GetInstance().ResultReadInterval);
                }
            });
        }

        /// <summary>
        /// Changes the active scan results type.
        /// </summary>
        /// <param name="newType">The new scan results type.</param>
        private void ChangeType(Type newType)
        {
            this.ActiveType = newType;
        }

        /// <summary>
        /// Goes to the first page of results.
        /// </summary>
        private void FirstPage()
        {
            this.CurrentPage = 0;
        }

        /// <summary>
        /// Goes to the last page of results.
        /// </summary>
        private void LastPage()
        {
            this.CurrentPage = this.PageCount;
        }

        /// <summary>
        /// Goes to the previous page of results.
        /// </summary>
        private void PreviousPage()
        {
            this.CurrentPage = (this.CurrentPage - 1).Clamp(0UL, this.PageCount);
        }

        /// <summary>
        /// Goes to the next page of results.
        /// </summary>
        private void NextPage()
        {
            this.CurrentPage = (this.CurrentPage + 1).Clamp(0UL, this.PageCount);
        }

        /// <summary>
        /// Adds the given scan result to the project explorer.
        /// </summary>
        /// <param name="scanResult">The scan result to add to the project explorer.</param>
        private void AddScanResult(ScanResult scanResult)
        {
            ProjectExplorerViewModel.GetInstance().AddSpecificAddressItem(scanResult.ElementAddress, this.ActiveType);
        }

        /// <summary>
        /// Adds the given scan results to the project explorer.
        /// </summary>
        /// <param name="scanResults">The scan results to add to the project explorer.</param>
        private void AddScanResults(IEnumerable<ScanResult> scanResults)
        {
            if (scanResults == null)
            {
                return;
            }

            foreach (ScanResult scanResult in scanResults)
            {
                ProjectExplorerViewModel.GetInstance().AddSpecificAddressItem(scanResult.ElementAddress, this.ActiveType);
            }
        }

        /// <summary>
        /// Notify all observing objects of an active type change.
        /// </summary>
        private void NotifyObservers()
        {
            lock (this.ObserverLock)
            {
                foreach (IScanResultsObserver observer in this.ScanResultsObservers)
                {
                    observer.Update(this.ActiveType);
                }
            }
        }
    }
    //// End class
}
//// End namespace