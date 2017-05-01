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

        /// <summary>
        /// Prevents a default instance of the <see cref="StreamIconEditorViewModel" /> class.
        /// </summary>
        private StreamTableViewModel() : base("Stream Table")
        {
            this.ContentId = StreamTableViewModel.ToolContentId;
            this.StreamIconListLock = new Object();
            this.StreamIconItemLock = new Object();

            Task.Run(() => this.RebuildStreamIconList());
            Task.Run(() => MainViewModel.GetInstance().RegisterTool(this));
        }

        /// <summary>
        /// Gets or sets the icon list access lock.
        /// </summary>
        private Object StreamIconListLock { get; set; }

        /// <summary>
        /// Gets or sets the icon item access lock.
        /// </summary>
        private Object StreamIconItemLock { get; set; }

        /// <summary>
        /// Gets or sets the list of stream icons.
        /// </summary>
        public ObservableCollection<StreamIcon> StreamIconList
        {
            get
            {
                return this.streamIconList;
            }

            set
            {
                this.streamIconList = value;
                this.RaisePropertyChanged(nameof(this.StreamIconList));
            }
        }

        /// <summary>
        /// Gets or sets the stream table
        /// </summary>
        public IEnumerable<StreamIcon> StreamTable
        {
            get
            {
                return this.StreamIconList.Join(ProjectExplorerViewModel.GetInstance().ProjectRoot.Flatten(),
                    streamIcon => streamIcon.IconName, projectItem => projectItem.StreamIconPath?.RemoveSuffixes(true, ".svg"), (streamIcon, projectItem) => streamIcon);
            }
        }

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
        public void RebuildStreamIconList()
        {
            lock (this.StreamIconListLock)
            {
                this.StreamIconList = new ObservableCollection<StreamIcon>();

                Parallel.ForEach(
                    Directory.EnumerateFiles(StreamTableViewModel.StreamIconsPath).Where(file => file.ToLower().EndsWith(".svg")),
                    SettingsViewModel.GetInstance().ParallelSettingsFast,
                    (filePath) =>
                {
                    StreamIcon streamIcon = new StreamIcon(filePath);

                    lock (this.StreamIconItemLock)
                    {
                        App.Current.Dispatcher.Invoke(delegate
                        {
                            this.StreamIconList.Add(streamIcon);
                            this.RaisePropertyChanged(nameof(this.StreamIconList));
                        });
                    }
                });
            }
        }
    }
    //// End class
}
//// End namespace