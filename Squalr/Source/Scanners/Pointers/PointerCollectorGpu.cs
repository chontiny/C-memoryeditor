namespace Squalr.Source.Scanners.Pointers
{
    using Alea;
    using Alea.CSharp;
    using Squalr.Source.Snapshots;
    using SqualrCore.Source.Utils.Extensions;
    using System;
    using System.Linq;

    internal class PointerCollectorGpu
    {
        public PointerCollectorGpu()
        {

        }

        [GpuManaged]
        public void CollectPointers(Snapshot moduleSnapshot, Snapshot heapSnapshot, UInt64 minValue, UInt64 maxValue)
        {
            Byte[][] moduleRegions = moduleSnapshot.ReadGroups.Where(group => !group.CurrentValues.IsNullOrEmpty()).Select(group => group.CurrentValues).ToArray();
            Byte[][] heapRegions = heapSnapshot.ReadGroups.Where(group => !group.CurrentValues.IsNullOrEmpty()).Select(group => group.CurrentValues).ToArray();


            Gpu gpu = Gpu.Default;
            LaunchParam lp = new LaunchParam(16, 256);

            Int32[] results = new Int32[5];

            gpu.Launch(PointerCollectorKernel, lp, results, moduleRegions);
        }

        /// <summary>
        /// GPU code. This discovers pointers
        /// </summary>
        /// <param name="discoveredPointers"></param>
        /// <param name="regions"></param>
        /// <param name="heapRegions"></param>
        private static void PointerCollectorKernel(Int32[] discoveredPointers, Byte[][] regions)
        {
            Int32 start = blockIdx.x * blockDim.x + threadIdx.x;
            Int32 stride = gridDim.x * blockDim.x;

            for (Int32 i = start; i < discoveredPointers.Length; i += stride)
            {
                discoveredPointers[i] = (Byte)(regions[i][i] + regions[i][i]);
            }
        }
    }
    //// End class
}
//// End namespace