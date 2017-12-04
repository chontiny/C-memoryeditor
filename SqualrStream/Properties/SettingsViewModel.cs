namespace SqualrStream.Properties
{
    using SqualrCore.Source.Docking;
    using SqualrStream.Source.Api.Models;
    using System;
    using System.IO;
    using System.Runtime.Serialization.Json;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// View model for the Settings.
    /// </summary>
    internal class SettingsViewModel : ToolViewModel
    {
        /// <summary>
        /// Singleton instance of the <see cref="SettingsViewModel"/> class.
        /// </summary>
        private static Lazy<SettingsViewModel> settingsViewModelInstance = new Lazy<SettingsViewModel>(
                () => { return new SettingsViewModel(); },
                LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// Settings that control the degree of parallelism for multithreaded tasks.
        /// </summary>
        private static Lazy<ParallelOptions> parallelSettingsFast = new Lazy<ParallelOptions>(
                () =>
                {
                    ParallelOptions parallelOptions = new ParallelOptions()
                    {
                        // Only use 75% of available processing power, as not to interfere with other programs
                        MaxDegreeOfParallelism = (Environment.ProcessorCount * 3) / 4
                    };
                    return parallelOptions;
                },
                LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// Settings that control the degree of parallelism for multithreaded tasks.
        /// </summary>
        private static Lazy<ParallelOptions> parallelSettingsMedium = new Lazy<ParallelOptions>(
                () =>
                {
                    ParallelOptions parallelOptions = new ParallelOptions()
                    {
                        // Only use 25% of available processing power
                        MaxDegreeOfParallelism = (Environment.ProcessorCount * 1) / 4
                    };
                    return parallelOptions;
                },
                LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// Prevents a default instance of the <see cref="SettingsViewModel"/> class from being created.
        /// </summary>
        private SettingsViewModel() : base("Settings")
        {
            // Subscribe async to avoid a deadlock situation
            DockingViewModel.GetInstance().RegisterViewModel(this);
        }

        /// <summary>
        /// Gets the parallelism settings.
        /// </summary>
        public ParallelOptions ParallelSettingsFast
        {
            get
            {
                return parallelSettingsFast.Value;
            }
        }

        /// <summary>
        /// Gets the parallelism settings.
        /// </summary>
        public ParallelOptions ParallelSettingsMedium
        {
            get
            {
                return parallelSettingsMedium.Value;
            }
        }

        /// <summary>
        /// Gets or sets the saved twitch access tokens.
        /// </summary>
        public AccessTokens AccessTokens
        {
            get
            {
                using (MemoryStream memoryStream = new MemoryStream(Encoding.ASCII.GetBytes(Settings.Default.AccessTokens)))
                {
                    DataContractJsonSerializer deserializer = new DataContractJsonSerializer(typeof(AccessTokens));

                    return deserializer.ReadObject(memoryStream) as AccessTokens;
                }
            }

            set
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(AccessTokens));
                    serializer.WriteObject(memoryStream, value);

                    Settings.Default.AccessTokens = Encoding.ASCII.GetString(memoryStream.ToArray());
                    this.RaisePropertyChanged(nameof(this.AccessTokens));
                }
            }
        }

        /// <summary>
        /// Gets a singleton instance of the <see cref="SettingsViewModel"/> class.
        /// </summary>
        /// <returns>A singleton instance of the class.</returns>
        public static SettingsViewModel GetInstance()
        {
            return SettingsViewModel.settingsViewModelInstance.Value;
        }

        /// <summary>
        /// Saves the current settings.
        /// </summary>
        public void Save()
        {
            Settings.Default.Save();
        }
    }
    //// End class
}
//// End namespace