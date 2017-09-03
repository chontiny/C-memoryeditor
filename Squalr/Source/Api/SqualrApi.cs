namespace Squalr.Source.Api
{
    using RestSharp;
    using Squalr.Source.Api.Models;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.Serialization.Json;
    using System.Text;

    internal static class SqualrApi
    {
        /// <summary>
        /// The API url to get the twitch auth tokens.
        /// </summary>
        public const String TwitchTokenApi = "https://www.squalr.com/api/Client/TwitchTokens";

        public static TwitchTokens GetTwitchTokens(String code)
        {
            Dictionary<String, String> parameters = new Dictionary<String, String>();
            String result = ExecuteRequest(Method.GET, SqualrApi.TwitchTokenApi + "?code=" + code, "code=" + code);

            using (MemoryStream memoryStream = new MemoryStream(Encoding.ASCII.GetBytes(result)))
            {
                DataContractJsonSerializer deserializer = new DataContractJsonSerializer(typeof(TwitchTokens));

                return deserializer.ReadObject(memoryStream) as TwitchTokens;
            }
        }

        private static String ExecuteRequest(Method method, String endpoint, String parameters)
        {
            RestClient client = new RestClient(endpoint);
            RestRequest request = new RestRequest(Method.GET);

            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            request.AddParameter("application/x-www-form-urlencoded", parameters, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);

            return response?.Content;
        }
    }
    //// End class
}
//// End namespace