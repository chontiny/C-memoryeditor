namespace Squalr.Source.Scanners.InputCorrelator
{
    using Editors.HotkeyEditor;
    using GalaSoft.MvvmLight.Command;
    using SqualrCore.Source.Docking;
    using SqualrCore.Source.Engine.Input.HotKeys;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Input;

    /// <summary>
    /// View model for the Input Correlator.
    /// </summary>
    internal class InputCorrelatorViewModel : ToolViewModel
    {
        /// <summary>
        /// The content id for the docking library associated with this view model.
        /// </summary>
        public const String ToolContentId = nameof(InputCorrelatorViewModel);

        /// <summary>
        /// Singleton instance of the <see cref="InputCorrelatorViewModel" /> class.
        /// </summary>
        private static Lazy<InputCorrelatorViewModel> inputCorrelatorViewModelInstance = new Lazy<InputCorrelatorViewModel>(
                () => { return new InputCorrelatorViewModel(); },
                LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// Prevents a default instance of the <see cref="InputCorrelatorViewModel" /> class from being created.
        /// </summary>
        private InputCorrelatorViewModel() : base("Input Correlator")
        {
            this.ContentId = InputCorrelatorViewModel.ToolContentId;
            this.NewHotkeyCommand = new RelayCommand(() => this.NewHotkey(), () => true);
            this.RemoveHotkeyCommand = new RelayCommand<Hotkey>((hotkey) => this.RemoveHotkey(hotkey), (hotkey) => true);
            this.StartScanCommand = new RelayCommand(() => Task.Run(() => this.StartScan()), () => true);
            this.StopScanCommand = new RelayCommand(() => Task.Run(() => this.StopScan()), () => true);
            this.InputCorrelatorModel = new InputCorrelatorModel(this.ScanCountUpdated);

            DockingViewModel.GetInstance().RegisterViewModel(this);
        }

        public ICommand StartScanCommand { get; private set; }

        public ICommand StopScanCommand { get; private set; }

        public ICommand NewHotkeyCommand { get; private set; }

        public ICommand RemoveHotkeyCommand { get; private set; }

        public ObservableCollection<Hotkey> Hotkeys
        {
            get
            {
                return new ObservableCollection<Hotkey>(this.InputCorrelatorModel.HotKeys == null ? new List<Hotkey>() : this.InputCorrelatorModel.HotKeys);
            }
        }

        public Int32 ScanCount
        {
            get
            {
                return this.InputCorrelatorModel.ScanCount;
            }
        }

        private InputCorrelatorModel InputCorrelatorModel { get; set; }

        /// <summary>
        /// Gets a singleton instance of the <see cref="InputCorrelatorViewModel"/> class.
        /// </summary>
        /// <returns>A singleton instance of the class.</returns>
        public static InputCorrelatorViewModel GetInstance()
        {
            return inputCorrelatorViewModelInstance.Value;
        }

        private void NewHotkey()
        {
            HotkeyEditorModel hotkeyEditor = new HotkeyEditorModel();
            Hotkey newHotkey = hotkeyEditor.EditValue(context: null, provider: null, value: null) as Hotkey;

            if (newHotkey != null)
            {
                this.InputCorrelatorModel.HotKeys.Add(newHotkey);
                this.RaisePropertyChanged(nameof(this.Hotkeys));
            }
        }

        private void RemoveHotkey(Hotkey hotkey)
        {
            this.InputCorrelatorModel.HotKeys.Remove(hotkey);
            this.RaisePropertyChanged(nameof(this.Hotkeys));
        }

        private void ScanCountUpdated()
        {
            this.RaisePropertyChanged(nameof(this.ScanCount));
        }

        private void StartScan()
        {
            this.InputCorrelatorModel.Schedule();
        }

        private void StopScan()
        {
            this.InputCorrelatorModel.Cancel();
        }
    }
    //// End class
}
//// End namespace