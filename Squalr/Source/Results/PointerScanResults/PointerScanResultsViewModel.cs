namespace Squalr.Source.Results.PointerScanResults
{
    using GalaSoft.MvvmLight.CommandWpf;
    using Squalr.Source.ProjectExplorer;
    using Squalr.Source.Scanners.Pointers.Structures;
    using SqualrCore.Content;
    using SqualrCore.Source.Docking;
    using SqualrCore.Source.ProjectItems;
    using SqualrCore.Source.Utils;
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
    /// View model for the pointer scan results.
    /// </summary>
    internal class PointerScanResultsViewModel : ToolViewModel
    {
        /// <summary>
        /// The content id for the docking library associated with this view model.
        /// </summary>
        public const String ToolContentId = nameof(PointerScanResultsViewModel);

        /// <summary>
        /// The number of elements to display on each page.
        /// </summary>
        private const Int32 PageSize = 64;

        /// <summary>
        /// Singleton instance of the <see cref="PointerScanResultsViewModel" /> class.
        /// </summary>
        private static Lazy<PointerScanResultsViewModel> pointerScanResultsViewModelInstance = new Lazy<PointerScanResultsViewModel>(
                () => { return new PointerScanResultsViewModel(); },
                LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// The result display type.
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
        /// The addresses on the current page.
        /// </summary>
        private ObservableCollection<PointerItem> addresses;

        /// <summary>
        /// The list of discovered pointers.
        /// </summary>
        private IDiscoveredPointers discoveredPointers;

        /// <summary>
        /// The pointer read interval in milliseconds
        /// </summary>
        private const Int32 PointerReadIntervalMs = 1600;

        /// <summary>
        /// The selected scan results.
        /// </summary>
        private IEnumerable<PointerItem> selectedScanResults;

        /// <summary>
        /// Prevents a default instance of the <see cref="PointerScanResultsViewModel" /> class from being created.
        /// </summary>
        private PointerScanResultsViewModel() : base("Pointer Scan Results")
        {
            this.ContentId = PointerScanResultsViewModel.ToolContentId;

            this.SelectScanResultsCommand = new RelayCommand<Object>((selectedItems) => this.SelectedScanResults = (selectedItems as IList)?.Cast<PointerItem>(), (selectedItems) => true);
            this.AddScanResultCommand = new RelayCommand<PointerItem>((scanResult) => Task.Run(() => this.AddScanResult(scanResult)), (scanResult) => true);
            this.AddScanResultsCommand = new RelayCommand<Object>((selectedItems) => Task.Run(() => this.AddScanResults(this.SelectedScanResults)), (selectedItems) => true);
            this.ChangeTypeCommand = new RelayCommand<Type>((type) => Task.Run(() => this.ChangeType(type)), (type) => true);
            this.NewPointerScanCommand = new RelayCommand(() => Task.Run(() => this.DiscoveredPointers = null), () => true);
            this.FirstPageCommand = new RelayCommand(() => Task.Run(() => this.FirstPage()), () => true);
            this.LastPageCommand = new RelayCommand(() => Task.Run(() => this.LastPage()), () => true);
            this.PreviousPageCommand = new RelayCommand(() => Task.Run(() => this.PreviousPage()), () => true);
            this.NextPageCommand = new RelayCommand(() => Task.Run(() => this.NextPage()), () => true);
            this.AddAddressCommand = new RelayCommand<PointerItem>((address) => Task.Run(() => this.AddAddress(address)), (address) => true);

            this.ActiveType = typeof(Int32);
            this.addresses = new ObservableCollection<PointerItem>();

            DockingViewModel.GetInstance().RegisterViewModel(this);

            this.UpdateScanResults();
        }

        /// <summary>
        /// Gets or sets the command to select scan results.
        /// </summary>
        public ICommand SelectScanResultsCommand { get; private set; }

        /// <summary>
        /// Gets the command to add a scan result to the project explorer.
        /// </summary>
        public ICommand AddScanResultCommand { get; private set; }

        /// <summary>
        /// Gets the command to add all selected scan results to the project explorer.
        /// </summary>
        public ICommand AddScanResultsCommand { get; private set; }

        /// <summary>
        /// Gets the command to change the active data type.
        /// </summary>
        public ICommand ChangeTypeCommand { get; private set; }

        /// <summary>
        /// Gets a command to clear the pointer scan results.
        /// </summary>
        public ICommand NewPointerScanCommand { get; private set; }

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
        /// Gets the command to select a target process.
        /// </summary>
        public ICommand AddAddressCommand { get; private set; }

        /// <summary>
        /// Gets or sets the selected scan results.
        /// </summary>
        public IEnumerable<PointerItem> SelectedScanResults
        {
            get
            {
                return this.selectedScanResults;
            }

            set
            {
                this.selectedScanResults = value;
            }
        }

        public Type ActiveType
        {
            get
            {
                return this.activeType;
            }

            set
            {
                this.activeType = value;
                this.RaisePropertyChanged(nameof(this.ActiveType));
                this.RaisePropertyChanged(nameof(this.ActiveTypeName));
                this.RaisePropertyChanged(nameof(this.ActiveTypeImage));
            }
        }

        public String ActiveTypeName
        {
            get
            {
                return Conversions.TypeToName(this.ActiveType);
            }
        }

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
                this.LoadPointerScanResults();
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
                return this.ResultCount / PointerScanResultsViewModel.PageSize;
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
        /// Gets or sets the address elements.
        /// </summary>
        public ObservableCollection<PointerItem> Addresses
        {
            get
            {
                return this.addresses;
            }

            set
            {
                this.addresses = value;

                this.RaisePropertyChanged(nameof(this.Addresses));
            }
        }

        /// <summary>
        /// Gets or sets the list of discovered pointers.
        /// </summary>
        public IDiscoveredPointers DiscoveredPointers
        {
            get
            {
                return this.discoveredPointers;
            }

            set
            {
                this.discoveredPointers = value;

                this.ResultCount = discoveredPointers == null ? 0 : discoveredPointers.Count;
                this.CurrentPage = 0;

                this.RaisePropertyChanged(nameof(this.DiscoveredPointers));
            }
        }

        /// <summary>
        /// Gets or sets the lock for accessing observers.
        /// </summary>
        private Object ObserverLock { get; set; }

        /// <summary>
        /// Gets or sets objects observing changes in the pointer scan results data type.
        /// </summary>
        private List<IResultDataTypeObserver> ScanResultsObservers { get; set; }

        /// <summary>
        /// Subscribes the given object to changes in the pointer scan results data type.
        /// </summary>
        /// <param name="snapshotObserver">The object to observe pointer scan results data type changes.</param>
        public void Subscribe(IResultDataTypeObserver snapshotObserver)
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
        /// Unsubscribes the given object from changes in the pointer scan results data type.
        /// </summary>
        /// <param name="snapshotObserver">The object to observe pointer scan results data type changes.</param>
        public void Unsubscribe(IResultDataTypeObserver snapshotObserver)
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
        /// Gets a singleton instance of the <see cref="PointerScanResultsViewModel"/> class.
        /// </summary>
        /// <returns>A singleton instance of the class.</returns>
        public static PointerScanResultsViewModel GetInstance()
        {
            return PointerScanResultsViewModel.pointerScanResultsViewModelInstance.Value;
        }

        /// <summary>
        /// Adds the given scan result to the project explorer.
        /// </summary>
        /// <param name="scanResult">The scan result to add to the project explorer.</param>
        private void AddScanResult(PointerItem scanResult)
        {
            ProjectExplorerViewModel.GetInstance().AddNewProjectItems(addToSelected: false, projectItems: scanResult);
        }

        /// <summary>
        /// Adds the given scan results to the project explorer.
        /// </summary>
        /// <param name="scanResults">The scan results to add to the project explorer.</param>
        private void AddScanResults(IEnumerable<PointerItem> scanResults)
        {
            if (scanResults == null)
            {
                return;
            }

            ProjectExplorerViewModel.GetInstance().AddNewProjectItems(addToSelected: false, projectItems: scanResults.ToArray());
        }

        /// <summary>
        /// Loads the results for the current page.
        /// </summary>
        private void LoadPointerScanResults()
        {
            IList<PointerItem> newAddresses = new List<PointerItem>();

            UInt64 count = this.DiscoveredPointers == null ? 0 : this.DiscoveredPointers.Count;
            UInt64 startIndex = Math.Min(PointerScanResultsViewModel.PageSize * this.CurrentPage, count);
            UInt64 endIndex = Math.Min((PointerScanResultsViewModel.PageSize * this.CurrentPage) + PointerScanResultsViewModel.PageSize, count);

            for (UInt64 index = startIndex; index < endIndex; index++)
            {
                PointerItem pointerItem = this.DiscoveredPointers[index];

                if (pointerItem != null)
                {
                    newAddresses.Add(pointerItem);
                }
            }

            this.Addresses = new ObservableCollection<PointerItem>(newAddresses);

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
                    Boolean hasUpdate = false;

                    foreach (PointerItem pointer in this.Addresses.ToArray())
                    {
                        hasUpdate |= pointer.Update();
                    }

                    // This is a sidestep to a particular issue where we need to potentially perform RaisePropertyChanged for a {Binding Path=.} element, which is impossible.
                    // We recreate the entire collection to force a re-render.
                    if (hasUpdate)
                    {
                        this.Addresses = new ObservableCollection<PointerItem>(this.Addresses);
                    }

                    Thread.Sleep(PointerScanResultsViewModel.PointerReadIntervalMs);
                }
            });
        }

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
            this.CurrentPage = this.CurrentPage == 0 ? this.CurrentPage : this.CurrentPage - 1;
        }

        /// <summary>
        /// Goes to the next page of results.
        /// </summary>
        private void NextPage()
        {
            this.CurrentPage = this.CurrentPage >= this.PageCount ? this.CurrentPage : this.CurrentPage + 1;
        }

        /// <summary>
        /// Adds the given scan result address to the project explorer.
        /// </summary>
        /// <param name="scanResult">The scan result to add to the project explorer.</param>
        private void AddAddress(PointerItem scanResult)
        {
            ProjectExplorerViewModel.GetInstance().AddNewProjectItems(addToSelected: false, projectItems: scanResult);
        }


        private void AddSelectionToTable()
        {
            /*
            if (minIndex < 0)
            {
                minIndex = 0;
            }

            if (maxIndex > this.AcceptedPointers.Count)
            {
                maxIndex = this.AcceptedPointers.Count;
            }

            Int32 count = 0;

            for (Int32 index = minIndex; index <= maxIndex; index++)
            {
                String pointerValue = String.Empty;
                this.IndexValueMap.TryGetValue(index, out pointerValue);

                PointerItem newPointer = new PointerItem(
                    baseAddress: this.AcceptedPointers[index].Item1,
                    elementType: this.ElementType,
                    description: "New Pointer",
                    moduleName: String.Empty,
                    pointerOffsets: this.AcceptedPointers[index].Item2,
                    isValueHex: false,
                    value: pointerValue
                );

                ProjectExplorerViewModel.GetInstance().AddNewProjectItems(addToSelected: true, projectItems: newPointer);

                if (++count >= PointerScanner.MaxAdd)
                {
                    break;
                }
            }*/
        }
    }
    //// End class
}
//// End namespace