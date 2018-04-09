namespace Squalr.Engine
{
    using Squalr.Engine.Architecture;
    using Squalr.Engine.Debugger;
    using Squalr.Engine.Graphics;
    using Squalr.Engine.Input;
    using Squalr.Engine.Memory;
    using Squalr.Engine.Memory.Clr;
    using Squalr.Engine.Networks;
    using Squalr.Engine.Output;
    using Squalr.Engine.Processes;
    using Squalr.Engine.Speed;
    using Squalr.Engine.Unrandomizer;
    using System;
    using System.Threading;

    /// <summary>
    /// </summary>
    public class Eng
    {
        /// <summary>
        /// Singleton instance of the <see cref="Eng" /> class.
        /// </summary>
        private static Lazy<Eng> engineCoreInstance = new Lazy<Eng>(
                () => { return new Eng(); },
                LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// Prevents a default instance of the <see cref="Eng" /> class from being created.
        /// </summary>
        private Eng()
        {
            this.Processes = ProcessAdapterFactory.GetProcessAdapter();
            this.VirtualMemory = VirtualMemoryAdapterFactory.GetVirtualMemoryAdapter();
            this.Debugger = DebuggerFactory.GetDebugger();
            this.Graphics = new GraphicsAdapter();
            this.Network = new Network();
            this.Architecture = ArchitectureFactory.GetArchitecture();
            this.Input = InputManager.GetInstance();
        }

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
        /// Gets an object that provides access to an assembler and disassembler.
        /// </summary>
        public IArchitecture Architecture { get; private set; }

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
        /// Gets an instance of the engine.
        /// </summary>
        /// <returns>An instance of the engine.</returns>
        public static Eng GetInstance()
        {
            return engineCoreInstance.Value;
        }

        public void Initialize(IOutputObserver outputObserver)
        {
            Output.Output.Subscribe(outputObserver);

            if (this.Architecture.HasVectorSupport())
            {
                Output.Output.Log(LogLevel.Info, "Hardware acceleration enabled");
                Output.Output.Log(LogLevel.Info, "Vector size: " + System.Numerics.Vector<Byte>.Count);
            }

            this.StartBackgroundServices();
        }

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