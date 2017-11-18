namespace SqualrCore.Source.Engine.Types
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Defines the data types used by the engine.
    /// </summary>
    public static class DataTypes
    {
        /// <summary>
        /// DataType for a boolean.
        /// </summary>
        public static readonly DataType Boolean = new DataType(typeof(Boolean));

        /// <summary>
        /// DataType for a signed byte.
        /// </summary>
        public static readonly DataType SByte = new DataType(typeof(SByte));

        /// <summary>
        /// DataType for a 16-bit integer.
        /// </summary>
        public static readonly DataType Int16 = new DataType(typeof(Int16));

        /// <summary>
        /// DataType for a 32-bit integer.
        /// </summary>
        public static readonly DataType Int32 = new DataType(typeof(Int32));

        /// <summary>
        /// DataType for a 64-bit integer.
        /// </summary>
        public static readonly DataType Int64 = new DataType(typeof(Int64));

        /// <summary>
        /// DataType for a byte.
        /// </summary>
        public static readonly DataType Byte = new DataType(typeof(Byte));

        /// <summary>
        /// DataType for an unsigned 16-bit integer.
        /// </summary>
        public static readonly DataType UInt16 = new DataType(typeof(UInt16));

        /// <summary>
        /// DataType for an unsigned 32-bit integer.
        /// </summary>
        public static readonly DataType UInt32 = new DataType(typeof(UInt32));

        /// <summary>
        /// DataType for an unsigned 64-bit integer.
        /// </summary>
        public static readonly DataType UInt64 = new DataType(typeof(UInt64));

        /// <summary>
        /// DataType for a single precision floating point value.
        /// </summary>
        public static readonly DataType Single = new DataType(typeof(Single));

        /// <summary>
        /// DataType for a double precision floating point value.
        /// </summary>
        public static readonly DataType Double = new DataType(typeof(Double));

        /// <summary>
        /// DataType for a char.
        /// </summary>
        public static readonly DataType Char = new DataType(typeof(Char));

        /// <summary>
        /// DataType for a string.
        /// </summary>
        public static readonly DataType String = new DataType(typeof(String));

        /// <summary>
        /// DataType for an integer pointer.
        /// </summary>
        public static readonly DataType IntPtr = new DataType(typeof(IntPtr));

        /// <summary>
        /// DataType for an unsigned integer pointer.
        /// </summary>
        public static readonly DataType UIntPtr = new DataType(typeof(UIntPtr));

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