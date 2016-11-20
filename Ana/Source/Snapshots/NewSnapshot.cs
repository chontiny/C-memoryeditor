namespace Ana.Source.Snapshots
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UserSettings;
    using Utils.Extensions;

    internal interface ISnapshot<DataType, LabelType>
        where DataType : class, IComparable<DataType>
        where LabelType : class, IComparable<LabelType>
    {
        void SetAlignment(Int32 alignment);

        void SetAllValidBits(Boolean isValid);

        void RelaxAllRegions(Int32 relaxSize);

        void ExpandAllRegions(Int32 expandSize);

        void ReadAllMemory();

        void SetElementLabels(LabelType label);

        void AddSnapshotRegions(IEnumerable<ISnapshotRegion<DataType, LabelType>> snapshotRegions);

        ISnapshot<NewDataType, LabelType> CloneAs<NewDataType>() where NewDataType : class, IComparable<NewDataType>;

        UInt64 GetMemorySize();
    }

    internal class NewSnapshot<DataType, LabelType> : ISnapshot<DataType, LabelType>
        where DataType : class, IComparable<DataType>
        where LabelType : class, IComparable<LabelType>
    {
        /// <summary>
        /// Gets or sets the snapshot regions contained in this snapshot
        /// </summary>
        private IList<ISnapshotRegion<DataType, LabelType>> SnapshotRegions { get; set; }

        public NewSnapshot()
        {
            this.SetAlignment(SettingsViewModel.GetInstance().Alignment);
        }

        public void SetAlignment(Int32 alignment)
        {
            alignment = alignment <= 0 ? 1 : alignment;
            this.SnapshotRegions.ForEach(x => x.SetAlignment(alignment));
        }

        public void SetAllValidBits(Boolean isValid)
        {
            this.SnapshotRegions.ForEach(x => x.SetAllValidBits(isValid));
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

        public ISnapshot<NewDataType, LabelType> CloneAs<NewDataType>() where NewDataType : class, IComparable<NewDataType>
        {
            NewSnapshot<NewDataType, LabelType> clone = new NewSnapshot<NewDataType, LabelType>();
            // TODO
            return clone;
        }

        public UInt64 GetMemorySize()
        {
            return this.SnapshotRegions == null ? 0 : unchecked((UInt64)this.SnapshotRegions.AsEnumerable().Sum(x => unchecked((Int64)x.GetMemorySize())));
        }
    }
    //// End class
}
//// End namespace