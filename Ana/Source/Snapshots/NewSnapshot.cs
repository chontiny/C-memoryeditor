namespace Ana.Source.Snapshots
{
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

        unsafe DateTime GetTimeSinceLastUpdate();

        IEnumerable<ISnapshotRegion> GetSnapshotRegions();

        ISnapshot Clone();

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

        void MaskRegions(IEnumerable<ISnapshotRegion<DataType, LabelType>> groundTruth);

        ISnapshot<NewDataType, LabelType> CloneAs<NewDataType>() where NewDataType : struct, IComparable<NewDataType>;
    }

    internal class NewSnapshot<DataType, LabelType> : ISnapshot<DataType, LabelType>
        where DataType : struct, IComparable<DataType>
        where LabelType : struct, IComparable<LabelType>
    {
        /// <summary>
        /// Gets or sets the snapshot regions contained in this snapshot
        /// </summary>
        private IList<ISnapshotRegion<DataType, LabelType>> SnapshotRegions { get; set; }

        /// <summary>
        /// Gets or sets the time since the last update was performed on this snapshot
        /// </summary>
        private DateTime TimeSinceLastUpdate { get; set; }

        private Int32 Alignment { get; set; }

        private String SnapshotName { get; set; }

        public NewSnapshot(String snapshotName = null)
        {
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
            // TODO
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

            // TODO: Merge mask etc
        }

        public void MaskRegions(ISnapshot groundTruth)
        {
            // TODO
        }

        public void MaskRegions(IEnumerable<ISnapshotRegion<DataType, LabelType>> groundTruth)
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

        public ISnapshot Clone()
        {
            // TODO
            return this;
        }

        public ISnapshot<NewDataType, LabelType> CloneAs<NewDataType>() where NewDataType : struct, IComparable<NewDataType>
        {
            NewSnapshot<NewDataType, LabelType> clone = new NewSnapshot<NewDataType, LabelType>();
            // TODO
            return clone;
        }

        public Boolean ContainsAddress(IntPtr address)
        {
            // TODO
            return false;
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
    }
    //// End class
}
//// End namespace