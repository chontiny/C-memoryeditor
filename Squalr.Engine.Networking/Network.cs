namespace Squalr.Engine.Networks
{
    using Squalr.Engine.Engine.Hook;
    using Squalr.Engine.Processes;
    using System.Diagnostics;

    public class Network : INetwork, IProcessObserver
    {
        public Network()
        {
            ProcessInfo.Default.Subscribe(this);
        }

        private HookClient HookClient { get; set; }

        private Process TargetProcess { get; set; }

        public void Update(Process process)
        {
            this.TargetProcess = process;

            // Disabled for now
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
            this.HookClient?.Inject(this.TargetProcess.Id);
        }

        public void UninstallHook()
        {
            this.HookClient?.Uninject();
            this.HookClient = null;
        }
    }
    //// End class
}
//// End namespace