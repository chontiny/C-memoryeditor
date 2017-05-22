namespace Squalr.Source.Editors.StreamIconEditor
{
    using Docking;
    using Main;
    using Mvvm.Command;
    using Squalr.Source.StreamWeaver;
    using Squalr.Source.StreamWeaver.Table;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Input;

    /// <summary>
    /// View model for the Stream Icon Editor.
    /// </summary>
    internal class StreamIconEditorViewModel : ToolViewModel
    {
        /// <summary>
        /// The content id for the docking library associated with this view model.
        /// </summary>
        public const String ToolContentId = nameof(StreamIconEditorViewModel);

        /// <summary>
        /// The path to the stream icons.
        /// </summary>
        private const String StreamIconsPath = @"Content/Overlay/Images/Buffs/";

        /// <summary>
        /// Singleton instance of the <see cref="StreamIconEditorViewModel" /> class.
        /// </summary>
        private static Lazy<StreamIconEditorViewModel> streamIconEditorViewModelInstance = new Lazy<StreamIconEditorViewModel>(
                () => { return new StreamIconEditorViewModel(); },
                LazyThreadSafetyMode.ExecutionAndPublication);

        private String searchTerm;

        /// <summary>
        /// Prevents a default instance of the <see cref="StreamIconEditorViewModel" /> class.
        /// </summary>
        private StreamIconEditorViewModel() : base("Stream Icon Editor")
        {
            this.ContentId = StreamIconEditorViewModel.ToolContentId;
            this.SetIconCommand = new RelayCommand<StreamIcon>((streamIcon) => this.UpdateStreamIconPath(streamIcon), (streamIcon) => true);
            this.SelectIconCommand = new RelayCommand<StreamIcon>((streamIcon) => this.ChangeSelectedIcon(streamIcon), (streamIcon) => true);

            Task.Run(() => MainViewModel.GetInstance().RegisterTool(this));
        }

        /// <summary>
        /// Gets the command to set the stream icon.
        /// </summary>
        public ICommand SetIconCommand { get; private set; }

        /// <summary>
        /// Gets the command to select a stream icon.
        /// </summary>
        public ICommand SelectIconCommand { get; private set; }

        /// <summary>
        /// Gets or sets the current search term.
        /// </summary>
        public String SearchTerm
        {
            get
            {
                return this.searchTerm;
            }

            set
            {
                this.searchTerm = value;
                this.RaisePropertyChanged(nameof(this.SearchTerm));
                this.RaisePropertyChanged(nameof(this.FilteredStreamIconList));
            }
        }

        /// <summary>
        /// Gets or sets the list of stream icons.
        /// </summary>
        public IEnumerable<StreamIcon> FilteredStreamIconList
        {
            get
            {
                if (String.IsNullOrWhiteSpace(this.SearchTerm))
                {
                    return StreamTableViewModel.GetInstance().StreamIconList;
                }

                return StreamTableViewModel.GetInstance().StreamIconList
                    .Select(item => item)
                    .Where(item => item.IconName.Contains(this.SearchTerm)
                        || item.IconMeta.DisplayName.Contains(this.SearchTerm)
                        || item.IconMeta.Keywords.Any(keyword => keyword.Contains(this.SearchTerm)));
            }
        }

        /// <summary>
        /// Gets the stream icon name.
        /// </summary>
        public String StreamIconName { get; private set; }

        /// <summary>
        /// Gets or sets the selection callback.
        /// </summary>
        public Action SelectionCallBack { get; set; }

        /// <summary>
        /// Gets the selected stream icon.
        /// </summary>
        public StreamIcon SelectedStreamIcon { get; private set; }

        /// <summary>
        /// Gets a singleton instance of the <see cref="StreamIconEditorViewModel" /> class.
        /// </summary>
        /// <returns>A singleton instance of the class.</returns>
        public static StreamIconEditorViewModel GetInstance()
        {
            return StreamIconEditorViewModel.streamIconEditorViewModelInstance.Value;
        }

        /// <summary>
        /// Updates the selected stream icon
        /// </summary>
        /// <param name="text">The stream icon path.</param>
        private void ChangeSelectedIcon(StreamIcon streamIcon)
        {
            this.SelectedStreamIcon = streamIcon;
        }

        /// <summary>
        /// Updates the stream icon path.
        /// </summary>
        /// <param name="text">The stream icon path.</param>
        private void UpdateStreamIconPath(StreamIcon streamIcon)
        {
            if (streamIcon == null)
            {
                return;
            }

            this.StreamIconName = streamIcon.IconName + ".svg";
            this.SelectionCallBack?.Invoke();
        }
    }
    //// End class
}
//// End namespace