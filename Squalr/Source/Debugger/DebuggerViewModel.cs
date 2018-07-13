namespace Squalr.Source.Debugger
{
    using Squalr.Source.Docking;
    using System;
    using System.Threading;
    using System.Windows.Input;

    /// <summary>
    /// View model for the Debugger.
    /// </summary>
    internal class DebuggerViewModel : ToolViewModel
    {
        /// <summary>
        /// Singleton instance of the <see cref="DebuggerViewModel" /> class.
        /// </summary>
        private static Lazy<DebuggerViewModel> debuggerViewModelInstance = new Lazy<DebuggerViewModel>(
                () => { return new DebuggerViewModel(); },
                LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// Prevents a default instance of the <see cref="DebuggerViewModel" /> class from being created.
        /// </summary>
        private DebuggerViewModel() : base("Debugger")
        {
            DockingViewModel.GetInstance().RegisterViewModel(this);
        }

        /// <summary>
        /// Gets a command to resume execution.
        /// </summary>
        public ICommand Resume { get; private set; }

        /// <summary>
        /// Gets a singleton instance of the <see cref="DebuggerViewModel"/> class.
        /// </summary>
        /// <returns>A singleton instance of the class.</returns>
        public static DebuggerViewModel GetInstance()
        {
            return debuggerViewModelInstance.Value;
        }
    }
    //// End class
}
//// End namespace