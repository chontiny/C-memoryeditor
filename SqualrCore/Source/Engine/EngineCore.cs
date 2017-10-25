namespace SqualrCore.Source.Engine
{
    using Architecture;
    using Graphics;
    using Input;
    using OperatingSystems;
    using Processes;
    using SpeedManipulator;
    using SqualrCore.Source.Analytics;
    using SqualrCore.Source.Engine.Networks;
    using SqualrCore.Source.Engine.VirtualMachines;
    using SqualrCore.Source.Engine.VirtualMachines.DotNet;
    using SqualrCore.Source.Output;
    using System;
    using System.Threading;
    using Unrandomizer;

    /// <summary>
    /// Abstraction of the system, providing the ability to easily manipulate system internals regardless of the platform.
    /// </summary>
    public class EngineCore
    {
        /// <summary>
        /// Singleton instance of the <see cref="EngineCore" /> class.
        /// </summary>
        private static Lazy<EngineCore> engineCoreInstance = new Lazy<EngineCore>(
                () => { return new EngineCore(); },
                LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// Prevents a default instance of the <see cref="EngineCore" /> class from being created.
        /// </summary>
        private EngineCore()
        {
            this.Processes = new ProcessAdapter();
            this.OperatingSystem = OperatingSystemAdapterFactory.GetOperatingSystemAdapter();
            this.Network = new Network();
            this.Architecture = ArchitectureFactory.GetArchitecture();
            this.Input = new InputManager();

            this.StartBackgroundServices();
        }

        /// <summary>
        /// Gets an object that provides access to target process manipulations.
        /// </summary>
        public IProcesses Processes { get; private set; }

        /// <summary>
        /// Gets an object that provides access to target memory manipulations.
        /// </summary>
        public IOperatingSystemAdapter OperatingSystem { get; private set; }

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
        public static EngineCore GetInstance()
        {
            return engineCoreInstance.Value;
        }

        /// <summary>
        /// Starts useful services that run in the background to assist in various operations.
        /// </summary>
        private void StartBackgroundServices()
        {
            DotNetObjectCollector.GetInstance().Schedule();
            AddressResolver.GetInstance().Schedule();
            AnalyticsService.GetInstance().Start();

            AnalyticsService.GetInstance().SendEvent(AnalyticsService.AnalyticsAction.General, "Start");
            OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Info, "Background services started");
        }
    }
    //// End interface
}
//// End namespace