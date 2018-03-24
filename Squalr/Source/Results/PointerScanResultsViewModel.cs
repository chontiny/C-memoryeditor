namespace Squalr.Source.Results
{
    using GalaSoft.MvvmLight.CommandWpf;
    using Squalr.Engine.DataTypes;
    using Squalr.Engine.Utils;
    using Squalr.Engine.Utils.DataStructures;
    using Squalr.Source.Docking;
    using Squalr.Source.ProjectExplorer;
    using Squalr.Source.ProjectItems;
    using Squalr.Source.Scanners.Pointers.Structures;
    using Squalr.Source.Utils.Extensions;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Input;

    /// <summary>
    /// View model for the pointer scan results.
    /// </summary>
    internal class PointerScanResultsViewModel : ToolViewModel
    {
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
        private FullyObservableCollection<PointerItem> pointers;

        /// <summary>
        /// The list of discovered pointers.
        /// </summary>
        private DiscoveredPointers discoveredPointers;

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
            this.ObserverLock = new Object();

            this.SelectScanResultsCommand = new RelayCommand<Object>((selectedItems) => this.SelectedScanResults = (selectedItems as IList)?.Cast<PointerItem>(), (selectedItems) => true);
            this.AddScanResultCommand = new RelayCommand<PointerItem>((scanResult) => Task.Run(() => this.AddScanResult(scanResult)), (scanResult) => true);
            this.AddScanResultsCommand = new RelayCommand<Object>((selectedItems) => Task.Run(() => this.AddScanResults(this.SelectedScanResults)), (selectedItems) => true);
            this.ChangeTypeCommand = new RelayCommand<DataType>((type) => Task.Run(() => this.ChangeType(type)), (type) => true);
            this.NewPointerScanCommand = new RelayCommand(() => Task.Run(() => this.DiscoveredPointers = null), () => true);
            this.FirstPageCommand = new RelayCommand(() => Task.Run(() => this.FirstPage()), () => true);
            this.LastPageCommand = new RelayCommand(() => Task.Run(() => this.LastPage()), () => true);
            this.PreviousPageCommand = new RelayCommand(() => Task.Run(() => this.PreviousPage()), () => true);
            this.NextPageCommand = new RelayCommand(() => Task.Run(() => this.NextPage()), () => true);
            this.AddAddressCommand = new RelayCommand<PointerItem>((address) => Task.Run(() => this.AddAddress(address)), (address) => true);

            this.ScanResultsObservers = new List<IResultDataTypeObserver>();
            this.ActiveType = DataType.Int32;
            this.pointers = new FullyObservableCollection<PointerItem>();

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
                this.RaisePropertyChanged(nameof(this.SelectedScanResults));
            }
        }

        /// <summary>
        /// Gets or sets the active scan results data type.
        /// </summary>
        public DataType ActiveType
        {
            get
            {
                return this.activeType;
            }

            set
            {
                this.activeType = value;

                // Update data type of pointers
                this.Pointers?.ToArray().ForEach(pointer => pointer.DataType = this.ActiveType);

                this.NotifyObservers();
                this.RaisePropertyChanged(nameof(this.ActiveType));
                this.RaisePropertyChanged(nameof(this.ActiveTypeName));
            }
        }

        /// <summary>
        /// Gets the name associated with the active data type.
        /// </summary>
        public String ActiveTypeName
        {
            get
            {
                return Conversions.DataTypeToName(this.ActiveType);
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
                this.RaisePropertyChanged(nameof(this.CanNavigateFirst));
                this.RaisePropertyChanged(nameof(this.CanNavigatePrevious));
                this.RaisePropertyChanged(nameof(this.CanNavigateNext));
                this.RaisePropertyChanged(nameof(this.CanNavigateLast));
            }
        }

        /// <summary>
        /// Gets a value indicating whether first page navigation is available.
        /// </summary>
        public Boolean CanNavigateFirst
        {
            get
            {
                return this.PageCount > 0 && this.CurrentPage > 0;
            }
        }

        /// <summary>
        /// Gets a value indicating whether next page navigation is available.
        /// </summary>
        public Boolean CanNavigateNext
        {
            get
            {
                return this.CurrentPage < this.PageCount;
            }
        }

        /// <summary>
        /// Gets a value indicating whether previous page navigation is available.
        /// </summary>
        public Boolean CanNavigatePrevious
        {
            get
            {
                return this.CurrentPage > 0;
            }
        }

        /// <summary>
        /// Gets a value indicating whether last page navigation is available.
        /// </summary>
        public Boolean CanNavigateLast
        {
            get
            {
                return this.PageCount > 0 && this.CurrentPage != this.PageCount;
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
        public FullyObservableCollection<PointerItem> Pointers
        {
            get
            {
                return this.pointers;
            }

            set
            {
                this.pointers = value;

                this.RaisePropertyChanged(nameof(this.Pointers));
            }
        }

        /// <summary>
        /// Gets or sets the list of discovered pointers.
        /// </summary>
        public DiscoveredPointers DiscoveredPointers
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

            ProjectExplorerViewModel.GetInstance().AddNewProjectItems(addToSelected: false, projectItems: scanResults);
        }

        /// <summary>
        /// Loads the results for the current page.
        /// </summary>
        private void LoadPointerScanResults()
        {
            UInt64 count = this.DiscoveredPointers == null ? 0 : this.DiscoveredPointers.Count;
            UInt64 startIndex = Math.Min(PointerScanResultsViewModel.PageSize * this.CurrentPage, count);
            UInt64 endIndex = Math.Min((PointerScanResultsViewModel.PageSize * this.CurrentPage) + PointerScanResultsViewModel.PageSize, count);

            if (this.DiscoveredPointers != null)
            {
                IEnumerable<PointerItem> newPointers = this.DiscoveredPointers.GetPointers(startIndex, endIndex);
                newPointers.ForEach(x => x.DataType = this.ActiveType);

                this.Pointers = new FullyObservableCollection<PointerItem>(newPointers);
            }
            else
            {
                this.Pointers = new FullyObservableCollection<PointerItem>();
            }

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
                    foreach (PointerItem pointer in this.Pointers.ToArray())
                    {
                        pointer.Update();
                    }

                    Thread.Sleep(PointerScanResultsViewModel.PointerReadIntervalMs);
                }
            });
        }

        /// <summary>
        /// Changes the active scan pointer results type.
        /// </summary>
        /// <param name="newType">The new pointer scan results type.</param>
        private void ChangeType(DataType newType)
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

        /// <summary>
        /// Notify all observing objects of an active type change.
        /// </summary>
        private void NotifyObservers()
        {
            lock (this.ObserverLock)
            {
                foreach (IResultDataTypeObserver observer in this.ScanResultsObservers)
                {
                    observer.Update(this.ActiveType);
                }
            }
        }
    }
    //// End class
}
//// End namespace