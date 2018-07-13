using Squalr.Engine.DataTypes;
using System;

namespace Squalr.Engine.Scanning.Scanners.Pointers.Structures
{
    public static class PointerSizeExtensions
    {
        public static Int32 ToSize(this PointerSize pointerSize)
        {
            switch (pointerSize)
            {
                case PointerSize.Byte4:
                    return 4;
                case PointerSize.Byte8:
                    return 8;
                default:
                    throw new ArgumentException("Unknown pointer size");
            }
        }

        public static DataType ToDataType(this PointerSize pointerSize)
        {
            switch (pointerSize)
            {
                case PointerSize.Byte4:
                    return DataType.UInt32;
                case PointerSize.Byte8:
                    return DataType.UInt64;
                default:
                    throw new ArgumentException("Unknown pointer size");
            }
        }
    }

    /// <summary>
    /// An enum for possible pointer sizes.
    /// </summary>
    public enum PointerSize
    {
        Byte4,
        Byte8,
    }
    //// End class
}
//// End namespace