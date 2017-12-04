namespace SqualrStream.Source.Api
{
    using System;
    using System.Net;

    /// <summary>
    /// Class for pinging a url to check if it exists.
    /// </summary>
    internal class PingClient : WebClient
    {
        /// <summary>
        /// Pings the given endpoint.
        /// </summary>
        /// <param name="endpoint">The endpoint to ping.</param>
        public void Ping(String endpoint)
        {
            this.DownloadString(endpoint);
        }

        /// <summary>
        /// Override of <see cref="GetWebRequest(Uri)"/> to replace the request method with HEAD.
        /// </summary>
        /// <param name="address">The uri endpoint.</param>
        /// <returns>The web request.</returns>
        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest request = base.GetWebRequest(address);

            request.Method = "HEAD";

            return request;
        }
    }
    //// End class
}
//// End namespace