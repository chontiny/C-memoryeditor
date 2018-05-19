namespace Squalr.Engine.Scanning.Scanners.Pointers.Structures
{
    using System;

    public class Pointer
    {
        public Pointer(
            UInt64 baseAddress,
            Type dataType,
            Int32[] offsets = null)
        {
            this.BaseAddress = baseAddress;
            this.Offsets = offsets;
        }

        public UInt64 BaseAddress { get; set; }

        public Int32[] Offsets { get; private set; }

        public void Update()
        {
        }
    }
    //// End class
}
//// End namespace