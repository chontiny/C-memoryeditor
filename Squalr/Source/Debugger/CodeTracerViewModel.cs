namespace Squalr.Source.Debugger
{
    using GalaSoft.MvvmLight.CommandWpf;
    using Squalr.Engine;
    using Squalr.Engine.Debugger;
    using Squalr.Engine.Utils;
    using Squalr.Engine.Utils.DataStructures;
    using Squalr.Source.Docking;
    using Squalr.Source.ProjectItems;
    using Squalr.Source.Utils.Extensions;
    using System;
    using System.Threading;
    using System.Windows.Input;

    /// <summary>
    /// View model for the Code Tracer.
    /// </summary>
    internal class CodeTracerViewModel : ToolViewModel
    {
        /// <summary>
        /// Singleton instance of the <see cref="CodeTracerViewModel" /> class.
        /// </summary>
        private static Lazy<CodeTracerViewModel> codeTracerViewModelInstance = new Lazy<CodeTracerViewModel>(
                () => { return new CodeTracerViewModel(); },
                LazyThreadSafetyMode.ExecutionAndPublication);

        private FullyObservableCollection<CodeTraceResult> results;

        /// <summary>
        /// Prevents a default instance of the <see cref="CodeTracerViewModel" /> class from being created.
        /// </summary>
        private CodeTracerViewModel() : base("Code Tracer")
        {
            DockingViewModel.GetInstance().RegisterViewModel(this);

            this.Results = new FullyObservableCollection<CodeTraceResult>();

            this.FindWhatWritesCommand = new RelayCommand<ProjectItem>((projectItem) => this.FindWhatWrites(projectItem));
            this.FindWhatReadsCommand = new RelayCommand<ProjectItem>((projectItem) => this.FindWhatReads(projectItem));
            this.FindWhatAccessesCommand = new RelayCommand<ProjectItem>((projectItem) => this.FindWhatAccesses(projectItem));
        }

        /// <summary>
        /// Gets a command to find what writes to an address.
        /// </summary>
        public ICommand FindWhatWritesCommand { get; private set; }

        /// <summary>
        /// Gets a command to find what reads from an address.
        /// </summary>
        public ICommand FindWhatReadsCommand { get; private set; }

        /// <summary>
        /// Gets a command to find what accesses an an address.
        /// </summary>
        public ICommand FindWhatAccessesCommand { get; private set; }

        /// <summary>
        /// Gets a command to stop recording events.
        /// </summary>
        public ICommand StopCommand { get; private set; }

        public FullyObservableCollection<CodeTraceResult> Results
        {
            get
            {
                return this.results;
            }

            set
            {
                this.results = value;
            }
        }

        /// <summary>
        /// Gets a singleton instance of the <see cref="DebuggerViewModel"/> class.
        /// </summary>
        /// <returns>A singleton instance of the class.</returns>
        public static CodeTracerViewModel GetInstance()
        {
            return codeTracerViewModelInstance.Value;
        }

        private void FindWhatWrites(ProjectItem projectItem)
        {
            if (projectItem is AddressItem)
            {
                AddressItem addressItem = projectItem as AddressItem;

                BreakpointSize size = Eng.GetInstance().Debugger.SizeToBreakpointSize((UInt32)Conversions.SizeOf(addressItem.DataType));
                Eng.GetInstance().Debugger.FindWhatWrites(addressItem.CalculatedAddress.ToUInt64(), size, this.CodeTraceEvent);
            }
        }

        private void FindWhatReads(ProjectItem projectItem)
        {
            if (projectItem is AddressItem)
            {
                AddressItem addressItem = projectItem as AddressItem;

                BreakpointSize size = Eng.GetInstance().Debugger.SizeToBreakpointSize((UInt32)Conversions.SizeOf(addressItem.DataType));
                Eng.GetInstance().Debugger.FindWhatReads(addressItem.CalculatedAddress.ToUInt64(), size, this.CodeTraceEvent);
            }
        }

        private void FindWhatAccesses(ProjectItem projectItem)
        {
            if (projectItem is AddressItem)
            {
                AddressItem addressItem = projectItem as AddressItem;

                BreakpointSize size = Eng.GetInstance().Debugger.SizeToBreakpointSize((UInt32)Conversions.SizeOf(addressItem.DataType));
                Eng.GetInstance().Debugger.FindWhatAccesses(addressItem.CalculatedAddress.ToUInt64(), size, this.CodeTraceEvent);
            }
        }

        private void CodeTraceEvent(CodeTraceInfo codeTraceInfo)
        {
            this.Results.Add(new CodeTraceResult(codeTraceInfo));
        }
    }
    //// End class
}
//// End namespace