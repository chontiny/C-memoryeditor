namespace Ana.Source.Engine.Networks
{
    using Ana.Source.Engine.Hook.Client;
    using Ana.Source.Engine.Processes;
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