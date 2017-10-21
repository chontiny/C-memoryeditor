namespace Squalr.View
{
    using Source.DotNetExplorer;
    using Source.ProjectExplorer;
    using Source.Results.PointerScanResults;
    using Source.Results.ScanResults;
    using Source.Scanners.ChangeCounter;
    using Source.Scanners.InputCorrelator;
    using Source.Scanners.LabelThresholder;
    using Source.Scanners.ManualScanner;
    using Source.Scanners.PointerScanner;
    using Source.Snapshots;
    using Squalr.Properties;
    using Squalr.Source.Browse;
    using Squalr.Source.Browse.StreamConfig;
    using Squalr.Source.Browse.TwitchLogin;
    using System;
    using System.Windows;

    /// <summary>
    /// Provides the template required to view a pane.
    /// </summary>
    internal class ViewTemplateSelector : SqualrCore.View.ViewTemplateSelector
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ViewTemplateSelector" /> class.
        /// </summary>
        public ViewTemplateSelector()
        {
        }

        /// <summary>
        /// Gets or sets the template for the Change Counter.
        /// </summary>
        public DataTemplate ChangeCounterViewTemplate { get; set; }

        /// <summary>
        /// Gets or sets the template for the Input Correlator.
        /// </summary>
        public DataTemplate InputCorrelatorViewTemplate { get; set; }

        /// <summary>
        /// Gets or sets the template for the Label Thresholder.
        /// </summary>
        public DataTemplate LabelThresholderViewTemplate { get; set; }

        /// <summary>
        /// Gets or sets the template for the Manual Scanner.
        /// </summary>
        public DataTemplate ManualScannerViewTemplate { get; set; }

        /// <summary>
        /// Gets or sets the template for the Pointer Scanner.
        /// </summary>
        public DataTemplate PointerScannerViewTemplate { get; set; }

        /// <summary>
        /// Gets or sets the template for the Snapshot Manager.
        /// </summary>
        public DataTemplate SnapshotManagerViewTemplate { get; set; }

        /// <summary>
        /// Gets or sets the template for the Scan Results.
        /// </summary>
        public DataTemplate ScanResultsViewTemplate { get; set; }

        /// <summary>
        /// Gets or sets the template for the Pointer Scan Results.
        /// </summary>
        public DataTemplate PointerScanResultsViewTemplate { get; set; }

        /// <summary>
        /// Gets or sets the template for the Browser.
        /// </summary>
        public DataTemplate BrowseViewTemplate { get; set; }

        /// <summary>
        /// Gets or sets the template for the Cheat Browser.
        /// </summary>
        public DataTemplate CheatBrowserViewTemplate { get; set; }

        /// <summary>
        /// Gets or sets the template for the Twitch Login.
        /// </summary>
        public DataTemplate TwitchLoginViewTemplate { get; set; }

        /// <summary>
        /// Gets or sets the template for the Stream Weaver.
        /// </summary>
        public DataTemplate TwitchConfigViewTemplate { get; set; }

        /// <summary>
        /// Gets or sets the template for the Stream Table.
        /// </summary>
        public DataTemplate StreamTableViewTemplate { get; set; }

        /// <summary>
        /// Gets or sets the template for the Stream Icon Editor.
        /// </summary>
        public DataTemplate StreamIconEditorViewTemplate { get; set; }

        /// <summary>
        /// Gets or sets the template for the .Net Explorer.
        /// </summary>
        public DataTemplate DotNetExplorerViewTemplate { get; set; }

        /// <summary>
        /// Gets or sets the template for the Project Explorer.
        /// </summary>
        public DataTemplate ProjectExplorerViewTemplate { get; set; }

        /// <summary>
        /// Gets or sets the template for the Signature Collector.
        /// </summary>
        public DataTemplate SignatureCollectorViewTemplate { get; set; }

        /// <summary>
        /// Returns the required template to display the given view model.
        /// </summary>
        /// <param name="item">The view model.</param>
        /// <param name="container">The dependency object.</param>
        /// <returns>The template associated with the provided view model.</returns>
        public override DataTemplate SelectTemplate(Object item, DependencyObject container)
        {
            if (item is ChangeCounterViewModel)
            {
                return this.ChangeCounterViewTemplate;
            }
            else if (item is InputCorrelatorViewModel)
            {
                return this.InputCorrelatorViewTemplate;
            }
            else if (item is LabelThresholderViewModel)
            {
                return this.LabelThresholderViewTemplate;
            }
            else if (item is ManualScannerViewModel)
            {
                return this.ManualScannerViewTemplate;
            }
            else if (item is PointerScannerViewModel)
            {
                return this.PointerScannerViewTemplate;
            }
            else if (item is SnapshotManagerViewModel)
            {
                return this.SnapshotManagerViewTemplate;
            }
            else if (item is ScanResultsViewModel)
            {
                return this.ScanResultsViewTemplate;
            }
            else if (item is PointerScanResultsViewModel)
            {
                return this.PointerScanResultsViewTemplate;
            }
            else if (item is BrowseViewModel)
            {
                return this.BrowseViewTemplate;
            }
            else if (item is TwitchLoginViewModel)
            {
                return this.TwitchLoginViewTemplate;
            }
            else if (item is StreamConfigViewModel)
            {
                return this.TwitchConfigViewTemplate;
            }
            else if (item is DotNetExplorerViewModel)
            {
                return this.DotNetExplorerViewTemplate;
            }
            else if (item is SettingsViewModel)
            {
                return this.SettingsViewTemplate;
            }
            else if (item is ProjectExplorerViewModel)
            {
                return this.ProjectExplorerViewTemplate;
            }

            return base.SelectTemplate(item, container);
        }
    }
    //// End class
}
//// End namespace