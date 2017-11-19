using SqualrCore.Source.Engine;
using SqualrCore.Source.Engine.Processes;
using System.Threading;
using System.Threading.Tasks;

namespace Squalr.Source.SnapshotsV2
{
    class DEBUG_Snapshots : IProcessObserver
    {
        public DEBUG_Snapshots()
        {
            Task.Run(() => EngineCore.GetInstance().Processes.Subscribe(this));
        }

        public void Update(NormalizedProcess process)
        {
            Task.Run(() =>
            {
                Thread.Sleep(1000);
                var debug = SnapshotManagerViewModel.GetInstance().GetSnapshot(SnapshotRetrievalMode.FromActiveSnapshotOrPrefilter);
            });
        }
    }
}
