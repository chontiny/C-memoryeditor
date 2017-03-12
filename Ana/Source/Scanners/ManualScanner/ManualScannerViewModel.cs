namespace Ana.Source.Scanners.ManualScanner
{
    using Docking;
    using Main;
    using Mvvm.Command;
    using Results.ScanResults;
    using ScanConstraints;
    using System;
    using System.Collections.ObjectModel;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using System.Windows.Media.Imaging;

    /// <summary>
    /// View model for the Manual Scanner.
    /// </summary>
    internal class ManualScannerViewModel : ToolViewModel, IScanResultsObserver
    {
        /// <summary>
        /// The content id for the docking library associated with this view model.
        /// </summary>
        public const String ToolContentId = nameof(ManualScannerViewModel);

        /// <summary>
        /// Singleton instance of the <see cref="ManualScannerViewModel" /> class.
        /// </summary>
        private static Lazy<ManualScannerViewModel> manualScannerViewModelInstance = new Lazy<ManualScannerViewModel>(
                () => { return new ManualScannerViewModel(); },
                LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// The current scan constraint.
        /// </summary>
        private ScanConstraint currentScanConstraint;

        /// <summary>
        /// Manager for all active scan constraints.
        /// </summary>
        private ScanConstraintManager scanConstraintManager;

        /// <summary>
        /// Prevents a default instance of the <see cref="ManualScannerViewModel" /> class from being created.
        /// </summary>
        private ManualScannerViewModel() : base("Manual Scanner")
        {
            this.ContentId = ManualScannerViewModel.ToolContentId;
            this.StartScanCommand = new RelayCommand(() => Task.Run(() => this.StartScan()), () => true);

            // Note: Not async to avoid updates slower than the perception threshold
            this.UpdateActiveValueCommand = new RelayCommand<Object>((newValue) => this.UpdateActiveValue(newValue), (newValue) => true);
            this.SelectChangedCommand = new RelayCommand(() => this.ChangeScanConstraintSelection(ConstraintsEnum.Changed), () => true);
            this.SelectDecreasedCommand = new RelayCommand(() => this.ChangeScanConstraintSelection(ConstraintsEnum.Decreased), () => true);
            this.SelectDecreasedByXCommand = new RelayCommand(() => this.ChangeScanConstraintSelection(ConstraintsEnum.DecreasedByX), () => true);
            this.SelectEqualCommand = new RelayCommand(() => this.ChangeScanConstraintSelection(ConstraintsEnum.Equal), () => true);
            this.SelectGreaterThanCommand = new RelayCommand(() => this.ChangeScanConstraintSelection(ConstraintsEnum.GreaterThan), () => true);
            this.SelectGreaterThanOrEqualCommand = new RelayCommand(() => this.ChangeScanConstraintSelection(ConstraintsEnum.GreaterThanOrEqual), () => true);
            this.SelectIncreasedCommand = new RelayCommand(() => this.ChangeScanConstraintSelection(ConstraintsEnum.Increased), () => true);
            this.SelectIncreasedByXCommand = new RelayCommand(() => this.ChangeScanConstraintSelection(ConstraintsEnum.IncreasedByX), () => true);
            this.SelectLessThanCommand = new RelayCommand(() => this.ChangeScanConstraintSelection(ConstraintsEnum.LessThan), () => true);
            this.SelectLessThanOrEqualCommand = new RelayCommand(() => this.ChangeScanConstraintSelection(ConstraintsEnum.LessThanOrEqual), () => true);
            this.SelectNotEqualCommand = new RelayCommand(() => this.ChangeScanConstraintSelection(ConstraintsEnum.NotEqual), () => true);
            this.SelectNotScientificNotationCommand = new RelayCommand(() => this.ChangeScanConstraintSelection(ConstraintsEnum.NotScientificNotation), () => true);
            this.SelectUnchangedCommand = new RelayCommand(() => this.ChangeScanConstraintSelection(ConstraintsEnum.Unchanged), () => true);

            // Note: Constraint modifying commands cannot be async since they modify the observable collection, which must be done on the same thread as the GUI
            this.AddCurrentConstraintCommand = new RelayCommand(() => this.AddCurrentConstraint(), () => true);
            this.RemoveConstraintCommand = new RelayCommand<ScanConstraint>((ScanConstraint) => this.RemoveConstraint(ScanConstraint), (ScanConstraint) => true);
            this.EditConstraintCommand = new RelayCommand<ScanConstraint>((ScanConstraint) => this.EditConstraint(ScanConstraint), (ScanConstraint) => true);
            this.ClearConstraintsCommand = new RelayCommand(() => this.ClearConstraints(), () => true);
            this.CurrentScanConstraint = new ScanConstraint(ConstraintsEnum.Equal);
            this.ManualScannerModel = new ManualScannerModel();
            this.ScanConstraintManager = new ScanConstraintManager();
            this.ScanConstraintManager.SetElementType(typeof(Int32));

            Task.Run(() => ScanResultsViewModel.GetInstance().Subscribe(this));
            Task.Run(() => MainViewModel.GetInstance().Subscribe(this));
        }

        /// <summary>
        /// Gets the command begin the scan.
        /// </summary>
        public ICommand StartScanCommand { get; private set; }

        /// <summary>
        /// Gets the command to update the value of the active scan constraint.
        /// </summary>
        public ICommand UpdateActiveValueCommand { get; private set; }

        /// <summary>
        /// Gets the command to add the current constraint to the list of scan constraints.
        /// </summary>
        public ICommand AddCurrentConstraintCommand { get; private set; }

        /// <summary>
        /// Gets the command to remove the target constraint to the list of scan constraints.
        /// </summary>
        public ICommand RemoveConstraintCommand { get; private set; }

        /// <summary>
        /// Gets the command to edit the target constraint.
        /// </summary>
        public ICommand EditConstraintCommand { get; private set; }

        /// <summary>
        /// Gets the command to clear all added constraints.
        /// </summary>
        public ICommand ClearConstraintsCommand { get; private set; }

        /// <summary>
        /// Gets the command to select the <see cref="ConstraintsEnum.Changed"/> constraint.
        /// </summary>
        public ICommand SelectChangedCommand { get; private set; }

        /// <summary>
        /// Gets the command to select the <see cref="ConstraintsEnum.Decreased"/> constraint.
        /// </summary>
        public ICommand SelectDecreasedCommand { get; private set; }

        /// <summary>
        /// Gets the command to select the <see cref="ConstraintsEnum.DecreasedByX"/> constraint.
        /// </summary>
        public ICommand SelectDecreasedByXCommand { get; private set; }

        /// <summary>
        /// Gets the command to select the <see cref="ConstraintsEnum.Equal"/> constraint.
        /// </summary>
        public ICommand SelectEqualCommand { get; private set; }

        /// <summary>
        /// Gets the command to select the <see cref="ConstraintsEnum.GreaterThan"/> constraint.
        /// </summary>
        public ICommand SelectGreaterThanCommand { get; private set; }

        /// <summary>
        /// Gets the command to select the <see cref="ConstraintsEnum.GreaterThanOrEqual"/> constraint.
        /// </summary>
        public ICommand SelectGreaterThanOrEqualCommand { get; private set; }

        /// <summary>
        /// Gets the command to select the <see cref="ConstraintsEnum.Increased"/> constraint.
        /// </summary>
        public ICommand SelectIncreasedCommand { get; private set; }

        /// <summary>
        /// Gets the command to select the <see cref="ConstraintsEnum.IncreasedByX"/> constraint.
        /// </summary>
        public ICommand SelectIncreasedByXCommand { get; private set; }

        /// <summary>
        /// Gets the command to select the <see cref="ConstraintsEnum.LessThan"/> constraint.
        /// </summary>
        public ICommand SelectLessThanCommand { get; private set; }

        /// <summary>
        /// Gets the command to select the <see cref="ConstraintsEnum.LessThanOrEqual"/> constraint.
        /// </summary>
        public ICommand SelectLessThanOrEqualCommand { get; private set; }

        /// <summary>
        /// Gets the command to select the <see cref="ConstraintsEnum.NotEqual"/> constraint.
        /// </summary>
        public ICommand SelectNotEqualCommand { get; private set; }

        /// <summary>
        /// Gets the command to select the <see cref="ConstraintsEnum.NotScientificNotation"/> constraint.
        /// </summary>
        public ICommand SelectNotScientificNotationCommand { get; private set; }

        /// <summary>
        /// Gets the command to select the <see cref="ConstraintsEnum.Unchanged"/> constraint.
        /// </summary>
        public ICommand SelectUnchangedCommand { get; private set; }

        /// <summary>
        /// Gets the current set of scan constraints added to the manager.
        /// </summary>
        public ObservableCollection<ScanConstraint> Constraints
        {
            get
            {
                return this.ScanConstraintManager.ValueConstraints;
            }
        }

        /// <summary>
        /// Gets or sets the scan constraint manager.
        /// </summary>
        public ScanConstraintManager ScanConstraintManager
        {
            get
            {
                return this.scanConstraintManager;
            }

            set
            {
                this.scanConstraintManager = value;
                this.RaisePropertyChanged(nameof(this.ScanConstraintManager));
            }
        }

        /// <summary>
        /// Gets the current scan constraint, wrapped as a collection for easier display.
        /// </summary>
        public ObservableCollection<ScanConstraint> ActiveScanConstraint
        {
            get
            {
                return new ObservableCollection<ScanConstraint>(new ScanConstraint[] { this.CurrentScanConstraint });
            }
        }

        /// <summary>
        /// Gets or sets the current scan constraint.
        /// </summary>
        public ScanConstraint CurrentScanConstraint
        {
            get
            {
                return this.currentScanConstraint;
            }

            set
            {
                this.currentScanConstraint = value;
                this.UpdateAllProperties();
            }
        }

        /// <summary>
        /// Gets a value indicating whether or not no constraints have been added.
        /// </summary>
        public Boolean HasNoConstraints
        {
            get
            {
                return this.ScanConstraintManager.ValueConstraints.Count <= 0;
            }
        }

        /// <summary>
        /// Gets the image associated with the current scan constraint.
        /// </summary>
        public BitmapSource ScanConstraintImage
        {
            get
            {
                return this.CurrentScanConstraint.ConstraintImage;
            }
        }

        /// <summary>
        /// Gets or sets the manual scanner model.
        /// </summary>
        private ManualScannerModel ManualScannerModel { get; set; }

        /// <summary>
        /// Gets a singleton instance of the <see cref="ManualScannerViewModel"/> class.
        /// </summary>
        /// <returns>A singleton instance of the class.</returns>
        public static ManualScannerViewModel GetInstance()
        {
            return ManualScannerViewModel.manualScannerViewModelInstance.Value;
        }

        /// <summary>
        /// Updates the active type.
        /// </summary>
        /// <param name="activeType">The new active type.</param>
        public void Update(Type activeType)
        {
            // Create a temporary manager to update our current constraint
            ScanConstraintManager manager = new ScanConstraintManager();
            manager.AddConstraint(this.CurrentScanConstraint);
            manager.SetElementType(activeType);

            this.ScanConstraintManager.SetElementType(activeType);
            this.UpdateAllProperties();
        }

        /// <summary>
        /// Starts the scan using the current constraints.
        /// </summary>
        private void StartScan()
        {
            // Create a constraint manager that includes the current active constraint
            ScanConstraintManager allScanConstraints = this.ScanConstraintManager.Clone();
            allScanConstraints.AddConstraint(this.CurrentScanConstraint);

            ManualScannerModel.SetScanConstraintManager(allScanConstraints);
            ManualScannerModel.Begin();
        }

        /// <summary>
        /// Adds the current constraint to the list of scan constraints.
        /// </summary>
        private void AddCurrentConstraint()
        {
            this.ScanConstraintManager.AddConstraint(this.CurrentScanConstraint);
            this.CurrentScanConstraint = new ScanConstraint(this.CurrentScanConstraint.Constraint, this.CurrentScanConstraint.ConstraintValue);
            this.UpdateAllProperties();
        }

        /// <summary>
        /// Updates the value of the current scan constraint.
        /// </summary>
        /// <param name="newValue">The new value of the scan constraint.</param>
        private void UpdateActiveValue(Object newValue)
        {
            this.CurrentScanConstraint.ConstraintValue = newValue;
            this.UpdateAllProperties();
        }

        /// <summary>
        /// Edits the target constraint by removing it and making it the current constraint.
        /// </summary>
        /// <param name="scanConstraint">The constraint to edit.</param>
        private void EditConstraint(ScanConstraint scanConstraint)
        {
            this.ScanConstraintManager.RemoveConstraints(scanConstraint);
            this.CurrentScanConstraint = scanConstraint;
            this.UpdateAllProperties();
        }

        /// <summary>
        /// Removes the target constraint from the list of scan constraints.
        /// </summary>
        /// <param name="scanConstraint">The constraint to remove.</param>
        private void RemoveConstraint(ScanConstraint scanConstraint)
        {
            this.ScanConstraintManager.RemoveConstraints(scanConstraint);
            this.UpdateAllProperties();
        }

        /// <summary>
        /// Clears all scan constraints.
        /// </summary>
        private void ClearConstraints()
        {
            this.ScanConstraintManager.ClearConstraints();
            this.UpdateAllProperties();
        }

        /// <summary>
        /// Changes the current scan constraint.
        /// </summary>
        /// <param name="constraint">The new scan constraint.</param>
        private void ChangeScanConstraintSelection(ConstraintsEnum constraint)
        {
            this.CurrentScanConstraint.Constraint = constraint;
            this.UpdateAllProperties();
        }

        /// <summary>
        /// Raises property changed events for all used properties. This is convenient since there are several interdependencies between these.
        /// </summary>
        private void UpdateAllProperties()
        {
            this.RaisePropertyChanged(nameof(this.CurrentScanConstraint));
            this.RaisePropertyChanged(nameof(this.ScanConstraintImage));
            this.RaisePropertyChanged(nameof(this.ActiveScanConstraint));
            this.RaisePropertyChanged(nameof(this.HasNoConstraints));
        }
    }
    //// End class
}
//// End namespace