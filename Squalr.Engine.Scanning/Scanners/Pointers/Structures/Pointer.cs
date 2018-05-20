namespace Squalr.Engine.Scanning.Scanners.Pointers.Structures
{
    using System;

    public class Pointer
    {
        public Pointer(
            UInt64 baseAddress,
            PointerSize pointerSize,
            Int32[] offsets = null)
        {
            this.BaseAddress = baseAddress;
            this.PointerSize = pointerSize;
            this.Offsets = offsets;
        }

        public UInt64 BaseAddress { get; private set; }

        public Int32[] Offsets { get; private set; }

        public PointerSize PointerSize { get; private set; }
    }
    //// End class
}
//// End namespace