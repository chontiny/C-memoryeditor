namespace Squalr.View
{
    using Source.DotNetExplorer;
    using Source.Results.PointerScanResults;
    using Source.Results.ScanResults;
    using Source.Scanners.ChangeCounter;
    using Source.Scanners.InputCorrelator;
    using Source.Scanners.LabelThresholder;
    using Source.Scanners.ManualScanner;
    using Source.Scanners.PointerScanner;
    using Source.Snapshots;
    using System.Windows;

    /// <summary>
    /// Provides the template required to view a pane.
    /// </summary>
    internal class ViewTemplateSelector : SqualrCore.View.ViewTemplateSelector
    {
        /// <summary>
        /// The template for the Change Counter.
        /// </summary>
        public DataTemplate changeCounterViewTemplate;

        /// <summary>
        /// The template for the Input Correlator.
        /// </summary>
        public DataTemplate inputCorrelatorViewTemplate;

        /// <summary>
        /// The template for the Label Thresholder.
        /// </summary>
        public DataTemplate labelThresholderViewTemplate;

        /// <summary>
        /// The template for the Manual Scanner.
        /// </summary>
        public DataTemplate manualScannerViewTemplate;

        /// <summary>
        /// The template for the Pointer Scanner.
        /// </summary>
        public DataTemplate pointerScannerViewTemplate;

        /// <summary>
        /// The template for the Snapshot Manager.
        /// </summary>
        public DataTemplate snapshotManagerViewTemplate;

        /// <summary>
        /// The template for the Scan Results.
        /// </summary>
        public DataTemplate scanResultsViewTemplate;

        /// <summary>
        /// The template for the Pointer Scan Results.
        /// </summary>
        public DataTemplate pointerScanResultsViewTemplate;

        /// <summary>
        /// The template for the .Net Explorer.
        /// </summary>
        public DataTemplate dotNetExplorerViewTemplate;

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewTemplateSelector" /> class.
        /// </summary>
        public ViewTemplateSelector()
        {
        }

        /// <summary>
        /// Gets or sets the template for the Change Counter.
        /// </summary>
        public DataTemplate ChangeCounterViewTemplate
        {
            get
            {
                return this.changeCounterViewTemplate;
            }

            set
            {
                this.changeCounterViewTemplate = value;
                this.DataTemplates[typeof(ChangeCounterViewModel)] = value;
            }
        }

        /// <summary>
        /// Gets or sets the template for the Input Correlator.
        /// </summary>
        public DataTemplate InputCorrelatorViewTemplate
        {
            get
            {
                return this.inputCorrelatorViewTemplate;
            }

            set
            {
                this.inputCorrelatorViewTemplate = value;
                this.DataTemplates[typeof(InputCorrelatorViewModel)] = value;
            }
        }

        /// <summary>
        /// Gets or sets the template for the Label Thresholder.
        /// </summary>
        public DataTemplate LabelThresholderViewTemplate
        {
            get
            {
                return this.labelThresholderViewTemplate;
            }

            set
            {
                this.labelThresholderViewTemplate = value;
                this.DataTemplates[typeof(LabelThresholderViewModel)] = value;
            }
        }

        /// <summary>
        /// Gets or sets the template for the Manual Scanner.
        /// </summary>
        public DataTemplate ManualScannerViewTemplate
        {
            get
            {
                return this.manualScannerViewTemplate;
            }

            set
            {
                this.manualScannerViewTemplate = value;
                this.DataTemplates[typeof(ManualScannerViewModel)] = value;
            }
        }

        /// <summary>
        /// Gets or sets the template for the Pointer Scanner.
        /// </summary>
        public DataTemplate PointerScannerViewTemplate
        {
            get
            {
                return this.pointerScannerViewTemplate;
            }

            set
            {
                this.pointerScannerViewTemplate = value;
                this.DataTemplates[typeof(PointerScannerViewModel)] = value;
            }
        }

        /// <summary>
        /// Gets or sets the template for the Snapshot Manager.
        /// </summary>
        public DataTemplate SnapshotManagerViewTemplate
        {
            get
            {
                return this.snapshotManagerViewTemplate;
            }

            set
            {
                this.snapshotManagerViewTemplate = value;
                this.DataTemplates[typeof(SnapshotManagerViewModel)] = value;
            }
        }

        /// <summary>
        /// Gets or sets the template for the Scan Results.
        /// </summary>
        public DataTemplate ScanResultsViewTemplate
        {
            get
            {
                return this.scanResultsViewTemplate;
            }

            set
            {
                this.scanResultsViewTemplate = value;
                this.DataTemplates[typeof(ScanResultsViewModel)] = value;
            }
        }

        /// <summary>
        /// Gets or sets the template for the Pointer Scan Results.
        /// </summary>
        public DataTemplate PointerScanResultsViewTemplate
        {
            get
            {
                return this.pointerScanResultsViewTemplate;
            }

            set
            {
                this.pointerScanResultsViewTemplate = value;
                this.DataTemplates[typeof(PointerScanResultsViewModel)] = value;
            }
        }

        /// <summary>
        /// Gets or sets the template for the .Net Explorer.
        /// </summary>
        public DataTemplate DotNetExplorerViewTemplate
        {
            get
            {
                return this.dotNetExplorerViewTemplate;
            }

            set
            {
                this.dotNetExplorerViewTemplate = value;
                this.DataTemplates[typeof(DotNetExplorerViewModel)] = value;
            }
        }
    }
    //// End class
}
//// End namespace