namespace SqualrCore.Source.Docking
{
    using GalaSoft.MvvmLight;
    using System;
    using System.Collections.Generic;
    using System.Threading;

    /// <summary>
    /// Docking view model.
    /// </summary>
    public class DockingViewModel : ViewModelBase
    {
        /// <summary>
        /// Singleton instance of the <see cref="DockingViewModel" /> class
        /// </summary>
        private static Lazy<DockingViewModel> mainViewModelInstance = new Lazy<DockingViewModel>(
                () => { return new DockingViewModel(); },
                LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// Collection of tools contained in the main docking panel.
        /// </summary>
        private HashSet<ToolViewModel> tools;

        /// <summary>
        /// Prevents a default instance of the <see cref="DockingViewModel" /> class from being created.
        /// </summary>
        private DockingViewModel()
        {
            this.tools = new HashSet<ToolViewModel>();
        }

        /// <summary>
        /// Gets the tools contained in the main docking panel.
        /// </summary>
        public IEnumerable<ToolViewModel> Tools
        {
            get
            {
                if (this.tools == null)
                {
                    this.tools = new HashSet<ToolViewModel>();
                }

                return this.tools;
            }
        }

        /// <summary>
        /// Gets the singleton instance of the <see cref="DockingViewModel" /> class.
        /// </summary>
        /// <returns>The singleton instance of the <see cref="DockingViewModel" /> class.</returns>
        public static DockingViewModel GetInstance()
        {
            return mainViewModelInstance.Value;
        }

        /// <summary>
        /// Registers a view model to the list of available view models for docking.
        /// </summary>
        /// <param name="observer">The tool to be added.</param>
        public void RegisterViewModel(ToolViewModel observer)
        {
            if (observer != null && !this.tools.Contains(observer))
            {
                this.tools?.Add(observer);
            }

            this.RaisePropertyChanged(nameof(this.Tools));
        }
    }
    //// End class
}
//// End namesapce