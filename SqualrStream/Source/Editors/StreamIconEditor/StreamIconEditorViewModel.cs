namespace SqualrStream.Source.Editors.StreamIconEditor
{
    using GalaSoft.MvvmLight.CommandWpf;
    using SqualrCore.Source.Docking;
    using SqualrCore.Source.Output;
    using SqualrStream.Source.Api;
    using SqualrStream.Source.Api.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Input;

    /// <summary>
    /// View model for the Stream Icon Editor.
    /// </summary>
    public class StreamIconEditorViewModel : ToolViewModel
    {
        /// <summary>
        /// The path to the stream icons.
        /// </summary>
        private const String StreamIconsPath = @"Content/Overlay/Images/Buffs/";

        /// <summary>
        /// The list of available stream icons.
        /// </summary>
        private IEnumerable<StreamIcon> streamIcons;

        /// <summary>
        /// Singleton instance of the <see cref="StreamIconEditorViewModel" /> class.
        /// </summary>
        private static Lazy<StreamIconEditorViewModel> streamIconEditorViewModelInstance = new Lazy<StreamIconEditorViewModel>(
                () => { return new StreamIconEditorViewModel(); },
                LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// The current search term.
        /// </summary>
        private String searchTerm;

        /// <summary>
        /// A value indicating whether the stream icon list is loading.
        /// </summary>
        private Boolean isStreamIconListLoading;

        /// <summary>
        /// Prevents a default instance of the <see cref="StreamIconEditorViewModel" /> class.
        /// </summary>
        private StreamIconEditorViewModel() : base("Stream Icon Editor")
        {
            this.SetIconCommand = new RelayCommand<StreamIcon>((streamIcon) => this.UpdateStreamIconPath(streamIcon), (streamIcon) => true);
            this.SelectIconCommand = new RelayCommand<StreamIcon>((streamIcon) => this.ChangeSelectedIcon(streamIcon), (streamIcon) => true);

            this.ObserverLock = new Object();
            this.IsStreamIconListLoading = true;
            this.IconsLoadedObservers = new List<IStreamIconsLoadedObserver>();

            this.LoadIcons();

            Task.Run(() => DockingViewModel.GetInstance().RegisterViewModel(this));
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
        /// A value indicating whether the stream icon list is loading.
        /// </summary>
        public Boolean IsStreamIconListLoading
        {
            get
            {
                return this.isStreamIconListLoading;
            }

            set
            {
                this.isStreamIconListLoading = value;
                this.RaisePropertyChanged(nameof(this.IsStreamIconListLoading));
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
                    return this.streamIcons;
                }

                return this.streamIcons?
                    .Select(item => item)
                    .Where(item => item.IconName.IndexOf(this.SearchTerm, StringComparison.InvariantCultureIgnoreCase) >= 0
                        || item.Keywords.Any(keyword => keyword.IndexOf(this.SearchTerm, StringComparison.InvariantCultureIgnoreCase) >= 0));
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
        /// Gets or sets a lock to ensure multiple entities do not try and update the observer list at the same time.
        /// </summary>
        private Object ObserverLock { get; set; }

        /// <summary>
        /// Gets or sets objects observing the loaded icons event.
        /// </summary>
        private List<IStreamIconsLoadedObserver> IconsLoadedObservers { get; set; }

        /// <summary>
        /// Gets a singleton instance of the <see cref="StreamIconEditorViewModel" /> class.
        /// </summary>
        /// <returns>A singleton instance of the class.</returns>
        public static StreamIconEditorViewModel GetInstance()
        {
            return StreamIconEditorViewModel.streamIconEditorViewModelInstance.Value;
        }

        /// <summary>
        /// Subscribes the given object to the icon load event.
        /// </summary>
        /// <param name="observer">The object to observe the loaded icons.</param>
        public void Subscribe(IStreamIconsLoadedObserver observer)
        {
            lock (this.ObserverLock)
            {
                if (!this.IconsLoadedObservers.Contains(observer))
                {
                    this.IconsLoadedObservers.Add(observer);
                }
            }
        }

        /// <summary>
        /// Unsubscribes the given object from the icon load event.
        /// </summary>
        /// <param name="observer">The object observing the loaded icons.</param>
        public void Unsubscribe(IStreamIconsLoadedObserver observer)
        {
            lock (this.ObserverLock)
            {
                if (this.IconsLoadedObservers.Contains(observer))
                {
                    this.IconsLoadedObservers.Remove(observer);
                }
            }
        }

        /// <summary>
        /// Notifies all observers that the icons were loaded.
        /// </summary>
        public void NotifyIconsLoaded()
        {
            lock (this.ObserverLock)
            {
                foreach (IStreamIconsLoadedObserver observer in this.IconsLoadedObservers.ToArray())
                {
                    observer.Update(this.streamIcons);
                }
            }
        }

        /// <summary>
        /// Attempts to load icons from the Squalr api.
        /// </summary>
        private void LoadIcons()
        {
            Task.Run(() =>
            {
                try
                {
                    this.streamIcons = SqualrApi.GetStreamIcons();
                    this.RaisePropertyChanged(nameof(this.FilteredStreamIconList));
                    this.IsStreamIconListLoading = false;
                    this.NotifyIconsLoaded();
                }
                catch (Exception ex)
                {
                    OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Error, "Error loading icons", ex);
                }
            });
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

            this.StreamIconName = streamIcon.IconName;
            this.SelectionCallBack?.Invoke();
        }
    }
    //// End class
}
//// End namespace