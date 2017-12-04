namespace SqualrStream.Source.Api.Exceptions
{
    using RestSharp;
    using System;

    internal class ResponseStatusException : Exception
    {
        public ResponseStatusException(IRestResponse response) : base(
            "Status: " + response.ResponseStatus.ToString() +
            response.ErrorException != null ? (Environment.NewLine + "Exception: " + response.ErrorException) : String.Empty +
            response.ErrorMessage != null ? (Environment.NewLine + "Message: " + response.ErrorMessage) : String.Empty)
        {
        }
    }
    //// End class
}
//// End namespace