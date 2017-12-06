namespace SqualrCore.Source.Engine.Speed
{
    using SqualrCore.Source.Engine.Hook;
    using SqualrCore.Source.Engine.Processes;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Manipulates thread speed in a target process.
    /// </summary>
    public class SpeedManipulator : ISpeedManipulator, IProcessObserver
    {
        public SpeedManipulator()
        {
            Task.Run(() => { EngineCore.GetInstance().Processes.Subscribe(this); });
        }

        /// <summary>
        /// Sets the speed in the external process.
        /// </summary>
        /// <param name="speed">The speed multiplier.</param>
        public void SetSpeed(Double speed)
        {

        }

        private HookClient HookClient { get; set; }

        private NormalizedProcess TargetProcess { get; set; }

        public void Update(NormalizedProcess process)
        {
            this.TargetProcess = process;

            // this.UninstallHook();
            // this.InstallHook();
        }

        public void InstallHook()
        {
            if (this.TargetProcess == null)
            {
                return;
            }

            this.HookClient = new HookClient();
            this.HookClient?.Inject(this.TargetProcess.ProcessId);
        }

        public void UninstallHook()
        {
            this.HookClient?.Uninject();
            this.HookClient = null;
        }
    }
    //// End interface
}
//// End namespace