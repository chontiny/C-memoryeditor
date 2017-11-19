using SqualrCore.Source.Engine;
using SqualrCore.Source.Engine.Processes;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Squalr.Source.SnapshotsV2
{
    internal class DEBUG_Snapshots : IProcessObserver
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
                Snapshot snapshot = SnapshotManagerViewModel.GetInstance().GetSnapshot(SnapshotRetrievalMode.FromActiveSnapshotOrPrefilter);


                foreach (SnapshotRegion region in snapshot)
                {
                    for (IEnumerator<SnapshotElementIterator> enumerator = region.IterateElements(PointerIncrementMode.AllPointers); enumerator.MoveNext();)
                    {

                    }
                }
            });
        }
    }
}
