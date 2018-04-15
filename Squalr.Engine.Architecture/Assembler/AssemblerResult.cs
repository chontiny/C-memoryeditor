namespace Squalr.Engine.Architecture.Assembler
{
    using System;

    public class AssemblerResult
    {
        /// <summary>
        /// Contains the results of an assembler operation.
        /// </summary>
        public AssemblerResult()
        {
            this.Data = null;
            this.Message = null;
            this.InnerMessage = null;
        }

        /// <summary>
        /// Contains the results of an assembler operation.
        /// </summary>
        /// <param name="data">The compiled assembly.</param>
        /// <param name="message">The message of the compilation result.</param>
        /// <param name="innerMessage">The inner message of the compilation result.</param>
        public AssemblerResult(Byte[] data, String message, String innerMessage)
        {
            this.Data = data;
            this.Message = message;
            this.InnerMessage = innerMessage;
        }

        /// <summary>
        /// The compiled assembly.
        /// </summary>
        public Byte[] Data { get; set; }

        /// <summary>
        /// The message of the compilation result.
        /// </summary>
        public String Message { get; set; }

        /// <summary>
        /// The inner message of the compilation result. Usually contains error data.
        /// </summary>
        public String InnerMessage { get; set; }
    }
    //// End class
}
