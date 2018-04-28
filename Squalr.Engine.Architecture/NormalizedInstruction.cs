namespace Squalr.Engine.Architecture
{
    using System;
    using System.ComponentModel;

    /// <summary>
    /// Object that represents a platform agnostic instruction.
    /// </summary>
    public class Instruction : INotifyPropertyChanged
    {
        /// <summary>
        /// The instruction address.
        /// </summary>
        private UInt64 address;

        /// <summary>
        /// The string representation of the instruction.
        /// </summary>
        private String mnemonic;

        /// <summary>
        /// The instruction bytes.
        /// </summary>
        private Byte[] bytes;

        /// <summary>
        /// The size of this instruction.
        /// </summary>
        private Int32 size;

        /// <summary>
        /// Initializes a new instance of the <see cref="Instruction" /> class.
        /// </summary>
        /// <param name="address">The instruction address.</param>
        /// <param name="mnemonic">The instruction string.</param>
        /// <param name="bytes">The bytes of the instruction.</param>
        /// <param name="size">The instruction size.</param>
        public Instruction(UInt64 address, String mnemonic, Byte[] bytes, Int32 size)
        {
            this.Address = address;
            this.Mnemonic = mnemonic;
            this.Bytes = bytes;
            this.Size = size;
        }

        /// <summary>
        /// The instruction address.
        /// </summary>
        public UInt64 Address
        {
            get
            {
                return this.address;
            }

            set
            {
                this.address = value;
                this.RaisePropertyChanged(nameof(this.Address));
            }
        }

        /// <summary>
        /// Gets the string representation of the instruction.
        /// </summary>
        public String Mnemonic
        {
            get
            {
                return this.mnemonic;
            }

            set
            {
                this.mnemonic = value;
                this.RaisePropertyChanged(nameof(this.Mnemonic));
            }
        }

        /// <summary>
        /// Gets the instruction bytes.
        /// </summary>
        public Byte[] Bytes
        {
            get
            {
                return this.bytes;
            }

            set
            {
                this.bytes = value;
                this.RaisePropertyChanged(nameof(this.Bytes));
            }
        }

        /// <summary>
        /// Gets the size of this instruction.
        /// </summary>
        public Int32 Size
        {
            get
            {
                return this.size;
            }

            set
            {
                this.size = value;
                this.RaisePropertyChanged(nameof(this.Size));
            }
        }

        /// <summary>
        /// Occurs after a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Indicates that a given property in this project item has changed.
        /// </summary>
        /// <param name="propertyName">The name of the changed property.</param>
        protected void RaisePropertyChanged(String propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    //// End class
}
//// End namespace