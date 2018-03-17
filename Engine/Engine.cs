namespace Squalr.Engine
{
    using Squalr.Engine.Architecture;
    using Squalr.Engine.Debugger;
    using Squalr.Engine.Graphics;
    using Squalr.Engine.Input;
    using Squalr.Engine.Networks;
    using Squalr.Engine.Processes;
    using Squalr.Engine.Speed;
    using Squalr.Engine.Unrandomizer;
    using Squalr.Engine.VirtualMachines;
    using Squalr.Engine.VirtualMachines.DotNet;
    using System;
    using System.Threading;
    using VirtualMemory;

    /// <summary>
    /// </summary>
    public class Engine
    {
        /// <summary>
        /// Singleton instance of the <see cref="Engine" /> class.
        /// </summary>
        private static readonly Lazy<Engine> engineInstance = new Lazy<Engine>(
            () => { return new Engine(); },
            LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// Gets an instance of the engine.
        /// </summary>
        /// <returns>An instance of the engine.</returns>
        public static Engine GetInstance()
        {
            return engineInstance.Value;
        }

        public Engine()
        {
            this.Architecture = ArchitectureFactory.GetArchitecture();
            this.Processes = ProcessAdapterFactory.GetProcessAdapter();
            this.VirtualMemory = VirtualMemoryAdapterFactory.GetVirtualMemoryAdapter();
            this.Debugger = DebuggerFactory.GetDebugger();
            this.Input = new InputManager();
            this.SpeedManipulator = new SpeedManipulator();
            this.Graphics = new GraphicsAdapter();
            this.Network = new Network();

            if (this.Architecture.HasVectorSupport())
            {
                Output.Output.Log(Output.LogLevel.Info, "Hardware acceleration enabled");
                Output.Output.Log(Output.LogLevel.Info, "Vector size: " + System.Numerics.Vector<Byte>.Count);
            }
        }

        /// <summary>
        /// Gets an object that provides access to an assembler and disassembler.
        /// </summary>
        public IArchitecture Architecture { get; private set; }

        /// <summary>
        /// Gets an object that provides access to target process manipulations.
        /// </summary>
        public IProcessAdapter Processes { get; private set; }

        /// <summary>
        /// Gets an object that provides access to target memory manipulations.
        /// </summary>
        public IVirtualMemoryAdapter VirtualMemory { get; private set; }
        /// <summary>
        /// Gets an object that enables debugging of a process.
        /// </summary>
        public IDebugger Debugger { get; private set; }

        /// <summary>
        /// Gets an object that provides access to the network access for a process.
        /// </summary>
        public INetwork Network { get; private set; }

        /// <summary>
        /// Gets an object that provides access to target execution speed manipulations.
        /// </summary>
        public ISpeedManipulator SpeedManipulator { get; private set; }

        /// <summary>
        /// Gets an object that provides access to target random library manipulations.
        /// </summary>
        public IUnrandomizer Unrandomizer { get; private set; }

        /// <summary>
        /// Gets an object that provides access to target graphics library manipulations.
        /// </summary>
        public IGraphics Graphics { get; private set; }

        /// <summary>
        /// Gets an object that provides access to system input for mouse, keyboard, and controllers.
        /// </summary>
        public IInputManager Input { get; private set; }

        /// <summary>
        /// Starts useful services that run in the background to assist in various operations.
        /// </summary>
        private void StartBackgroundServices()
        {
            DotNetObjectCollector.GetInstance().Start();
            AddressResolver.GetInstance().Start();

            Output.Output.Log(Output.LogLevel.Info, "Background services started");
        }
    }
    //// End class
}
//// End namespace