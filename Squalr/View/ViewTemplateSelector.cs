namespace Squalr.View
{
    using Source.DotNetExplorer;
    using Source.Results;
    using Source.Scanners.ChangeCounter;
    using Source.Scanners.InputCorrelator;
    using Source.Scanners.LabelThresholder;
    using Source.Scanners.ManualScanner;
    using Source.Scanners.Pointers;
    using Source.Snapshots;
    using Squalr.Properties;
    using Squalr.Source.Debugger;
    using Squalr.Source.Debugger.Disassembly;
    using Squalr.Source.ProjectExplorer;
    using System.Windows;

    /// <summary>
    /// Provides the template required to view a pane.
    /// </summary>
    internal class ViewTemplateSelector : SqualrCore.View.ViewTemplateSelector
    {
        /// <summary>
        /// The template for the Change Counter.
        /// </summary>
        private DataTemplate changeCounterViewTemplate;

        /// <summary>
        /// The template for the Input Correlator.
        /// </summary>
        private DataTemplate inputCorrelatorViewTemplate;

        /// <summary>
        /// The template for the Label Thresholder.
        /// </summary>
        private DataTemplate labelThresholderViewTemplate;

        /// <summary>
        /// The template for the Manual Scanner.
        /// </summary>
        private DataTemplate manualScannerViewTemplate;

        /// <summary>
        /// The template for the Pointer Scanner.
        /// </summary>
        private DataTemplate pointerScannerViewTemplate;

        /// <summary>
        /// The template for the Snapshot Manager.
        /// </summary>
        private DataTemplate snapshotManagerViewTemplate;

        /// <summary>
        /// The template for the Scan Results.
        /// </summary>
        private DataTemplate scanResultsViewTemplate;

        /// <summary>
        /// The template for the Pointer Scan Results.
        /// </summary>
        private DataTemplate pointerScanResultsViewTemplate;

        /// <summary>
        /// The template for the .Net Explorer.
        /// </summary>
        private DataTemplate dotNetExplorerViewTemplate;

        /// <summary>
        /// The template for the Project Explorer.
        /// </summary>
        private DataTemplate projectExplorerViewTemplate;

        /// <summary>
        /// The template for the Settings.
        /// </summary>
        private DataTemplate settingsViewTemplate;

        /// <summary>
        /// The template for the Debugger.
        /// </summary>
        private DataTemplate debuggerViewTemplate;

        /// <summary>
        /// The template for the Disassembly.
        /// </summary>
        private DataTemplate disassemblyViewTemplate;

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewTemplateSelector" /> class.
        /// </summary>
        public ViewTemplateSelector()
        {
        }

        /// <summary>
        /// Gets or sets the template for the Settings.
        /// </summary>
        public DataTemplate SettingsViewTemplate
        {
            get
            {
                return this.settingsViewTemplate;
            }

            set
            {
                this.settingsViewTemplate = value;
                this.DataTemplates[typeof(SettingsViewModel)] = value;
            }
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

        /// <summary>
        /// Gets or sets the template for the Project Explorer.
        /// </summary>
        public DataTemplate ProjectExplorerViewTemplate
        {
            get
            {
                return this.projectExplorerViewTemplate;
            }

            set
            {
                this.projectExplorerViewTemplate = value;
                this.DataTemplates[typeof(ProjectExplorerViewModel)] = value;
            }
        }

        /// <summary>
        /// Gets or sets the template for the Debugger.
        /// </summary>
        public DataTemplate DebuggerViewTemplate
        {
            get
            {
                return this.debuggerViewTemplate;
            }

            set
            {
                this.debuggerViewTemplate = value;
                this.DataTemplates[typeof(DebuggerViewModel)] = value;
            }
        }

        /// <summary>
        /// Gets or sets the template for the Disassembly.
        /// </summary>
        public DataTemplate DisassemblyViewTemplate
        {
            get
            {
                return this.disassemblyViewTemplate;
            }

            set
            {
                this.disassemblyViewTemplate = value;
                this.DataTemplates[typeof(DisassemblyViewModel)] = value;
            }
        }
    }
    //// End class
}
//// End namespace