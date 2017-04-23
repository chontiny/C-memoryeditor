namespace Ana.Source.ProcessSelector
{
    using Docking;
    using Engine;
    using Engine.Processes;
    using Main;
    using Mvvm.Command;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using Utils;
    using Utils.Extensions;

    /// <summary>
    /// View model for the Process Selector.
    /// </summary>
    internal class ProcessSelectorViewModel : ToolViewModel, IProcessObserver
    {
        /// <summary>
        /// The content id for the docking library associated with this view model.
        /// </summary>
        public const String ToolContentId = nameof(ProcessSelectorViewModel);

        /// <summary>
        /// Singleton instance of the <see cref="ProcessSelectorViewModel" /> class.
        /// </summary>
        private static Lazy<ProcessSelectorViewModel> processSelectorViewModelInstance = new Lazy<ProcessSelectorViewModel>(
                () => { return new ProcessSelectorViewModel(); },
                LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// The list of running processes.
        /// </summary>
        private IEnumerable<NormalizedProcess> processList;

        /// <summary>
        /// Prevents a default instance of the <see cref="ProcessSelectorViewModel" /> class from being created.
        /// </summary>
        private ProcessSelectorViewModel() : base("Process Selector")
        {
            this.ContentId = ProcessSelectorViewModel.ToolContentId;
            this.IconSource = ImageUtils.LoadImage("pack://application:,,,/Ana;component/Content/Icons/SelectProcess.png");
            this.RefreshProcessListCommand = new RelayCommand(() => Task.Run(() => this.RefreshProcessList()), () => true);
            this.SelectProcessCommand = new RelayCommand<NormalizedProcess>((process) => Task.Run(() => this.SelectProcess(process)), (process) => true);

            ProcessSelectorTask processSelectorTask = new ProcessSelectorTask(this.RefreshProcessList);

            // Subscribe async to avoid a deadlock situation
            Task.Run(() => { MainViewModel.GetInstance().RegisterTool(this); });

            // Subscribe to process events (async call as to avoid locking on GetInstance() if engine is being constructed)
            Task.Run(() => { EngineCore.GetInstance().Processes.Subscribe(this); });
        }

        /// <summary>
        /// Gets the command to refresh the process list.
        /// </summary>
        public ICommand RefreshProcessListCommand { get; private set; }

        /// <summary>
        /// Gets the command to select a target process.
        /// </summary>
        public ICommand SelectProcessCommand { get; private set; }

        /// <summary>
        /// Gets the processes running on the machine.
        /// </summary>
        public IEnumerable<NormalizedProcess> ProcessList
        {
            get
            {
                return this.processList;
            }

            set
            {
                this.processList = value;

                this.RaisePropertyChanged(nameof(this.ProcessList));
                this.RaisePropertyChanged(nameof(this.WindowedProcessList));
            }
        }

        /// <summary>
        /// Gets the processes with a window running on the machine, as well as the selected process.
        /// </summary>
        public IEnumerable<NormalizedProcess> WindowedProcessList
        {
            get
            {
                return this.ProcessList?.Where(x => x.HasWindow).Select(x => x).PrependIfNotNull(this.SelectedProcess).Distinct();
            }
        }

        /// <summary>
        /// Gets or sets the selected process.
        /// </summary>
        public NormalizedProcess SelectedProcess
        {
            get
            {
                return EngineCore.GetInstance().Processes.GetOpenedProcess();
            }

            set
            {
                EngineCore.GetInstance().Processes.OpenProcess(value);
                this.RaisePropertyChanged(nameof(this.SelectedProcess));
            }
        }

        /// <summary>
        /// Gets the name of the selected process.
        /// </summary>
        public String ProcessName
        {
            get
            {
                String processName = EngineCore.GetInstance().Processes?.GetOpenedProcess()?.ProcessName;
                return String.IsNullOrEmpty(processName) ? "Please Select a Process" : processName;
            }
        }

        /// <summary>
        /// Gets a singleton instance of the <see cref="ProcessSelectorViewModel"/> class.
        /// </summary>
        /// <returns>A singleton instance of the class.</returns>
        public static ProcessSelectorViewModel GetInstance()
        {
            return processSelectorViewModelInstance.Value;
        }

        /// <summary>
        /// Recieves a process update.
        /// </summary>
        /// <param name="process">The newly selected process.</param>>
        public void Update(NormalizedProcess process)
        {
            // Raise event to update process name in the view
            this.RaisePropertyChanged(nameof(this.ProcessName));
        }

        /// <summary>
        /// Called when the visibility of this tool is changed.
        /// </summary>
        protected override void OnVisibilityChanged()
        {
            if (this.IsVisible)
            {
                this.RefreshProcessList();
            }
        }

        /// <summary>
        /// Refreshes the process list.
        /// </summary>
        private void RefreshProcessList()
        {
            this.ProcessList = EngineCore.GetInstance().Processes.GetProcesses();
        }

        /// <summary>
        /// Makes the target process selection.
        /// </summary>
        /// <param name="process">The process being selected.</param>
        private void SelectProcess(NormalizedProcess process)
        {
            if (process == null)
            {
                return;
            }

            this.SelectedProcess = process;
            this.RefreshProcessList();
            this.IsVisible = false;
        }
    }
    //// End class
}
//// End namespace