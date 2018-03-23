namespace Squalr.Engine.Speed
{
    using Squalr.Engine.Engine.Hook;
    using Squalr.Engine.Processes;
    using System;

    /// <summary>
    /// Manipulates thread speed in a target process.
    /// </summary>
    public class SpeedManipulator : ISpeedManipulator, IProcessObserver
    {
        public SpeedManipulator()
        {
            ProcessAdapterFactory.GetProcessAdapter().Subscribe(this);
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