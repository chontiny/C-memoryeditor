namespace Squalr.Source.Engine.Networks
{
    using Squalr.Source.Engine.Hook.Client;
    using Squalr.Source.Engine.Processes;
    using System;
    using System.Threading.Tasks;

    internal class Network : INetwork, IProcessObserver
    {
        public Network()
        {
            Task.Run(() => { EngineCore.GetInstance().Processes.Subscribe(this); });
        }

        private HookClient HookClient { get; set; }

        private NormalizedProcess TargetProcess { get; set; }

        public void Update(NormalizedProcess process)
        {
            this.TargetProcess = process;

            this.UninstallHook();
            this.InstallHook();
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

        private AppDomain ad;

        public void UninstallHook()
        {
            this.HookClient?.Uninject();
            this.HookClient = null;
        }
    }
    //// End class
}
//// End namespace