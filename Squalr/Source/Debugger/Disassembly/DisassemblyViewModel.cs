namespace Squalr.Source.Debugger.Disassembly
{
    using SqualrCore.Source.Docking;
    using SqualrCore.Source.Engine;
    using SqualrCore.Source.Engine.Architecture;
    using SqualrCore.Source.Engine.Architecture.Disassembler;
    using SqualrCore.Source.Engine.Processes;
    using SqualrCore.Source.Engine.VirtualMachines;
    using SqualrCore.Source.ProjectItems;
    using SqualrCore.Source.Utils.DataStructures;
    using SqualrCore.Source.Utils.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Input;

    /// <summary>
    /// View model for the Disassembly.
    /// </summary>
    internal class DisassemblyViewModel : ToolViewModel, IProcessObserver
    {
        /// <summary>
        /// Singleton instance of the <see cref="DisassemblyViewModel" /> class.
        /// </summary>
        private static Lazy<DisassemblyViewModel> disassemblyViewModelInstance = new Lazy<DisassemblyViewModel>(
                () => { return new DisassemblyViewModel(); },
                LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// The base address from which dissassembly begins.
        /// </summary>
        private UInt64 baseAddress;

        /// <summary>
        /// The collection of visible instructions.
        /// </summary>
        private FullyObservableCollection<InstructionItem> instructions;

        /// <summary>
        /// Prevents a default instance of the <see cref="DisassemblyViewModel" /> class from being created.
        /// </summary>
        private DisassemblyViewModel() : base("Disassembly")
        {
            this.Instructions = new FullyObservableCollection<InstructionItem>();

            DockingViewModel.GetInstance().RegisterViewModel(this);

            Task.Run(() => EngineCore.GetInstance().Processes.Subscribe(this));
        }

        /// <summary>
        /// Gets a command to resume execution.
        /// </summary>
        public ICommand Resume { get; private set; }

        /// <summary>
        /// Gets or sets the base address from which dissassembly begins.
        /// </summary>
        public UInt64 BaseAddress
        {
            get
            {
                return this.baseAddress;
            }

            set
            {
                this.baseAddress = value;
                this.RaisePropertyChanged(nameof(this.BaseAddress));
            }
        }

        /// <summary>
        /// Gets or sets the collection of visible instructions.
        /// </summary>
        public FullyObservableCollection<InstructionItem> Instructions
        {
            get
            {
                return this.instructions;
            }

            set
            {
                this.instructions = value;
                this.RaisePropertyChanged(nameof(this.Instructions));
            }
        }

        /// <summary>
        /// Gets the disassembler.
        /// </summary>
        private IDisassembler Disassembler
        {
            get
            {
                return EngineCore.GetInstance().Architecture.GetDisassembler();
            }
        }

        /// <summary>
        /// Gets a singleton instance of the <see cref="DisassemblyViewModel"/> class.
        /// </summary>
        /// <returns>A singleton instance of the class.</returns>
        public static DisassemblyViewModel GetInstance()
        {
            return disassemblyViewModelInstance.Value;
        }

        public void Update(NormalizedProcess process)
        {
            if (process != null)
            {
                this.BaseAddress = EngineCore.GetInstance().VirtualMemory.GetModules().FirstOrDefault()?.BaseAddress.ToUInt64() ?? 0UL;

                this.LoadInstructions();
            }
        }

        /// <summary>
        /// Loads the instructions to display.
        /// </summary>
        private void LoadInstructions()
        {
            Byte[] bytes = EngineCore.GetInstance().VirtualMemory.ReadBytes(this.BaseAddress.ToIntPtr(), 200, out _);

            if (bytes.IsNullOrEmpty())
            {
                return;
            }

            Boolean isProcess32Bit = EngineCore.GetInstance().Processes.IsOpenedProcess32Bit();

            // Disassemble instructions
            IEnumerable<NormalizedInstruction> disassembledInstructions = this.Disassembler.Disassemble(bytes, isProcess32Bit, this.BaseAddress.ToIntPtr());
            IList<InstructionItem> instructions = new List<InstructionItem>();

            foreach (NormalizedInstruction disassembledInstruction in disassembledInstructions)
            {
                String moduleName;
                UInt64 address = AddressResolver.GetInstance().AddressToModule(disassembledInstruction.Address, out moduleName);

                instructions.Add(new InstructionItem(address.ToIntPtr(), moduleName, disassembledInstruction.Instruction, disassembledInstruction.Bytes));
            }

            this.Instructions = new FullyObservableCollection<InstructionItem>(instructions);
        }
    }
    //// End class
}
//// End namespace