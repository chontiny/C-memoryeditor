﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Binarysharp.MemoryManagement;
using Binarysharp.MemoryManagement.Memory;
using System.Collections.Concurrent;

namespace Anathema
{
    class ChunkScanner : IChunkScannerModel
    {
        private List<MemoryChunkRoots> ChunkRoots;
        private ConcurrentBag<MemoryChunkRoots> InvalidChunkRoots;
        private Snapshot Snapshot;
        private Int32 ChunkSize;

        // User controlled variables
        private Int32 MinChanges;

        public ChunkScanner()
        {

        }

        public override void SetMinChanges(Int32 MinChanges)
        {
            this.MinChanges = MinChanges;
        }

        /// <summary>
        /// Heuristic algorithm to determine the proper chunk size for this chunk scan based on the current region size.
        /// Chunks vary in powers of 2 from 32B to 1024B
        /// </summary>
        /// <param name="MemorySize"></param>
        /// <returns></returns>
        private Int32 SetChunkSizeDoe(UInt64 MemorySize)
        {
            UInt32 Value = (UInt32)(Snapshot.GetMemorySize() >> 20);
            Int32 Bits = 0;
            while ((Value >>= 1) != 0) { Bits++; }
            Bits = (Bits <= 5 ? 5 : (Bits >= 10 ? 10 : Bits));
            return (Int32)(1 << Bits);
        }

        public override void BeginScan()
        {
            this.Snapshot = new Snapshot(SnapshotManager.GetInstance().GetActiveSnapshot());
            this.ChunkRoots = new List<MemoryChunkRoots>();
            this.InvalidChunkRoots = new ConcurrentBag<MemoryChunkRoots>();
            this.ChunkSize = SetChunkSizeDoe(Snapshot.GetMemorySize());

            // Initialize filter tree roots
            foreach (dynamic MemoryRegion in Snapshot)
                ChunkRoots.Add(new MemoryChunkRoots(MemoryRegion, ChunkSize));

            base.BeginScan();
        }

        protected override void UpdateScan()
        {

            Parallel.ForEach(ChunkRoots, (ChunkRoot) =>
            {
                try
                {
                    // Process the changes that have occurred since the last sampling for this memory page
                    ChunkRoot.ProcessChanges(Snapshot.ReadSnapshotMemoryOfRegion(ChunkRoot));
                }
                catch (ScanFailedException)
                {
                    InvalidChunkRoots.Add(ChunkRoot);
                }
            });

            // Handle invalid reads
            if (InvalidChunkRoots.Count > 0)
            {
                MemoryChunkRoots[] InvalidChunks = InvalidChunkRoots.ToArray();

                // Remove invalid items from collection
                foreach (MemoryChunkRoots Root in InvalidChunks)
                    ChunkRoots.Remove(Root);

                // Get current memory regions
                
                // Mask each chunk against the original region

                // Copy the attributes to the new regions, if they exist
                
                // Clear invalid items
                InvalidChunkRoots = new ConcurrentBag<MemoryChunkRoots>();
            }
        }

        public override void EndScan()
        {
            // Wait for the filter to finish
            base.EndScan();

            // Collect the pages that have changed
            List<SnapshotRegion> ChangedRegions = new List<SnapshotRegion>();
            for (Int32 Index = 0; Index < ChunkRoots.Count; Index++)
                ChunkRoots[Index].GetChangedRegions(ChangedRegions, MinChanges);
            ChunkRoots = null;

            // Convert trees to a list of memory regions
            List<SnapshotRegion> FilteredRegions = ChangedRegions.ConvertAll(Page => (SnapshotRegion)Page);

            // Create snapshot with results
            Snapshot FilteredSnapshot = new Snapshot(FilteredRegions.ToArray());

            // Grow regions by the size of the largest standard variable and mask this with the original memory list.
            FilteredSnapshot.SetVariableSize(sizeof(UInt64));
            FilteredSnapshot.GrowAllRegions();
            FilteredSnapshot = new Snapshot(FilteredSnapshot.MaskRegions(Snapshot, FilteredSnapshot.GetSnapshotRegions()));

            // Read memory so that there are values for the next scan to process
            FilteredSnapshot.ReadAllSnapshotMemory();
            FilteredSnapshot.SetScanMethod("Chunk Scan");

            // Save result
            SnapshotManager.GetInstance().SaveSnapshot(FilteredSnapshot);
        }

        public class MemoryChunkRoots : SnapshotRegion
        {
            private SnapshotRegion[] Chunks;
            private UInt16[] ChangeCounts;
            private UInt32?[] Checksums;

            public MemoryChunkRoots(SnapshotRegion Region, Int32 ChunkSize) : base(Region.BaseAddress, Region.RegionSize)
            {
                // Initialize state variables
                Int32 ChunkCount = RegionSize / ChunkSize + 1;
                IntPtr CurrentBase = Region.BaseAddress;
                Chunks = new SnapshotRegion[ChunkCount];
                ChangeCounts = new UInt16[ChunkCount];
                Checksums = new UInt32?[ChunkCount];

                // Initialize all chunks and checksums
                for (Int32 Index = 0; Index < ChunkCount; Index++)
                {
                    Int32 ChunkRegionSize = ChunkSize;

                    if (Index == ChunkCount - 1)
                        ChunkRegionSize = RegionSize % ChunkSize;

                    Chunks[Index] = new SnapshotRegion(CurrentBase, ChunkRegionSize);
                    ChangeCounts[Index] = 0;
                    Checksums[Index] = 0;

                    CurrentBase += ChunkSize;
                }
            }

            public void GetChangedRegions(List<SnapshotRegion> AcceptedRegions, Int32 MinChanges)
            {
                for (Int32 Index = 0; Index < Chunks.Length; Index++)
                {
                    if (ChangeCounts[Index] >= MinChanges)
                        AcceptedRegions.Add(Chunks[Index]);
                }
            }

            public void ProcessChanges(Byte[] Data)
            {
                for (Int32 Index = 0; Index < Chunks.Length; Index++)
                {
                    UInt32 NewChecksum = Hashing.ComputeCheckSum(Data,
                        (UInt32)((UInt64)Chunks[Index].BaseAddress - (UInt64)this.BaseAddress),
                        (UInt32)((UInt64)Chunks[Index].EndAddress - (UInt64)this.BaseAddress));

                    if (Checksums[Index].HasValue && Checksums[Index] != NewChecksum)
                        ChangeCounts[Index]++;

                    Checksums[Index] = NewChecksum;
                }
            }

        } // End class

    } // End class

} // End namespace