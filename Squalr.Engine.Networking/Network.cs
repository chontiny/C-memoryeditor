namespace Squalr.Engine.Networks
{
    using Squalr.Engine.Engine.Hook;
    using Squalr.Engine.Processes;

    public class Network : INetwork, IProcessObserver
    {
        public Network()
        {
            ProcessAdapterFactory.GetProcessAdapter().Subscribe(this);
        }

        private Squalr.Engine.Engine.Hook.HookClient HookClient { get; set; }

        private NormalizedProcess TargetProcess { get; set; }

        public void Update(NormalizedProcess process)
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
            this.HookClient?.Inject(this.TargetProcess.ProcessId);
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