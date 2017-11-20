namespace SqualrTests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class SnapshotTests
    {
        /*
        [TestMethod]
        public void TestRegionChunking()
        {
            Random random = new Random();
            List<SnapshotRegion> groundTruth = new List<SnapshotRegion>();
            List<SnapshotRegion> chunkedRegions = new List<SnapshotRegion>();

            UInt64 baseAddress = 0;
            for (Int32 i = 0; i < 10000; i++)
            {
                baseAddress += (UInt64)random.Next(1, 1024);
                UInt64 regionSize = random.Next(1, 1024).ToUInt64();
                UInt64 chunkSize = random.Next(32, 128).ToUInt64();

                SnapshotRegion region = new SnapshotRegion(baseAddress.ToIntPtr(), regionSize);
                IEnumerable<SnapshotRegion> regions = region.ChunkNormalizedRegion(chunkSize).Select(x => new SnapshotRegion(x.BaseAddress, x.RegionSize)).OrderBy(x => random.Next());

                groundTruth.Add(region);
                chunkedRegions.AddRange(regions);
                baseAddress += (UInt64)regionSize;
            }

            UInt64 originalSize = groundTruth.Sum(x => x.RegionSize);
            UInt64 newSize = chunkedRegions.Sum(x => x.RegionSize);

            Assert.AreEqual(originalSize, newSize);
        }

        [TestMethod]
        public void TestSnapshotMerging()
        {
            Random random = new Random();
            List<SnapshotRegion> groundTruth = new List<SnapshotRegion>();
            List<SnapshotRegion> chunkedRegions = new List<SnapshotRegion>();

            UInt64 baseAddress = 0;
            for (Int32 i = 0; i < 10000; i++)
            {
                baseAddress += (UInt64)random.Next(1, 1024);
                UInt64 regionSize = random.Next(1, 1024).ToUInt64();
                UInt64 chunkSize = random.Next(32, 2048).ToUInt64();

                SnapshotRegion region = new SnapshotRegion(baseAddress.ToIntPtr(), regionSize);
                IEnumerable<SnapshotRegion> regions = region.ChunkNormalizedRegion(chunkSize).Select(x => new SnapshotRegion(x.BaseAddress, x.RegionSize)).OrderBy(x => random.Next());

                groundTruth.Add(region);
                chunkedRegions.AddRange(regions);

                baseAddress += (UInt64)regionSize;
            }

            Snapshot snapshot = new Snapshot(chunkedRegions);
            List<SnapshotRegion> reconstructedRegions = snapshot.GetSnapshotRegions().ToList();

            Assert.AreEqual(groundTruth.Count, reconstructedRegions.Count);

            UInt64 originalSize = groundTruth.Sum(x => x.RegionSize);
            UInt64 newSize = reconstructedRegions.Sum(x => x.RegionSize);

            Assert.AreEqual(originalSize, newSize);
        }
        */
    }
    //// End class
}
//// End namespace