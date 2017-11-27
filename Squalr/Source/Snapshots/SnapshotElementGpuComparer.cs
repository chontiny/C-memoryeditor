namespace Squalr.Source.Snapshots
{
    using Alea;
    using Alea.Parallel;
    using Scanners.ScanConstraints;
    using SqualrCore.Source.Engine.Types;
    using SqualrCore.Source.Utils;
    using SqualrCore.Source.Utils.Extensions;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Class for comparing snapshot regions.
    /// </summary>
    [GpuManaged]
    internal class SnapshotElementGpuComparer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SnapshotElementGpuComparer" /> class.
        /// </summary>
        /// <param name="region">The parent region that contains this element.</param>
        /// <param name="scanConstraints">The constraint to use for the element quick action.</param>
        public unsafe SnapshotElementGpuComparer(
            SnapshotRegion region,
            ScanConstraintManager scanConstraints)
        {
            this.Region = region;
            this.GpuReadBase = this.Region.ReadGroupOffset;
            this.DataTypeSize = unchecked((UInt32)Conversions.SizeOf(this.Region.ReadGroup.ElementDataType));

            // Initialize capacity to 1/16 elements
            this.ResultRegions = new List<SnapshotRegion>(unchecked((Int32)(this.Region.ElementCount)) / 16);

            this.SetConstraintFunctions();
            this.SetCompareAction(scanConstraints);
        }

        /// <summary>
        /// Gets an action based on the element iterator scan constraint.
        /// </summary>
        public Func<Boolean[]> GpuCompare { get; private set; }

        /// <summary>
        /// Gets a function which determines if this element has changed.
        /// </summary>
        public Func<Boolean[]> Changed { get; private set; }

        /// <summary>
        /// Gets a function which determines if this element has not changed.
        /// </summary>
        public Func<Boolean[]> Unchanged { get; private set; }

        /// <summary>
        /// Gets a function which determines if this element has increased.
        /// </summary>
        public Func<Boolean[]> Increased { get; private set; }

        /// <summary>
        /// Gets a function which determines if this element has decreased.
        /// </summary>
        public Func<Boolean[]> Decreased { get; private set; }

        /// <summary>
        /// Gets a function which determines if this element has a value equal to the given value.
        /// </summary>
        public Func<Object, Boolean[]> EqualToValue { get; private set; }

        /// <summary>
        /// Gets a function which determines if this element has a value not equal to the given value.
        /// </summary>
        public Func<Object, Boolean[]> NotEqualToValue { get; private set; }

        /// <summary>
        /// Gets a function which determines if this element has a value greater than to the given value.
        /// </summary>
        public Func<Object, Boolean[]> GreaterThanValue { get; private set; }

        /// <summary>
        /// Gets a function which determines if this element has a value greater than or equal to the given value.
        /// </summary>
        public Func<Object, Boolean[]> GreaterThanOrEqualToValue { get; private set; }

        /// <summary>
        /// Gets a function which determines if this element has a value less than to the given value.
        /// </summary>
        public Func<Object, Boolean[]> LessThanValue { get; private set; }

        /// <summary>
        /// Gets a function which determines if this element has a value less than to the given value.
        /// </summary>
        public Func<Object, Boolean[]> LessThanOrEqualToValue { get; private set; }

        /// <summary>
        /// Gets a function which determines if the element has increased it's value by the given value.
        /// </summary>
        public Func<Object, Boolean[]> IncreasedByValue { get; private set; }

        /// <summary>
        /// Gets a function which determines if the element has decreased it's value by the given value.
        /// </summary>
        public Func<Object, Boolean[]> DecreasedByValue { get; private set; }

        /// <summary>
        /// Gets or sets the parent snapshot region.
        /// </summary>
        private SnapshotRegion Region { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether we are currently encoding a new result region.
        /// </summary>
        private Boolean Encoding { get; set; }

        /// <summary>
        /// Gets or sets the current run length for run length encoded current scan results.
        /// </summary>
        private UInt32 RunLength { get; set; }

        /// <summary>
        /// Gets or sets the index of this element.
        /// </summary>
        private UInt64 GpuReadBase { get; set; }

        /// <summary>
        /// Gets or sets the size of the data type being compared.
        /// </summary>
        private UInt32 DataTypeSize { get; set; }

        /// <summary>
        /// Gets or sets the list of discovered result regions.
        /// </summary>
        private IList<SnapshotRegion> ResultRegions { get; set; }

        /// <summary>
        /// Compares the parent snapshot region at the current vector index.
        /// </summary>
        public IList<SnapshotRegion> Compare()
        {
            Boolean[] scanResults = this.GpuCompare();

            // NOTE: This code is visually similar but VERY different to the snapshot vector comparer.
            // Below, each byte in the vector represents the result of an element comparison. In the vector comparer,
            // a set of bytes from 1 to n (depending on data type size) represented the result of an element comparison. Be careful.
            /*
            UInt32 vectorSize = EngineCore.GetInstance().Architecture.GetVectorSize();

            for (UInt64 elementIndex = 0; elementIndex < this.Region.ElementCount; elementIndex += vectorSize)
            {
                Vector<Byte> vectorScanResults = new Vector<Byte>(scanResults, unchecked((Int32)elementIndex));

                // Check all vector results true (vector of 0xFF's, which is how SSE instructions store true)
                if (Vector.GreaterThanAll(vectorScanResults, Vector<Byte>.Zero))
                {
                    this.RunLength += vectorSize * this.DataTypeSize;
                    this.Encoding = true;
                }
                // Check all vector results false
                else if (Vector.EqualsAll(vectorScanResults, Vector<Byte>.Zero))
                {
                    if (this.Encoding)
                    {
                        this.ResultRegions.Add(new SnapshotRegion(this.Region.ReadGroup, this.GpuReadBase + elementIndex * this.DataTypeSize - this.RunLength, this.RunLength));
                        this.RunLength = 0;
                        this.Encoding = false;
                    }
                }
                // Otherwise the vector contains a mixture of true and false
                else
                {
                    for (UInt32 vectorIndex = 0; vectorIndex < vectorSize; vectorIndex++)
                    {
                        // Vector result was false
                        if (vectorScanResults[unchecked((Int32)vectorIndex)] == 0)
                        {
                            if (this.Encoding)
                            {
                                this.ResultRegions.Add(new SnapshotRegion(this.Region.ReadGroup, this.GpuReadBase + elementIndex * this.DataTypeSize + vectorIndex * this.DataTypeSize - this.RunLength, this.RunLength));
                                this.RunLength = 0;
                                this.Encoding = false;
                            }
                        }
                        // Vector result was true
                        else
                        {
                            this.RunLength += this.DataTypeSize;
                            this.Encoding = true;
                        }
                    }
                }
            }

            // TODO: cleanup remaining elements

            // Create the final region if we are still encoding
            if (this.Encoding)
            {
                // this.ResultRegions.Add(new SnapshotRegion(this.Region.ReadGroup, this.GpuReadBase + this.RunLength, this.RunLength));
                this.RunLength = 0;
                this.Encoding = false;
            }*/

            return this.ResultRegions;
            for (UInt64 index = 0; index < this.Region.ElementCount; index++)
            {
                if (scanResults[unchecked((Int32)index)])
                {
                    this.RunLength += this.DataTypeSize;
                    this.Encoding = true;
                }
                else
                {
                    if (this.Encoding)
                    {
                        this.ResultRegions.Add(new SnapshotRegion(this.Region.ReadGroup, this.GpuReadBase + index * this.DataTypeSize - this.RunLength, this.RunLength));
                        this.RunLength = 0;
                        this.Encoding = false;
                    }
                }
            }

            // Create the final region if we are still encoding
            if (this.Encoding)
            {
                this.ResultRegions.Add(new SnapshotRegion(this.Region.ReadGroup, this.GpuReadBase + this.RunLength, this.RunLength));
                this.RunLength = 0;
                this.Encoding = false;
            }

            return this.ResultRegions;
        }

        /// <summary>
        /// Initializes all constraint functions for value comparisons.
        /// </summary>
        [GpuManaged]
        private unsafe void SetConstraintFunctions()
        {
            switch (this.Region.ReadGroup.ElementDataType)
            {
                case DataType type when type == DataTypes.Byte:
                    break;
                case DataType type when type == DataTypes.SByte:
                    break;
                case DataType type when type == DataTypes.Int16:
                    break;
                case DataType type when type == DataTypes.Int32:

                    this.EqualToValue = (value) =>
                    {
                        Byte[] currentValues = this.Region.ReadGroup.CurrentValues;
                        Boolean[] result = new Boolean[this.Region.ElementCount];
                        Int64 baseIndex = this.GpuReadBase.ToInt64();

                        Int32 realValue = unchecked((Int32)value);

                        // TODO: Figure out how to implement this https://erkaman.github.io/posts/cuda_rle.html
                        Gpu.Default.LongFor(0, this.Region.ElementCount.ToInt64(), (index) =>
                        {
                            fixed (Byte* currentValuePointer = &currentValues[0])
                            {
                                result[index] = ((Int32*)currentValuePointer)[baseIndex + index] == realValue;
                            }
                        });

                        return result;
                    };
                    break;
                case DataType type when type == DataTypes.Int64:
                    break;
                case DataType type when type == DataTypes.UInt16:
                    break;
                case DataType type when type == DataTypes.UInt32:
                    break;
                case DataType type when type == DataTypes.UInt64:
                    break;
                case DataType type when type == DataTypes.Single:
                    break;
                case DataType type when type == DataTypes.Double:
                    break;
                default:
                    throw new ArgumentException();
            }
        }

        /// <summary>
        /// Sets the default compare action to use for this element.
        /// </summary>
        /// <param name="scanConstraints">The constraint to use for the element quick action.</param>
        /// <param name="compareActionValue">The value to use for the element quick action.</param>
        private void SetCompareAction(ScanConstraintManager scanConstraints)
        {
            foreach (ScanConstraint scanConstraint in scanConstraints)
            {
                Func<Boolean[]> vectorCompare = null;

                switch (scanConstraint.Constraint)
                {
                    case ScanConstraint.ConstraintType.Unchanged:
                        vectorCompare = this.Unchanged;
                        break;
                    case ScanConstraint.ConstraintType.Changed:
                        vectorCompare = this.Changed;
                        break;
                    case ScanConstraint.ConstraintType.Increased:
                        vectorCompare = this.Increased;
                        break;
                    case ScanConstraint.ConstraintType.Decreased:
                        vectorCompare = this.Decreased;
                        break;
                    case ScanConstraint.ConstraintType.IncreasedByX:
                        vectorCompare = () => this.IncreasedByValue(scanConstraint.ConstraintValue);
                        break;
                    case ScanConstraint.ConstraintType.DecreasedByX:
                        vectorCompare = () => this.DecreasedByValue(scanConstraint.ConstraintValue);
                        break;
                    case ScanConstraint.ConstraintType.Equal:
                        vectorCompare = () => this.EqualToValue(scanConstraint.ConstraintValue);
                        break;
                    case ScanConstraint.ConstraintType.NotEqual:
                        vectorCompare = () => this.NotEqualToValue(scanConstraint.ConstraintValue);
                        break;
                    case ScanConstraint.ConstraintType.GreaterThan:
                        vectorCompare = () => this.GreaterThanValue(scanConstraint.ConstraintValue);
                        break;
                    case ScanConstraint.ConstraintType.GreaterThanOrEqual:
                        vectorCompare = () => this.GreaterThanOrEqualToValue(scanConstraint.ConstraintValue);
                        break;
                    case ScanConstraint.ConstraintType.LessThan:
                        vectorCompare = () => this.LessThanValue(scanConstraint.ConstraintValue);
                        break;
                    case ScanConstraint.ConstraintType.LessThanOrEqual:
                        vectorCompare = () => this.LessThanOrEqualToValue(scanConstraint.ConstraintValue);
                        break;
                    default:
                        throw new Exception("Unknown constraint type");
                }

                if (this.GpuCompare == null)
                {
                    this.GpuCompare = vectorCompare;
                }
                else
                {
                    // We have multiple constraints -- the best way to enforce this is to simply AND the scan results together
                    Func<Boolean[]> currentCompare = this.GpuCompare;

                    // TODO: Just and the Byte[]s together
                    // this.GpuCompare = () => Vector.BitwiseAnd(currentCompare.Invoke(), vectorCompare.Invoke());
                }
            }
        }
    }
    //// End class
}
//// End namespace