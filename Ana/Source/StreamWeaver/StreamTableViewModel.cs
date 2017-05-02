namespace Ana.Source.StreamWeaver
{
    using Ana.Source.Editors.StreamIconEditor;
    using Ana.Source.Project;
    using Ana.Source.UserSettings;
    using Ana.Source.Utils.Extensions;
    using Docking;
    using Main;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// View model for the Stream Table.
    /// </summary>
    internal class StreamTableViewModel : ToolViewModel
    {
        /// <summary>
        /// The content id for the docking library associated with this view model.
        /// </summary>
        public const String ToolContentId = nameof(StreamTableViewModel);

        /// <summary>
        /// The path to the stream icons.
        /// </summary>
        private const String StreamIconsPath = @"Content/Overlay/Images/Buffs/";

        /// <summary>
        /// Singleton instance of the <see cref="StreamTableViewModel" /> class.
        /// </summary>
        private static Lazy<StreamTableViewModel> streamTableViewModelInstance = new Lazy<StreamTableViewModel>(
                () => { return new StreamTableViewModel(); },
                LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// The list of stream icons.
        /// </summary>
        private ObservableCollection<StreamIcon> streamIconList;

        private IEnumerable<StreamTableItem> streamTableSubView;

        /// <summary>
        /// Prevents a default instance of the <see cref="StreamIconEditorViewModel" /> class.
        /// </summary>
        private StreamTableViewModel() : base("Stream Table")
        {
            this.ContentId = StreamTableViewModel.ToolContentId;
            this.StreamIconListLock = new Object();

            Task.Run(() => this.BuildStreamIconListAsync());
            Task.Run(() => MainViewModel.GetInstance().RegisterTool(this));
        }

        /// <summary>
        /// Gets or sets the list of stream icons.
        /// </summary>
        public ObservableCollection<StreamIcon> StreamIconList
        {
            get
            {
                return this.streamIconList;
            }
        }

        /// <summary>
        /// Gets or sets the stream table.
        /// </summary>
        public IEnumerable<StreamTableItem> StreamTable
        {
            get
            {
                this.streamIconList = new ObservableCollection<StreamIcon>(BuildStreamIconList());
                this.RaisePropertyChanged(nameof(this.StreamIconList));

                return this.StreamIconList.Join(
                    ProjectExplorerViewModel.GetInstance().ProjectRoot.Flatten()
                        .Where(projectItem => !String.IsNullOrWhiteSpace(projectItem.StreamCommand)),
                        streamIcon => streamIcon.IconName,
                            projectItem => projectItem.StreamIconPath?.RemoveSuffixes(true, ".svg"),
                            (streamIcon, projectItem) => new StreamTableItem(projectItem, streamIcon))
                        .OrderBy(streamTableItem => streamTableItem.StreamCommand)
                        .OrderBy(streamTableItem => streamTableItem.Category);
            }
        }

        /// <summary>
        /// Gets or sets the view of the stream table being currently viewed for rendering.
        /// </summary>
        public IEnumerable<StreamTableItem> StreamTableSubView
        {
            get
            {
                return this.streamTableSubView;
            }

            set
            {
                this.streamTableSubView = value;
                this.RaisePropertyChanged(nameof(this.StreamTableSubView));
            }
        }

        private Object StreamIconListLock { get; set; }

        /// <summary>
        /// Gets a singleton instance of the <see cref="StreamTableViewModel" /> class.
        /// </summary>
        /// <returns>A singleton instance of the class.</returns>
        public static StreamTableViewModel GetInstance()
        {
            return StreamTableViewModel.streamTableViewModelInstance.Value;
        }

        /// <summary>
        /// Lodas all stream icons from disk.
        /// </summary>
        public void BuildStreamIconListAsync()
        {
            lock (this.StreamIconListLock)
            {
                if (this.StreamIconList != null)
                {
                    return;
                }

                this.streamIconList = new ObservableCollection<StreamIcon>();

                Parallel.ForEach(
                    Directory.EnumerateFiles(StreamTableViewModel.StreamIconsPath).Where(file => file.ToLower().EndsWith(".svg")),
                    SettingsViewModel.GetInstance().ParallelSettingsFast,
                    (filePath) =>
                    {
                        App.Current.Dispatcher.Invoke(delegate
                        {
                            streamIconList.Add(new StreamIcon(filePath));
                            this.RaisePropertyChanged(nameof(this.StreamIconList));
                        });
                    });
            }
        }

        /// <summary>
        /// Lodas all stream icons from disk, without calling any property changed notification events.
        /// </summary>
        public IEnumerable<StreamIcon> BuildStreamIconList()
        {
            lock (this.StreamIconListLock)
            {
                if (this.StreamIconList != null)
                {
                    return this.StreamIconList;
                }

                List<StreamIcon> streamIcons = new List<StreamIcon>();

                Parallel.ForEach(
                    Directory.EnumerateFiles(StreamTableViewModel.StreamIconsPath).Where(file => file.ToLower().EndsWith(".svg")),
                    SettingsViewModel.GetInstance().ParallelSettingsFast,
                    (filePath) =>
                {
                    streamIcons.Add(new StreamIcon(filePath));
                });

                return streamIcons;
            }
        }
    }
    //// End class
}
//// End namespace