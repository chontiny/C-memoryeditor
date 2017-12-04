namespace SqualrStream.Source.Api.Exceptions
{
    using System;

    internal class NotFoundException : Exception
    {
        public NotFoundException(String uri) : base("Requested Resource not Found: " + uri)
        {
        }
    }
    //// End class
}
//// End namespace