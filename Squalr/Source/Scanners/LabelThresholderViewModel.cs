namespace Squalr.Source.Scanning
{
    using GalaSoft.MvvmLight.CommandWpf;
    using LiveCharts;
    using LiveCharts.Wpf;
    using Squalr.Engine.Scanning.Scanners;
    using Squalr.Source.Docking;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using System.Windows.Media;

    /// <summary>
    /// View model for the Label Thresholder.
    /// </summary>
    internal class LabelThresholderViewModel : ToolViewModel
    {
        /// <summary>
        /// Singleton instance of the <see cref="LabelThresholderViewModel" /> class.
        /// </summary>
        private static Lazy<LabelThresholderViewModel> labelThresholderViewModelInstance = new Lazy<LabelThresholderViewModel>(
                () => { return new LabelThresholderViewModel(); },
                LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// The histogram collection object.
        /// </summary>
        private SeriesCollection seriesCollection;

        /// <summary>
        /// The histogram labels.
        /// </summary>
        private IList<String> labels;

        /// <summary>
        /// The histogram of label values which will be discarded if the filter is applied.
        /// </summary>
        private IChartValues filteredValues;

        /// <summary>
        /// The histogram of label values which will be kept if the filter is applied.
        /// </summary>
        private IChartValues keptValues;

        /// <summary>
        /// Prevents a default instance of the <see cref="LabelThresholderViewModel" /> class from being created.
        /// </summary>
        private LabelThresholderViewModel() : base("Label Thresholder")
        {
            this.LabelThresholder = new LabelThresholder(this.OnUpdateHistogram);
            this.LowerThreshold = 0;
            this.UpperThreshold = 100;
            this.ApplyThresholdCommand = new RelayCommand(() => Task.Run(() => this.ApplyThreshold()), () => true);
            this.InvertSelectionCommand = new RelayCommand(() => Task.Run(() => this.InvertSelection()), () => true);

            DockingViewModel.GetInstance().RegisterViewModel(this);
        }

        /// <summary>
        /// Gets a command to apply a filter to the current selection.
        /// </summary>
        public ICommand ApplyThresholdCommand { get; private set; }

        /// <summary>
        /// Gets a command to invert the current filter selection.
        /// </summary>
        public ICommand InvertSelectionCommand { get; private set; }

        /// <summary>
        /// Gets or sets the histogram collection object.
        /// </summary>
        public SeriesCollection SeriesCollection
        {
            get
            {
                return this.seriesCollection;
            }

            set
            {
                this.seriesCollection = value;
                this.RaisePropertyChanged(nameof(this.SeriesCollection));
            }
        }

        /// <summary>
        /// Gets or sets the histogram labels.
        /// </summary>
        public IList<String> Labels
        {
            get
            {
                return this.labels;
            }

            set
            {
                this.labels = value;
                this.RaisePropertyChanged(nameof(this.Labels));
            }
        }

        /// <summary>
        /// Gets or sets the histogram of label values which will be discarded if the filter is applied.
        /// </summary>
        public IChartValues FilteredValues
        {
            get
            {
                return this.filteredValues;
            }

            set
            {
                this.filteredValues = value;
                this.RaisePropertyChanged(nameof(this.FilteredValues));
            }
        }

        /// <summary>
        /// Gets or sets the histogram of label values which will be kept if the filter is applied.
        /// </summary>
        public IChartValues KeptValues
        {
            get
            {
                return this.keptValues;
            }

            set
            {
                this.keptValues = value;
                this.RaisePropertyChanged(nameof(this.KeptValues));
            }
        }

        /// <summary>
        /// Gets or sets the current lower filter threshold.
        /// </summary>
        public Double LowerThreshold
        {
            get
            {
                return this.LabelThresholder.LowerThreshold;
            }

            set
            {
                this.LabelThresholder.LowerThreshold = value;
                this.RaisePropertyChanged(nameof(this.LowerThreshold));
            }
        }

        /// <summary>
        /// Gets or sets the current upper filter threshold.
        /// </summary>
        public Double UpperThreshold
        {
            get
            {
                return this.LabelThresholder.UpperThreshold;
            }

            set
            {
                this.LabelThresholder.UpperThreshold = value;
                this.RaisePropertyChanged(nameof(this.UpperThreshold));
            }
        }

        /// <summary>
        /// Gets or sets the model associated with this view model.
        /// </summary>
        private LabelThresholder LabelThresholder { get; set; }

        /// <summary>
        /// Gets a singleton instance of the <see cref="LabelThresholderViewModel"/> class.
        /// </summary>
        /// <returns>A singleton instance of the class.</returns>
        public static LabelThresholderViewModel GetInstance()
        {
            return labelThresholderViewModelInstance.Value;
        }

        /// <summary>
        /// Opens the label thresholder.
        /// </summary>
        public void OpenLabelThresholder()
        {
            this.IsVisible = true;
            this.IsSelected = true;
            this.IsActive = true;
        }

        /// <summary>
        /// Applies the current label threshold.
        /// </summary>
        private void ApplyThreshold()
        {
            this.LabelThresholder.ApplyThreshold();
        }

        /// <summary>
        /// Inverts the selected histogram region.
        /// </summary>
        private void InvertSelection()
        {
            this.LabelThresholder.ToggleInverted();
        }

        /// <summary>
        /// Callback function when the model updates the histogram.
        /// </summary>
        private void OnUpdateHistogram()
        {
            SortedList<Object, Int64> histogram = LabelThresholder.Histogram;
            SortedList<Object, Int64> histogramKept = LabelThresholder.HistogramKept;
            SortedList<Object, Int64> histogramFiltered = LabelThresholder.HistogramFiltered;

            this.labels = histogram.Keys.Select(x => (String)x.ToString()).ToList();
            this.KeptValues = new ChartValues<Int64>(histogramKept.Values.Select(x => (Int64)Math.Log(x)));
            this.FilteredValues = new ChartValues<Int64>(histogramFiltered.Values.Select(x => (Int64)Math.Log(x)));

            // Note: this used to be Dispatcher.Run -- I do not understand where this method was from
            Task.Run(() =>
            {
                if (this.SeriesCollection == null)
                {
                    this.SeriesCollection = new SeriesCollection()
                    {
                        new ColumnSeries
                        {
                            Values = this.KeptValues,
                            Fill = Brushes.Blue,
                            DataLabels = false
                        },
                        new ColumnSeries
                        {
                            Values = this.FilteredValues,
                            Fill = Brushes.Red,
                            DataLabels = false
                        }
                    };
                }
                else
                {
                    this.SeriesCollection[0].Values = this.KeptValues;
                    this.SeriesCollection[1].Values = this.FilteredValues;
                }
            });
        }
    }
    //// End class
}
//// End namespace