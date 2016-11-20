namespace Ana.Source.Snapshots
{
    using Engine.OperatingSystems;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UserSettings;
    using Utils.Extensions;

    internal interface ISnapshot : IEnumerable
    {
        ISnapshotElementRef this[Int64 index]
        {
            get;
        }

        void SetAlignment(Int32 alignment);

        void SetAllValidBits(Boolean isValid);

        void DiscardInvalidRegions();

        void RelaxAllRegions(Int32 relaxSize);

        void ExpandAllRegions(Int32 expandSize);

        void ReadAllMemory();

        void MaskRegions(ISnapshot groundTruth);

        void MaskRegions(IEnumerable<NormalizedRegion> groundTruth);

        unsafe DateTime GetTimeSinceLastUpdate();

        IEnumerable<ISnapshotRegion> GetSnapshotRegions();

        ISnapshot Clone(String newSnapshotName = null);

        Boolean ContainsAddress(IntPtr address);

        Int32 GetRegionCount();

        Int64 GetElementCount();

        Int64 GetByteCount();
    }

    internal interface ISnapshot<DataType, LabelType> : ISnapshot
        where DataType : struct, IComparable<DataType>
        where LabelType : struct, IComparable<LabelType>
    {
        void SetElementLabels(LabelType label);

        void AddSnapshotRegions(IEnumerable<ISnapshotRegion<DataType, LabelType>> snapshotRegions);

        ISnapshot<NewDataType, LabelType> CloneAs<NewDataType>(String newSnapshotName = null) where NewDataType : struct, IComparable<NewDataType>;
    }

    internal class NewSnapshot<DataType, LabelType> : ISnapshot<DataType, LabelType>
        where DataType : struct, IComparable<DataType>
        where LabelType : struct, IComparable<LabelType>
    {

        public NewSnapshot(String snapshotName = null)
        {
            this.TimeSinceLastUpdate = DateTime.Now;
            this.SnapshotName = snapshotName == null ? String.Empty : snapshotName;
            this.SnapshotRegions = new List<ISnapshotRegion<DataType, LabelType>>();
            this.SetAlignment(SettingsViewModel.GetInstance().Alignment);
        }

        public NewSnapshot(IEnumerable<ISnapshotRegion<DataType, LabelType>> snapshotRegions, String snapshotName = null) : this(snapshotName)
        {
            this.AddSnapshotRegions(snapshotRegions);
            this.SetAlignment(SettingsViewModel.GetInstance().Alignment);
        }

        /// <summary>
        /// Gets or sets the snapshot regions contained in this snapshot
        /// </summary>
        private IList<ISnapshotRegion<DataType, LabelType>> SnapshotRegions { get; set; }

        /// <summary>
        /// Gets the time since the last update was performed on this snapshot
        /// </summary>
        public DateTime TimeSinceLastUpdate { get; private set; }

        public String SnapshotName { get; private set; }

        /// <summary>
        /// Gets the total number of bytes contained in this snapshot
        /// </summary>
        public Int64 MemorySize
        {
            get
            {
                return this.SnapshotRegions == null ? 0L : this.SnapshotRegions.AsEnumerable().Sum(x => x.GetByteCount());
            }
        }

        private Int32 Alignment { get; set; }

        /// <summary>
        /// Note: This must take a long value, but an individual region will never span more than Int32 in size
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public ISnapshotElementRef this[Int64 index]
        {
            get
            {
                foreach (ISnapshotRegion region in this)
                {
                    Int64 elementCount = (Int64)region.GetElementCount();

                    if (index - elementCount >= 0)
                    {
                        index -= elementCount;
                    }
                    else
                    {
                        return region[(Int32)index * this.Alignment];
                    }
                }

                return null;
            }
        }

        public void SetAlignment(Int32 alignment)
        {
            this.Alignment = alignment <= 0 ? 1 : alignment;
            this.SnapshotRegions.ForEach(x => x.SetAlignment(this.Alignment));
        }

        public void SetAllValidBits(Boolean isValid)
        {
            this.SnapshotRegions.ForEach(x => x.SetAllValidBits(isValid));
        }

        public void DiscardInvalidRegions()
        {
            List<ISnapshotRegion<DataType, LabelType>> candidateRegions = new List<ISnapshotRegion<DataType, LabelType>>();

            if (this.SnapshotRegions == null || this.SnapshotRegions.Count() <= 0)
            {
                return;
            }

            // Collect valid element regions
            foreach (ISnapshotRegion<DataType, LabelType> snapshotRegion in this)
            {
                candidateRegions.AddRange(snapshotRegion.GetValidRegions());
            }

            this.SnapshotRegions = candidateRegions;
        }

        public void RelaxAllRegions(Int32 relaxSize)
        {
            this.SnapshotRegions.ForEach(x => x.Relax(relaxSize));

            // TODO: Merge mask etc
        }

        public void ExpandAllRegions(Int32 expandSize)
        {
            this.SnapshotRegions.ForEach(x => x.Expand(expandSize));

            // TODO: Merge mask etc
        }

        public void ReadAllMemory()
        {
            this.TimeSinceLastUpdate = DateTime.Now;

            Boolean readSuccess;
            this.SnapshotRegions.ForEach(x => x.ReadAllRegionMemory(out readSuccess, keepValues: true));
        }

        public void SetElementLabels(LabelType label)
        {
            this.SnapshotRegions.ForEach(x => x.SetElementLabels(label));
        }

        public void AddSnapshotRegions(IEnumerable<ISnapshotRegion<DataType, LabelType>> snapshotRegions)
        {
            snapshotRegions?.ForEach(x => this.SnapshotRegions.Add(x));

            this.MaskRegions(SnapshotManager.GetInstance().CollectSnapshotRegions(useSettings: false));
        }

        public void MaskRegions(ISnapshot groundTruth)
        {
            // TODO
        }

        public void MaskRegions(IEnumerable<NormalizedRegion> groundTruth)
        {
            // TODO
        }

        public DateTime GetTimeSinceLastUpdate()
        {
            return this.TimeSinceLastUpdate;
        }

        public IEnumerable<ISnapshotRegion> GetSnapshotRegions()
        {
            return this.SnapshotRegions;
        }

        public ISnapshot Clone(String newSnapshotName = null)
        {
            return new NewSnapshot<DataType, LabelType>(this.SnapshotRegions, newSnapshotName);
        }

        public ISnapshot<NewDataType, LabelType> CloneAs<NewDataType>(String newSnapshotName = null) where NewDataType : struct, IComparable<NewDataType>
        {
            IList<NewSnapshotRegion<NewDataType, LabelType>> clonedRegions = new List<NewSnapshotRegion<NewDataType, LabelType>>();
            this.SnapshotRegions.ForEach(x => clonedRegions.Add(new NewSnapshotRegion<NewDataType, LabelType>(x as NormalizedRegion)));
            NewSnapshot<NewDataType, LabelType> clone = new NewSnapshot<NewDataType, LabelType>(clonedRegions, newSnapshotName);

            return clone;
        }

        public Boolean ContainsAddress(IntPtr address)
        {
            if (this.SnapshotRegions == null || this.SnapshotRegions.Count() == 0)
            {
                return false;
            }

            return this.ContainsAddress(address, this.SnapshotRegions.Count() / 2, 0, this.SnapshotRegions.Count());
        }

        public Int32 GetRegionCount()
        {
            return this.SnapshotRegions == null ? 0 : this.SnapshotRegions.Count;
        }

        public Int64 GetByteCount()
        {
            return this.SnapshotRegions == null ? 0 : this.SnapshotRegions.AsEnumerable().Sum(x => x.GetByteCount());
        }

        public Int64 GetElementCount()
        {
            return this.SnapshotRegions == null ? 0 : this.SnapshotRegions.AsEnumerable().Sum(x => (Int64)x.GetElementCount());
        }

        public IEnumerator GetEnumerator()
        {
            return this.SnapshotRegions.GetEnumerator();
        }

        /// <summary>
        /// Helper function for searching for an address in this snapshot. Binary search that assumes this snapshot has sorted regions.
        /// </summary>
        /// <param name="address">The address for which we are searching</param>
        /// <param name="middle">The middle region index</param>
        /// <param name="min">The lower region index</param>
        /// <param name="max">The upper region index</param>
        /// <returns>True if the address was found</returns>
        private Boolean ContainsAddress(IntPtr address, Int32 middle, Int32 min, Int32 max)
        {
            if (middle < 0 || middle == this.SnapshotRegions.Count() || max < min)
            {
                return false;
            }

            if (address.ToUInt64() < this.SnapshotRegions.ElementAt(middle).GetBaseAddress().ToUInt64())
            {
                return this.ContainsAddress(address, (min + middle - 1) / 2, min, middle - 1);
            }
            else if (address.ToUInt64() > this.SnapshotRegions.ElementAt(middle).GetEndAddress().ToUInt64())
            {
                return this.ContainsAddress(address, (middle + 1 + max) / 2, middle + 1, max);
            }
            else
            {
                return true;
            }
        }
    }
    //// End class
}
//// End namespace