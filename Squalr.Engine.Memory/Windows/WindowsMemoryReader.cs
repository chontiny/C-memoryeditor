namespace Squalr.Engine.Memory.Windows
{
    using Processes;
    using Squalr.Engine.DataTypes;
    using Squalr.Engine.Utils.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using Utils;

    /// <summary>
    /// Class for memory editing a remote process.
    /// </summary>
    internal class WindowsMemoryReader : IMemoryReader
    {
        /// <summary>
        /// A reference to target process.
        /// </summary>
        private Process systemProcess;

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowsAdapter"/> class.
        /// </summary>
        public WindowsMemoryReader()
        {
            // Subscribe to process events
            ProcessAdapterFactory.GetProcessAdapter().Subscribe(this);
        }

        /// <summary>
        /// Gets a reference to the target process. This is an optimization to minimize accesses to the Processes component of the Engine.
        /// </summary>
        public Process SystemProcess
        {
            get
            {
                try
                {
                    if (this.systemProcess?.HasExited == true)
                    {
                        this.systemProcess = null;
                    }
                }
                catch
                {
                }

                return this.systemProcess;
            }

            private set
            {
                this.systemProcess = value;
            }
        }

        /// <summary>
        /// Recieves a process update. This is an optimization over grabbing the process from the <see cref="IProcessAdapter"/> component
        /// of the <see cref="EngineCore"/> every time we need it, which would be cumbersome when doing hundreds of thousands of memory read/writes.
        /// </summary>
        /// <param name="process">The newly selected process.</param>
        public void Update(NormalizedProcess process)
        {
            if (process == null)
            {
                // Avoid setter functions
                this.systemProcess = null;
                return;
            }

            try
            {
                this.SystemProcess = Process.GetProcessById(process.ProcessId);
            }
            catch
            {
                // Avoid setter functions
                this.systemProcess = null;
            }
        }

        /// <summary>
        /// Reads the value of a specified type in the remote process.
        /// </summary>
        /// <param name="dataType">Type of value being read.</param>
        /// <param name="address">The address where the value is read.</param>
        /// <param name="success">Whether or not the read succeeded.</param>
        /// <returns>A value.</returns>
        public Object Read(DataType dataType, IntPtr address, out Boolean success)
        {
            Object value;

            switch (dataType)
            {
                case DataType type when type == DataType.Byte:
                    value = this.Read<Byte>(address, out success);
                    break;
                case DataType type when type == DataType.SByte:
                    value = this.Read<SByte>(address, out success);
                    break;
                case DataType type when type == DataType.Int16:
                    value = this.Read<Int16>(address, out success);
                    break;
                case DataType type when type == DataType.Int32:
                    value = this.Read<Int32>(address, out success);
                    break;
                case DataType type when type == DataType.Int64:
                    value = this.Read<Int64>(address, out success);
                    break;
                case DataType type when type == DataType.UInt16:
                    value = this.Read<UInt16>(address, out success);
                    break;
                case DataType type when type == DataType.UInt32:
                    value = this.Read<UInt32>(address, out success);
                    break;
                case DataType type when type == DataType.UInt64:
                    value = this.Read<UInt64>(address, out success);
                    break;
                case DataType type when type == DataType.Single:
                    value = this.Read<Single>(address, out success);
                    break;
                case DataType type when type == DataType.Double:
                    value = this.Read<Double>(address, out success);
                    break;
                default:
                    value = "?";
                    success = false;
                    break;
            }

            if (!success)
            {
                value = "?";
            }

            return value;
        }

        /// <summary>
        /// Reads the value of a specified type in the remote process.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="address">The address where the value is read.</param>
        /// <param name="success">Whether or not the read succeeded.</param>
        /// <returns>A value.</returns>
        public T Read<T>(IntPtr address, out Boolean success)
        {
            Byte[] byteArray = this.ReadBytes(address, Conversions.SizeOf(typeof(T)), out success);
            return Conversions.BytesToObject<T>(byteArray);
        }

        /// <summary>
        /// Reads an array of bytes in the remote process.
        /// </summary>
        /// <param name="address">The address where the array is read.</param>
        /// <param name="count">The number of cells.</param>
        /// <param name="success">Whether or not the read succeeded.</param>
        /// <returns>The array of bytes.</returns>
        public Byte[] ReadBytes(IntPtr address, Int32 count, out Boolean success)
        {
            return Memory.ReadBytes(this.SystemProcess == null ? IntPtr.Zero : this.SystemProcess.Handle, address, count, out success);
        }

        /// <summary>
        /// Reads a string with a specified encoding in the remote process.
        /// </summary>
        /// <param name="address">The address where the string is read.</param>
        /// <param name="encoding">The encoding used.</param>
        /// <param name="success">Whether or not the read succeeded</param>
        /// <param name="maxLength">[Optional] The number of maximum bytes to read. The string is automatically cropped at this end ('\0' char).</param>
        /// <returns>The string.</returns>
        public String ReadString(IntPtr address, Encoding encoding, out Boolean success, Int32 maxLength = 512)
        {
            // Read the string
            String data = encoding.GetString(this.ReadBytes(address, maxLength, out success));

            // Search the end of the string
            Int32 end = data.IndexOf('\0');

            // Crop the string with this end
            return data.Substring(0, end);
        }

        public IntPtr EvaluatePointer(IntPtr address, IEnumerable<int> offsets)
        {
            IntPtr finalAddress = address;

            if (!offsets.IsNullOrEmpty())
            {
                // Add and trace offsets
                foreach (Int32 offset in offsets.Take(offsets.Count() - 1))
                {
                    if (ProcessAdapterFactory.GetProcessAdapter().IsOpenedProcess32Bit())
                    {
                        finalAddress = (this.Read<UInt32>(finalAddress + offset, out _).ToInt64()).ToIntPtr();
                    }
                    else
                    {
                        finalAddress = (this.Read<UInt64>(finalAddress, out _).ToInt64() + offset).ToIntPtr();
                    }
                }

                // The last offset is added, but not traced
                finalAddress = finalAddress.Add(offsets.Last());
            }

            return finalAddress;
        }
    }
    //// End class
}
//// End namespace