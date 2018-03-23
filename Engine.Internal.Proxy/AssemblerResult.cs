namespace SqualrProxy
{
    using System;

    [Serializable()]
    public class AssemblerResult
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="message"></param>
        /// <param name="innerMessage"></param>
        public AssemblerResult(Byte[] data, String message, String innerMessage)
        {
            this.Data = data;
            this.Message = message;
            this.InnerMessage = innerMessage;
        }

        /// <summary>
        /// 
        /// </summary>
        public Byte[] Data { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public String Message { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public String InnerMessage { get; set; }
    }
    //// End class
}
//// End namespace