namespace Squalr.Source.Api
{
    using RestSharp;
    using Squalr.Source.Api.Exceptions;
    using Squalr.Source.Api.Models;
    using Squalr.Source.Output;
    using Squalr.Source.Utils.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Deployment.Application;
    using System.IO;
    using System.Net;
    using System.Runtime.Serialization.Json;
    using System.Text;

    internal static class SqualrApi
    {
        /// <summary>
        /// The base API url for Squalr.
        /// </summary>
        public const String SqualrHostBase = "https://www.squalr.com/";

        /// <summary>
        /// The base API url for Squalr development environments.
        /// </summary>
        public const String LocalHostBase = "http://localhost/";

        /// <summary>
        /// The API url to get the access tokens.
        /// </summary>
        public static String VirtualCurrencyStoreEndpoint = SqualrApi.ApiBase + "Account/Manage/BuyCoins";

        /// <summary>
        /// The API url to get the access tokens.
        /// </summary>
        public static String AccessTokensApi = SqualrApi.ApiBase + "api/User/Tokens";

        /// <summary>
        /// The API url to get the user.
        /// </summary>
        public static String UserApi = SqualrApi.ApiBase + "api/User";

        /// <summary>
        /// The endpoint for querying active and unactive cheat ids.
        /// </summary>
        private static String ActiveCheatIdsEndpoint = SqualrApi.ApiBase + "api/ActiveCheatIds/";

        /// <summary>
        /// The endpoint for querying the game lists.
        /// </summary>
        private static String GameListEndpoint = SqualrApi.ApiBase + "api/Games/List";

        /// <summary>
        /// The endpoint for querying the games for which a user has unlocked or uploaded at least one cheat.
        /// </summary>
        private static String OwnedGamesEndpoint = SqualrApi.ApiBase + "api/Games/Owned";

        /// <summary>
        /// The endpoint for searching games.
        /// </summary>
        private static String GameSearchEndpoint = SqualrApi.ApiBase + "api/Games/Search";

        /// <summary>
        /// The endpoint for querying the library lists.
        /// </summary>
        private static String LibraryListEndpoint = SqualrApi.ApiBase + "api/Library";

        /// <summary>
        /// The endpoint for querying the available and unavailable cheats in a library.
        /// </summary>
        private static String LibraryCheatsEndpoint = SqualrApi.ApiBase + "api/Cheats";

        /// <summary>
        /// The endpoint for querying the available and unavailable cheats in the store.
        /// </summary>
        private static String StoreCheatsEndpoint = SqualrApi.ApiBase + "api/Cheats/Store";

        /// <summary>
        /// The endpoint for unlocking cheats.
        /// </summary>
        private static String UnlockCheatEndpoint = SqualrApi.ApiBase + "api/Cheats/Unlock";

        /// <summary>
        /// The active API base depending on the environment.
        /// </summary>
        private static String apiBase;

        public static AccessTokens GetAccessTokens(String code)
        {
            Dictionary<String, String> parameters = new Dictionary<String, String>();
            parameters.Add("code", code);

            String result = ExecuteRequest(Method.GET, SqualrApi.AccessTokensApi, parameters);

            using (MemoryStream memoryStream = new MemoryStream(Encoding.ASCII.GetBytes(result)))
            {
                DataContractJsonSerializer deserializer = new DataContractJsonSerializer(typeof(AccessTokens));

                return deserializer.ReadObject(memoryStream) as AccessTokens;
            }
        }

        /// <summary>
        /// Gets or sets the active API base depending on the environment.
        /// </summary>
        private static String ApiBase
        {
            get
            {
                if (SqualrApi.apiBase.IsNullOrEmpty())
                {
                    SqualrApi.ApiBase = SqualrApi.InitializeApiBase();
                }

                return SqualrApi.apiBase;
            }

            set
            {
                SqualrApi.apiBase = value;
            }
        }

        public static User GetTwitchUser(String accessToken)
        {
            Dictionary<String, String> parameters = new Dictionary<String, String>();
            parameters.Add("access_token", accessToken);

            String result = ExecuteRequest(Method.GET, SqualrApi.UserApi, parameters);

            using (MemoryStream memoryStream = new MemoryStream(Encoding.ASCII.GetBytes(result)))
            {
                DataContractJsonSerializer deserializer = new DataContractJsonSerializer(typeof(User));

                return deserializer.ReadObject(memoryStream) as User;
            }
        }

        public static StreamActivationIds GetStreamActivationIds(String twitchChannel)
        {
            Dictionary<String, String> parameters = new Dictionary<String, String>();

            String result = ExecuteRequest(Method.GET, SqualrApi.ActiveCheatIdsEndpoint + twitchChannel, parameters);

            using (MemoryStream memoryStream = new MemoryStream(Encoding.ASCII.GetBytes(result)))
            {
                DataContractJsonSerializer deserializer = new DataContractJsonSerializer(typeof(StreamActivationIds));

                return deserializer.ReadObject(memoryStream) as StreamActivationIds;
            }
        }

        public static Game[] GetGameList()
        {
            Dictionary<String, String> parameters = new Dictionary<String, String>();

            String result = ExecuteRequest(Method.GET, SqualrApi.GameListEndpoint, parameters);

            using (MemoryStream memoryStream = new MemoryStream(Encoding.ASCII.GetBytes(result)))
            {
                DataContractJsonSerializer deserializer = new DataContractJsonSerializer(typeof(Game[]));

                return deserializer.ReadObject(memoryStream) as Game[];
            }
        }

        public static Game[] GetOwnedGames()
        {
            Dictionary<String, String> parameters = new Dictionary<String, String>();

            String result = ExecuteRequest(Method.GET, SqualrApi.OwnedGamesEndpoint, parameters);

            using (MemoryStream memoryStream = new MemoryStream(Encoding.ASCII.GetBytes(result)))
            {
                DataContractJsonSerializer deserializer = new DataContractJsonSerializer(typeof(Game[]));

                return deserializer.ReadObject(memoryStream) as Game[];
            }
        }

        public static Game[] GetOwnedGameList(String accessToken)
        {
            Dictionary<String, String> parameters = new Dictionary<String, String>();
            parameters.Add("access_token", accessToken);

            String result = ExecuteRequest(Method.GET, SqualrApi.OwnedGamesEndpoint, parameters);

            using (MemoryStream memoryStream = new MemoryStream(Encoding.ASCII.GetBytes(result)))
            {
                DataContractJsonSerializer deserializer = new DataContractJsonSerializer(typeof(Game[]));

                return deserializer.ReadObject(memoryStream) as Game[];
            }
        }

        public static Game[] SearchGameList(String searchTerm)
        {
            Dictionary<String, String> parameters = new Dictionary<String, String>();
            parameters.Add("search_term", searchTerm);

            String result = ExecuteRequest(Method.GET, SqualrApi.GameSearchEndpoint, parameters);

            using (MemoryStream memoryStream = new MemoryStream(Encoding.ASCII.GetBytes(result)))
            {
                DataContractJsonSerializer deserializer = new DataContractJsonSerializer(typeof(Game[]));

                return deserializer.ReadObject(memoryStream) as Game[];
            }
        }

        public static Library[] GetLibraryList(String accessToken, Int32 gameId)
        {
            Dictionary<String, String> parameters = new Dictionary<String, String>();
            parameters.Add("access_token", accessToken);
            parameters.Add("game_id", gameId.ToString());

            String result = ExecuteRequest(Method.GET, SqualrApi.LibraryListEndpoint, parameters);

            using (MemoryStream memoryStream = new MemoryStream(Encoding.ASCII.GetBytes(result)))
            {
                DataContractJsonSerializer deserializer = new DataContractJsonSerializer(typeof(Library[]));

                return deserializer.ReadObject(memoryStream) as Library[];
            }
        }

        public static StoreCheats GetCheatList(String accessToken, Int32 gameId)
        {
            Dictionary<String, String> parameters = new Dictionary<String, String>();
            parameters.Add("access_token", accessToken);
            parameters.Add("game_id", gameId.ToString());

            String result = ExecuteRequest(Method.GET, SqualrApi.StoreCheatsEndpoint, parameters);

            using (MemoryStream memoryStream = new MemoryStream(Encoding.ASCII.GetBytes(result)))
            {
                DataContractJsonSerializer deserializer = new DataContractJsonSerializer(typeof(StoreCheats));

                return deserializer.ReadObject(memoryStream) as StoreCheats;
            }
        }

        public static Cheat UnlockCheat(String accessToken, Int32 cheatId)
        {
            Dictionary<String, String> parameters = new Dictionary<String, String>();
            parameters.Add("access_token", accessToken);
            parameters.Add("cheat_id", cheatId.ToString());

            String result = ExecuteRequest(Method.POST, SqualrApi.UnlockCheatEndpoint, parameters);

            using (MemoryStream memoryStream = new MemoryStream(Encoding.ASCII.GetBytes(result)))
            {
                DataContractJsonSerializer deserializer = new DataContractJsonSerializer(typeof(Cheat));

                return deserializer.ReadObject(memoryStream) as Cheat;
            }
        }

        /// <summary>
        /// Gets the base url for api calls. In development environments this will be a local endpoint.
        /// </summary>
        /// <returns>The api base url.</returns>
        private static String InitializeApiBase()
        {
            // Check if development environment API endpoint available
            try
            {
                if (!ApplicationDeployment.IsNetworkDeployed)
                {
                    using (PingClient client = new PingClient())
                    {
                        Uri uri = new Uri(SqualrApi.LocalHostBase);

                        try
                        {
                            client.Ping(uri.GetLeftPart(UriPartial.Authority));

                            return SqualrApi.LocalHostBase;
                        }
                        catch (Exception ex)
                        {
                            OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Warn, "No local Squalr site found. Using production for API queries", ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Info, "Unable to detect environment for API calls.", ex);
            }

            // None found, use the actual API base
            return SqualrApi.SqualrHostBase;
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