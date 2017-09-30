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
        public static String VirtualCurrencyStoreEndpoint = SqualrApi.ApiBase + "Store";

        /// <summary>
        /// The API url to get the access tokens.
        /// </summary>
        public static String AccessTokensApi = SqualrApi.ApiBase + "api/User/Tokens";

        /// <summary>
        /// The API url to get the user.
        /// </summary>
        public static String UserApi = SqualrApi.ApiBase + "api/User";

        /// <summary>
        /// The endpoint for getting all available stream icons.
        /// </summary>
        private static String StreamIconsEndpoint = SqualrApi.ApiBase + "api/Stream/Icons";

        /// <summary>
        /// The endpoint for setting the stream integration configuration.
        /// </summary>
        private static String GetStreamConfigEndpoint = SqualrApi.ApiBase + "api/Stream/Config";

        /// <summary>
        /// The endpoint for setting the stream integration configuration.
        /// </summary>
        private static String SetStreamConfigEndpoint = SqualrApi.ApiBase + "api/Stream/SetConfig";

        /// <summary>
        /// The endpoint for setting the stream integration configuration.
        /// </summary>
        private static String SetCheatStreamMeta = SqualrApi.ApiBase + "api/Stream/SetCheatMeta";

        /// <summary>
        /// The endpoint for updating cheat stream metadata.
        /// </summary>
        private static String UpdateCheatStreamMetaEndpoint = SqualrApi.ApiBase + "api/streamMeta";

        /// <summary>
        /// The endpoint for querying active and unactive cheat ids.
        /// </summary>
        private static String ActiveCheatIdsEndpoint = SqualrApi.ApiBase + "api/Stream/ActiveCheatIds/";

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
        /// The endpoint for connecting to the Twitch service.
        /// </summary>
        private static String ConnectEndpoint = SqualrApi.ApiBase + "api/Stream/Connect";

        /// <summary>
        /// The endpoint for disconnecting to the Twitch service.
        /// </summary>
        private static String DisconnectEndpoint = SqualrApi.ApiBase + "api/Stream/Disconnect";

        /// <summary>
        /// The endpoint for querying the connection status to the Twitch service.
        /// </summary>
        private static String ConnectionStatusEndpoint = SqualrApi.ApiBase + "api/Stream/ConnectionStatusEndpoint";

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

        public static ConnectionStatus Connect(String accessToken)
        {
            Dictionary<String, String> parameters = new Dictionary<String, String>();
            parameters.Add("access_token", accessToken);

            return SqualrApi.ExecuteRequest<ConnectionStatus>(Method.POST, SqualrApi.ConnectEndpoint, parameters);
        }

        public static ConnectionStatus Disconnect(String accessToken)
        {
            Dictionary<String, String> parameters = new Dictionary<String, String>();
            parameters.Add("access_token", accessToken);

            return SqualrApi.ExecuteRequest<ConnectionStatus>(Method.POST, SqualrApi.DisconnectEndpoint, parameters);
        }

        public static ConnectionStatus GetConnectionStatus(String accessToken)
        {
            Dictionary<String, String> parameters = new Dictionary<String, String>();
            parameters.Add("access_token", accessToken);

            return SqualrApi.ExecuteRequest<ConnectionStatus>(Method.POST, SqualrApi.ConnectionStatusEndpoint, parameters);
        }

        private static StreamIcon[] cachedIcons = null;

        public static StreamIcon[] GetStreamIcons()
        {
            if (cachedIcons == null)
            {
                cachedIcons = SqualrApi.ExecuteRequest<StreamIcon[]>(Method.GET, SqualrApi.StreamIconsEndpoint);
            }

            return cachedIcons;
        }

        public static IEnumerable<CheatVotes> GetStreamActivationIds(String twitchChannel)
        {
            return SqualrApi.ExecuteRequest<IEnumerable<CheatVotes>>(Method.GET, SqualrApi.ActiveCheatIdsEndpoint + twitchChannel);
        }

        public static IEnumerable<Game> GetGameList()
        {
            return SqualrApi.ExecuteRequest<Game[]>(Method.GET, SqualrApi.GameListEndpoint);
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
            parameters.Add("cooldown", cheat?.Cooldown.ToString());
            parameters.Add("duration", cheat?.Duration.ToString());
            parameters.Add("icon", cheat?.Icon?.ToString());

            SqualrApi.ExecuteRequest(Method.PUT, SqualrApi.UpdateCheatStreamMetaEndpoint + "/" + cheat?.CheatId.ToString(), parameters);
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