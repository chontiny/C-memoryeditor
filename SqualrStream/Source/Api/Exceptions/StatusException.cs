namespace SqualrStream.Source.Api.Exceptions
{
    using System;
    using System.Net;

    internal class StatusException : Exception
    {
        public StatusException(Uri uri, HttpStatusCode statusCode) : base(
            "URL: " + uri?.ToString() + Environment.NewLine +
            "Status: " + statusCode.ToString())
        {
        }
    }
    //// End class
}
//// End namespace