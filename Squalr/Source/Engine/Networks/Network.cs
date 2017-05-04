namespace Squalr.Source.Engine.Networks
{
    using Squalr.Source.Engine.Hook.Client;
    using Squalr.Source.Engine.Processes;
    using System.Threading.Tasks;

    internal class Network : INetwork, IProcessObserver
    {
        public Network()
        {
            this.hookClient = new HookClient();

            Task.Run(() => { EngineCore.GetInstance().Processes.Subscribe(this); });
        }

        private HookClient hookClient { get; set; }

        private NormalizedProcess TargetProcess { get; set; }

        public void Update(NormalizedProcess process)
        {
            this.TargetProcess = process;

            this.DisableNetwork();
        }

        public void DisableNetwork()
        {
            if (this.TargetProcess == null)
            {
                return;
            }

            this.EnableNetwork();

            this.hookClient?.Inject(this.TargetProcess.ProcessId);
        }

        public void EnableNetwork()
        {
            this.hookClient?.Uninject();
        }
    }
    //// End class
}
//// End namespace