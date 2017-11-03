namespace SqualrStream.Source.Api
{
    using RestSharp;
    using SqualrStream.Source.Api.Exceptions;
    using SqualrStream.Source.Api.Models;
    using SqualrCore.Source.Output;
    using SqualrCore.Source.Utils.Extensions;
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
        public static String VirtualCurrencyStoreEndpoint = SqualrApi.ApiBase + "store";

        /// <summary>
        /// The API url to get the access tokens.
        /// </summary>
        public static String AccessTokensApi = SqualrApi.ApiBase + "api/user/tokens";

        /// <summary>
        /// The API url to get the user.
        /// </summary>
        public static String UserApi = SqualrApi.ApiBase + "api/user";

        /// <summary>
        /// The endpoint for getting all available stream icons.
        /// </summary>
        private static String StreamIconsEndpoint = SqualrApi.ApiBase + "api/stream/icons";

        /// <summary>
        /// The endpoint for setting the stream integration configuration.
        /// </summary>
        private static String GetStreamConfigEndpoint = SqualrApi.ApiBase + "api/stream/config";

        /// <summary>
        /// The endpoint for setting the stream integration configuration.
        /// </summary>
        private static String OverlayConfigEndpoint = SqualrApi.ApiBase + "api/stream/overlay";

        /// <summary>
        /// The endpoint for updating cheat stream metadata.
        /// </summary>
        private static String UpdateCheatStreamMetaEndpoint = SqualrApi.ApiBase + "api/streamMeta";

        /// <summary>
        /// The endpoint for querying active and unactive cheat ids.
        /// </summary>
        private static String ActiveCheatIdsEndpoint = SqualrApi.ApiBase + "api/stream/activeCheatIds";

        /// <summary>
        /// The endpoint for querying the game lists.
        /// </summary>
        private static String GameListEndpoint = SqualrApi.ApiBase + "api/games/list";

        /// <summary>
        /// The endpoint for querying the available game lists.
        /// </summary>
        private static String GameAvailableEndpoint = SqualrApi.ApiBase + "api/games/available";

        /// <summary>
        /// The endpoint for querying the games for which a user has unlocked or uploaded at least one cheat.
        /// </summary>
        private static String OwnedGamesEndpoint = SqualrApi.ApiBase + "api/games/owned";

        /// <summary>
        /// The endpoint for searching games.
        /// </summary>
        private static String GameSearchEndpoint = SqualrApi.ApiBase + "api/games/search";

        /// <summary>
        /// The endpoint for querying the library lists.
        /// </summary>
        private static String LibraryEndpoint = SqualrApi.ApiBase + "api/library";

        /// <summary>
        /// The endpoint for querying the available and unavailable cheats in a library.
        /// </summary>
        private static String CheatsEndpoint = SqualrApi.ApiBase + "api/cheat";

        /// <summary>
        /// The endpoint for querying the available and unavailable cheats in the store.
        /// </summary>
        private static String StoreEndpoint = SqualrApi.ApiBase + "api/store";

        /// <summary>
        /// The endpoint for interacting with the Twitch service.
        /// </summary>
        private static String TwitchConnectionEndpoint = SqualrApi.ApiBase + "api/twitch";

        /// <summary>
        /// The active API base depending on the environment.
        /// </summary>
        private static String apiBase;

        public static AccessTokens GetAccessTokens(String code)
        {
            Dictionary<String, String> parameters = new Dictionary<String, String>();
            parameters.Add("code", code);

            return ExecuteRequest<AccessTokens>(Method.GET, SqualrApi.AccessTokensApi, parameters);
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

            return SqualrApi.ExecuteRequest<User>(Method.GET, SqualrApi.UserApi, parameters);
        }

        public static String Connect(String accessToken)
        {
            Dictionary<String, String> parameters = new Dictionary<String, String>();
            parameters.Add("access_token", accessToken);

            return SqualrApi.ExecuteRequest<String>(Method.GET, SqualrApi.TwitchConnectionEndpoint + "/create", parameters);
        }

        private static IEnumerable<StreamIcon> cachedIcons = null;

        public static IEnumerable<StreamIcon> GetStreamIcons()
        {
            if (cachedIcons == null)
            {
                cachedIcons = SqualrApi.ExecuteRequest<StreamIcon[]>(Method.GET, SqualrApi.StreamIconsEndpoint).Shuffle();
            }

            return cachedIcons;
        }

        public static IEnumerable<CheatVotes> GetStreamActivationIds(String accessToken)
        {
            Dictionary<String, String> parameters = new Dictionary<String, String>();
            parameters.Add("access_token", accessToken);

            return SqualrApi.ExecuteRequest<IEnumerable<CheatVotes>>(Method.GET, SqualrApi.ActiveCheatIdsEndpoint, parameters);
        }

        public static IEnumerable<Game> GetGameList()
        {
            return SqualrApi.ExecuteRequest<Game[]>(Method.GET, SqualrApi.GameAvailableEndpoint);
        }

        public static IEnumerable<Game> GetOwnedGameList(String accessToken)
        {
            Dictionary<String, String> parameters = new Dictionary<String, String>();
            parameters.Add("access_token", accessToken);

            return SqualrApi.ExecuteRequest<Game[]>(Method.GET, SqualrApi.OwnedGamesEndpoint, parameters);
        }

        public static IEnumerable<Game> SearchGameList(String searchTerm)
        {
            Dictionary<String, String> parameters = new Dictionary<String, String>();
            parameters.Add("search_term", searchTerm);

            return SqualrApi.ExecuteRequest<Game[]>(Method.GET, SqualrApi.GameSearchEndpoint, parameters);
        }

        public static IEnumerable<Library> GetLibraries(String accessToken, Int32 gameId)
        {
            Dictionary<String, String> parameters = new Dictionary<String, String>();
            parameters.Add("access_token", accessToken);
            parameters.Add("game_id", gameId.ToString());

            return SqualrApi.ExecuteRequest<Library[]>(Method.GET, SqualrApi.LibraryEndpoint, parameters);
        }

        public static void SetActiveLibrary(String accessToken, Int32 libraryId)
        {
            Dictionary<String, String> parameters = new Dictionary<String, String>();
            parameters.Add("access_token", accessToken);
            parameters.Add("library_id", libraryId.ToString());

            SqualrApi.ExecuteRequest(Method.GET, SqualrApi.LibraryEndpoint + "/" + libraryId.ToString() + "/edit", parameters);
        }

        public static void RenameLibrary(String accessToken, Int32 libraryId, String libraryName)
        {
            Dictionary<String, String> parameters = new Dictionary<String, String>();
            parameters.Add("access_token", accessToken);
            parameters.Add("library_id", libraryId.ToString());
            parameters.Add("library_name", libraryName);

            SqualrApi.ExecuteRequest(Method.PUT, SqualrApi.LibraryEndpoint + "/" + libraryId.ToString(), parameters);
        }

        public static Library CreateLibrary(String accessToken, Int32 gameId)
        {
            Dictionary<String, String> parameters = new Dictionary<String, String>();
            parameters.Add("access_token", accessToken);
            parameters.Add("game_id", gameId.ToString());

            return SqualrApi.ExecuteRequest<Library>(Method.GET, SqualrApi.LibraryEndpoint + "/create", parameters);
        }

        public static void DeleteLibrary(String accessToken, Int32 libraryId)
        {
            Dictionary<String, String> parameters = new Dictionary<String, String>();
            parameters.Add("access_token", accessToken);
            parameters.Add("library_id", libraryId.ToString());

            SqualrApi.ExecuteRequest(Method.DELETE, SqualrApi.LibraryEndpoint + "/" + libraryId.ToString(), parameters);
        }

        public static StoreCheats GetStoreCheats(String accessToken, Int32 gameId)
        {
            Dictionary<String, String> parameters = new Dictionary<String, String>();
            parameters.Add("access_token", accessToken);
            parameters.Add("game_id", gameId.ToString());

            return SqualrApi.ExecuteRequest<StoreCheats>(Method.GET, SqualrApi.StoreEndpoint, parameters);
        }

        public static UnlockedCheat UnlockCheat(String accessToken, Int32 cheatId)
        {
            Dictionary<String, String> parameters = new Dictionary<String, String>();
            parameters.Add("access_token", accessToken);
            parameters.Add("cheat_id", cheatId.ToString());

            return SqualrApi.ExecuteRequest<UnlockedCheat>(Method.GET, SqualrApi.StoreEndpoint + "/create", parameters);
        }

        public static LibraryCheats GetCheats(String accessToken, Int32 libraryId)
        {
            Dictionary<String, String> parameters = new Dictionary<String, String>();
            parameters.Add("access_token", accessToken);
            parameters.Add("library_id", libraryId.ToString());

            return SqualrApi.ExecuteRequest<LibraryCheats>(Method.GET, SqualrApi.CheatsEndpoint, parameters);
        }

        public static Cheat AddCheatToLibrary(String accessToken, Int32 libraryId, Int32 cheatId)
        {
            Dictionary<String, String> parameters = new Dictionary<String, String>();
            parameters.Add("access_token", accessToken);
            parameters.Add("library_id", libraryId.ToString());
            parameters.Add("cheat_id", cheatId.ToString());

            return SqualrApi.ExecuteRequest<Cheat>(Method.GET, SqualrApi.CheatsEndpoint + "/create", parameters);
        }

        public static Cheat RemoveCheatFromLibrary(String accessToken, Int32 libraryId, Int32 cheatId)
        {
            Dictionary<String, String> parameters = new Dictionary<String, String>();
            parameters.Add("access_token", accessToken);
            parameters.Add("library_id", libraryId.ToString());
            parameters.Add("cheat_id", cheatId.ToString());

            return SqualrApi.ExecuteRequest<Cheat>(Method.DELETE, SqualrApi.CheatsEndpoint + "/" + cheatId.ToString(), parameters);
        }

        public static void UpdateCheatStreamMeta(String accessToken, Cheat cheat)
        {
            Dictionary<String, String> parameters = new Dictionary<String, String>();
            parameters.Add("access_token", accessToken);
            parameters.Add("cheat_id", cheat?.CheatId.ToString());

            parameters.Add("is_stream_disabled", ((cheat?.IsStreamDisabled ?? false) ? 1 : 0).ToString());
            parameters.Add("cooldown", cheat?.CooldownMax.ToString());
            parameters.Add("duration", cheat?.DurationMax.ToString());
            parameters.Add("icon", cheat?.IconName?.ToString());

            SqualrApi.ExecuteRequest(Method.PUT, SqualrApi.UpdateCheatStreamMetaEndpoint + "/" + cheat?.CheatId.ToString(), parameters);
        }

        public static void UpdateOverlayMeta(String accessToken, OverlayMeta[] activeCooldowns)
        {
            Dictionary<String, String> parameters = new Dictionary<String, String>();
            parameters.Add("access_token", accessToken);

            using (MemoryStream memoryStream = new MemoryStream())
            {
                DataContractJsonSerializer deserializer = new DataContractJsonSerializer(typeof(OverlayMeta[]));

                deserializer.WriteObject(memoryStream, activeCooldowns);

                parameters.Add("active_cooldowns", Encoding.ASCII.GetString(memoryStream.ToArray()));
            }

            SqualrApi.ExecuteRequest(Method.GET, SqualrApi.OverlayConfigEndpoint + "/create", parameters);
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

        /// <summary>
        /// Executes the given REST request.
        /// </summary>
        /// <param name="method">The HTTP method.</param>
        /// <param name="endpoint">The REST endpoint.</param>
        /// <param name="parameters">The request parameters.</param>
        /// <returns>The response content.</returns>
        private static void ExecuteRequest(Method method, String endpoint, Dictionary<String, String> parameters = null)
        {
            SqualrApi.ExecuteRequest<String>(method, endpoint, parameters);
        }

        /// <summary>
        /// Executes the given REST request.
        /// </summary>
        /// <param name="method">The HTTP method.</param>
        /// <param name="endpoint">The REST endpoint.</param>
        /// <param name="parameters">The request parameters.</param>
        /// <returns>The response content.</returns>
        private static T ExecuteRequest<T>(Method method, String endpoint, Dictionary<String, String> parameters = null) where T : class
        {
            RestClient client = new RestClient(endpoint);
            RestRequest request = new RestRequest(method);

            if (!parameters.IsNullOrEmpty())
            {
                foreach (KeyValuePair<String, String> parameter in parameters)
                {
                    request.AddParameter(parameter.Key, parameter.Value);
                }
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

            if (typeof(T) == typeof(String))
            {
                return response?.Content as T;
            }

            using (MemoryStream memoryStream = new MemoryStream(Encoding.ASCII.GetBytes(response?.Content)))
            {
                DataContractJsonSerializer deserializer = new DataContractJsonSerializer(typeof(T));

                return deserializer.ReadObject(memoryStream) as T;
            }
        }
    }
    //// End class
}
//// End namespace