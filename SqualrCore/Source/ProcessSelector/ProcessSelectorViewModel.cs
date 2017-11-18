namespace SqualrCore.Source.ProcessSelector
{
    using Docking;
    using Engine;
    using Engine.Processes;
    using GalaSoft.MvvmLight.CommandWpf;
    using SqualrCore.Content;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using Utils.Extensions;

    /// <summary>
    /// View model for the Process Selector.
    /// </summary>
    public class ProcessSelectorViewModel : ToolViewModel, IProcessObserver
    {
        /// <summary>
        /// Singleton instance of the <see cref="ProcessSelectorViewModel" /> class.
        /// </summary>
        private static Lazy<ProcessSelectorViewModel> processSelectorViewModelInstance = new Lazy<ProcessSelectorViewModel>(
                () => { return new ProcessSelectorViewModel(); },
                LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// A dummy process that detaches from the target process when selected.
        /// </summary>
        private NormalizedProcess detachProcess;

        /// <summary>
        /// The list of running processes.
        /// </summary>
        private IEnumerable<NormalizedProcess> processList;

        /// <summary>
        /// Prevents a default instance of the <see cref="ProcessSelectorViewModel" /> class from being created.
        /// </summary>
        private ProcessSelectorViewModel() : base("Process Selector")
        {
            this.IconSource = Images.SelectProcess;
            this.RefreshProcessListCommand = new RelayCommand(() => Task.Run(() => this.RefreshProcessList()), () => true);
            this.SelectProcessCommand = new RelayCommand<NormalizedProcess>((process) => Task.Run(() => this.SelectProcess(process)), (process) => true);

            this.detachProcess = new NormalizedProcess(0, "-- Detach from Process --", DateTime.Now, isSystemProcess: false, hasWindow: false, icon: null);
            ProcessSelectorTask processSelectorTask = new ProcessSelectorTask(this.RefreshProcessList);

            // Subscribe async to avoid a deadlock situation
            Task.Run(() => { DockingViewModel.GetInstance().RegisterViewModel(this); });

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
        /// Gets or sets the list of processes running on the machine.
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
                return this.ProcessList?.Where(x => x.HasWindow).Select(x => x)
                    .PrependIfNotNull(this.SelectedProcess != null ? this.DetachProcess : null)
                    .PrependIfNotNull(this.SelectedProcess)
                    .Distinct();
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
                Boolean selectedDetatchProcess = value == this.DetachProcess;

                if (selectedDetatchProcess)
                {
                    value = null;
                }

                if (value != this.SelectedProcess)
                {
                    EngineCore.GetInstance().Processes.OpenProcess(value);
                    this.RaisePropertyChanged(nameof(this.SelectedProcess));
                }

                if (selectedDetatchProcess)
                {
                    this.RaisePropertyChanged(nameof(this.WindowedProcessList));
                }
            }
        }

        /// <summary>
        /// Gets or sets a dummy process that detaches from the target process when selected.
        /// </summary>
        public NormalizedProcess DetachProcess
        {
            get
            {
                return this.detachProcess;
            }

            set
            {
                this.detachProcess = value;
                this.RaisePropertyChanged(nameof(this.DetachProcess));
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

            this.RefreshProcessList();
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
            this.SelectedProcess = process;
            this.IsVisible = false;
        }
    }
    //// End class
}
//// End namespace