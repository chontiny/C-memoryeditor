namespace Squalr.Source.Browse
{
    using Squalr.Source.Docking;
    using Squalr.Source.Main;
    using System;
    using System.Threading;

    /// <summary>
    /// View model for the Browser.
    /// </summary>
    internal class BrowseViewModel : ToolViewModel
    {
        /// <summary>
        /// The content id for the docking library associated with this view model.
        /// </summary>
        public const String ToolContentId = nameof(BrowseViewModel);

        /// <summary>
        /// Singleton instance of the <see cref="BrowseViewModel" /> class.
        /// </summary>
        private static Lazy<BrowseViewModel> browseViewModelInstance = new Lazy<BrowseViewModel>(
                () => { return new BrowseViewModel(); },
                LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// Prevents a default instance of the <see cref="BrowseViewModel" /> class from being created.
        /// </summary>
        private BrowseViewModel() : base("Browse")
        {
            this.ContentId = BrowseViewModel.ToolContentId;

            MainViewModel.GetInstance().RegisterTool(this);
        }

        /// <summary>
        /// Gets a singleton instance of the <see cref="BrowseViewModel"/> class.
        /// </summary>
        /// <returns>A singleton instance of the class.</returns>
        public static BrowseViewModel GetInstance()
        {
            return browseViewModelInstance.Value;
        }
    }
    //// End class
}
//// End namespace