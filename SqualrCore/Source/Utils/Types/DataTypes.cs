namespace SqualrCore.Source.Utils.Types
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Methods for primitive types on the system.
    /// </summary>
    public static class DataTypes
    {
        public static readonly DataType Boolean = new DataType(typeof(Boolean));

        public static readonly DataType SByte = new DataType(typeof(SByte));

        public static readonly DataType Int16 = new DataType(typeof(Int16));

        public static readonly DataType Int32 = new DataType(typeof(Int32));

        public static readonly DataType Int64 = new DataType(typeof(Int64));

        public static readonly DataType Byte = new DataType(typeof(Byte));

        public static readonly DataType UInt16 = new DataType(typeof(UInt16));

        public static readonly DataType UInt32 = new DataType(typeof(UInt32));

        public static readonly DataType UInt64 = new DataType(typeof(UInt64));

        public static readonly DataType Single = new DataType(typeof(Single));

        public static readonly DataType Double = new DataType(typeof(Double));

        /// <summary>
        /// The list of scannable data types.
        /// </summary>
        private static readonly DataType[] ScannableDataTypes = new DataType[]
        {
            DataTypes.Boolean,
            DataTypes.SByte,
            DataTypes.Int16,
            DataTypes.Int32,
            DataTypes.Int64,
            DataTypes.Byte,
            DataTypes.UInt16,
            DataTypes.UInt32,
            DataTypes.UInt64,
            DataTypes.Single,
            DataTypes.Double,
        };

        /// <summary>
        /// Gets primitive types that are available for scanning.
        /// </summary>
        /// <returns>An enumeration of scannable types.</returns>
        public static IEnumerable<DataType> GetScannableDataTypes()
        {
            return DataTypes.ScannableDataTypes;
        }
    }
    //// End class
}
//// End namespace