namespace Squalr.Source.Browse.Library
{
    using Squalr.Properties;
    using Squalr.Source.Api;
    using Squalr.Source.Api.Models;
    using Squalr.Source.Docking;
    using Squalr.Source.Main;
    using Squalr.Source.Output;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// View model for the Library.
    /// </summary>
    internal class LibraryViewModel : ToolViewModel
    {
        /// <summary>
        /// The content id for the docking library associated with this view model.
        /// </summary>
        public const String ToolContentId = nameof(LibraryViewModel);

        /// <summary>
        /// Singleton instance of the <see cref="StoreViewModel" /> class.
        /// </summary>
        private static Lazy<LibraryViewModel> libraryViewModelInstance = new Lazy<LibraryViewModel>(
                () => { return new LibraryViewModel(); },
                LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// The list of games.
        /// </summary>
        private IEnumerable<Game> gameList;

        /// <summary>
        /// Prevents a default instance of the <see cref="LibraryViewModel" /> class from being created.
        /// </summary>
        private LibraryViewModel() : base("Library")
        {
            this.ContentId = LibraryViewModel.ToolContentId;

            Task.Run(() =>
            {
                AccessTokens accessTokens = SettingsViewModel.GetInstance().AccessTokens;

                try
                {
                    this.GameList = SqualrApi.GetOwnedGameList(accessTokens?.AccessToken);
                }
                catch (Exception ex)
                {
                    OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Error, "Error fetching game list", ex);
                }
            });

            MainViewModel.GetInstance().RegisterTool(this);
        }

        /// <summary>
        /// Gets a singleton instance of the <see cref="LibraryViewModel"/> class.
        /// </summary>
        /// <returns>A singleton instance of the class.</returns>
        public static LibraryViewModel GetInstance()
        {
            return libraryViewModelInstance.Value;
        }

        /// <summary>
        /// Gets the list of games.
        /// </summary>
        public IEnumerable<Game> GameList
        {
            get
            {
                return this.gameList;
            }

            private set
            {
                this.gameList = value;
                this.RaisePropertyChanged(nameof(this.GameList));
            }
        }
    }
    //// End class
}
//// End namespace