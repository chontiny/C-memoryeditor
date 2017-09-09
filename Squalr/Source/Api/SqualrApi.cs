namespace Squalr.Source.Api
{
    using RestSharp;
    using Squalr.Source.Api.Exceptions;
    using Squalr.Source.Api.Models;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Runtime.Serialization.Json;
    using System.Text;

    internal static class SqualrApi
    {
        public const String ApiBase = "https://www.squalr.com/api";

        /// <summary>
        /// The API url to get the twitch auth tokens.
        /// </summary>
        public const String TwitchTokenApi = SqualrApi.ApiBase + "/TwitchTokens";

        /// <summary>
        /// The API url to get the twitch user.
        /// </summary>
        public const String TwitchUserApi = SqualrApi.ApiBase + "/TwitchUser";

        /// <summary>
        /// The endpoint for querying active and unactive cheat ids.
        /// </summary>
        private const String ActiveCheatIdsEndpoint = SqualrApi.ApiBase + "/ActiveCheatIds/";

        /// <summary>
        /// The endpoint for querying the game lists.
        /// </summary>
        private const String GameListEndpoint = SqualrApi.ApiBase + "/Games/List";

        /// <summary>
        /// The endpoint for searching games.
        /// </summary>
        private const String GameSearchEndpoint = SqualrApi.ApiBase + "/Games/Search";

        /// <summary>
        /// The endpoint for querying the library lists.
        /// </summary>
        private const String LibraryListEndpoint = SqualrApi.ApiBase + "/Library";

        /// <summary>
        /// The endpoint for querying the available and unavailable cheats cheats.
        /// </summary>
        private const String AvailableCheatsEndpoint = SqualrApi.ApiBase + "/Cheats";

        public static TwitchAccessTokens GetTwitchTokens(String code)
        {
            Dictionary<String, String> parameters = new Dictionary<String, String>();
            parameters.Add("code", code);

            String result = ExecuteRequest(Method.GET, SqualrApi.TwitchTokenApi, parameters);

            using (MemoryStream memoryStream = new MemoryStream(Encoding.ASCII.GetBytes(result)))
            {
                DataContractJsonSerializer deserializer = new DataContractJsonSerializer(typeof(TwitchAccessTokens));

                return deserializer.ReadObject(memoryStream) as TwitchAccessTokens;
            }
        }

        public static TwitchUser GetTwitchUser(String accessToken)
        {
            Dictionary<String, String> parameters = new Dictionary<String, String>();
            parameters.Add("accessToken", accessToken);

            String result = ExecuteRequest(Method.GET, SqualrApi.TwitchUserApi, parameters);

            using (MemoryStream memoryStream = new MemoryStream(Encoding.ASCII.GetBytes(result)))
            {
                DataContractJsonSerializer deserializer = new DataContractJsonSerializer(typeof(TwitchUser));

                return deserializer.ReadObject(memoryStream) as TwitchUser;
            }
        }

        public static StreamActivationIds GetStreamActivationIds(String twitchChannel)
        {
            String endpoint = SqualrApi.ActiveCheatIdsEndpoint + twitchChannel;

            using (WebClient webclient = new WebClient())
            {
                using (MemoryStream memoryStream = new MemoryStream(webclient.DownloadData(endpoint)))
                {
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(StreamActivationIds));
                    StreamActivationIds streamActivationIds = serializer.ReadObject(memoryStream) as StreamActivationIds;

                    return streamActivationIds;
                }
            }
        }

        public static Game[] GetGameList()
        {
            String endpoint = SqualrApi.GameListEndpoint;

            using (WebClient webclient = new WebClient())
            {
                using (MemoryStream memoryStream = new MemoryStream(webclient.DownloadData(endpoint)))
                {
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Game[]));
                    Game[] games = serializer.ReadObject(memoryStream) as Game[];

                    return games;
                }
            }
        }

        public static Game[] SearchGameList(String searchTerm)
        {
            String endpoint = SqualrApi.GameSearchEndpoint;
            Dictionary<String, String> parameters = new Dictionary<String, String>();
            parameters.Add("search_term", searchTerm);

            using (WebClient webclient = new WebClient())
            {
                using (MemoryStream memoryStream = new MemoryStream(webclient.DownloadData(endpoint)))
                {
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Game[]));
                    Game[] games = serializer.ReadObject(memoryStream) as Game[];

                    return games;
                }
            }
        }

        public static Library[] GetLibraryList(Int32 gameId)
        {
            String endpoint = SqualrApi.LibraryListEndpoint;
            Dictionary<String, String> parameters = new Dictionary<String, String>();
            parameters.Add("game_id", gameId.ToString());

            using (WebClient webclient = new WebClient())
            {
                using (MemoryStream memoryStream = new MemoryStream(webclient.DownloadData(endpoint)))
                {
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Library[]));
                    Library[] libraries = serializer.ReadObject(memoryStream) as Library[];

                    return libraries;
                }
            }
        }

        public static Library[] GetCheatList(Int32 libraryId)
        {
            String endpoint = SqualrApi.LibraryListEndpoint;
            Dictionary<String, String> parameters = new Dictionary<String, String>();
            parameters.Add("library_id", libraryId.ToString());

            using (WebClient webclient = new WebClient())
            {
                using (MemoryStream memoryStream = new MemoryStream(webclient.DownloadData(endpoint)))
                {
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Library[]));
                    Library[] libraries = serializer.ReadObject(memoryStream) as Library[];

                    return libraries;
                }
            }
        }

        private static String ExecuteRequest(Method method, String endpoint, Dictionary<String, String> parameters)
        {
            RestClient client = new RestClient(endpoint);
            RestRequest request = new RestRequest(method);

            foreach (KeyValuePair<String, String> parameter in parameters)
            {
                request.AddParameter(parameter.Key, parameter.Value);
            }

            IRestResponse response = client.Execute(request);

            if (response.ResponseStatus != ResponseStatus.Completed)
            {
                throw new ResponseStatusException(response);
            }

            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                    break;
                default:
                    throw new StatusException(response.ResponseUri, response.StatusCode);
            }

            return response?.Content;
        }
    }
    //// End class
}
//// End namespace