namespace Ana.Source.Project
{
    using Output;
    using Project.ProjectItems;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization.Json;
    using System.Threading;
    using Utils.Extensions;

    /// <summary>
    /// View model for the Hotkey Manager.
    /// </summary>
    internal class HotkeyManager
    {
        /// <summary>
        /// The file extension for hotkeys.
        /// </summary>
        private const String HotkeyFileExtension = ".hotkeys";

        /// <summary>
        /// Singleton instance of the <see cref="HotkeyManager" /> class.
        /// </summary>
        private static Lazy<HotkeyManager> hotkeyManagerInstance = new Lazy<HotkeyManager>(
                () => { return new HotkeyManager(); },
                LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// Prevents a default instance of the <see cref="HotkeyManager" /> class from being created.
        /// </summary>
        private HotkeyManager()
        {
        }

        /// <summary>
        /// Gets a singleton instance of the <see cref="HotkeyManager"/> class.
        /// </summary>
        /// <returns>A singleton instance of the class.</returns>
        public static HotkeyManager GetInstance()
        {
            return hotkeyManagerInstance.Value;
        }

        /// <summary>
        /// Saves the hotkey profile for this project.
        /// </summary>
        /// <param name="projectFilePath">The path to the main project file.</param>
        public void Save(String projectFilePath)
        {
            try
            {
                String hotkeyFilePath = this.GetHotkeyFilePathFromProjectFilePath(projectFilePath);

                using (FileStream fileStream = new FileStream(hotkeyFilePath, FileMode.Create, FileAccess.Write))
                {
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(ProjectItemHotkey[]));
                    ProjectItemHotkey[] hotkeys = ProjectExplorerViewModel.GetInstance().ProjectRoot?.Flatten().Where(x => x.HotKey != null).Select(x => new ProjectItemHotkey(x.HotKey, x.Guid)).ToArray();
                    serializer.WriteObject(fileStream, hotkeys);
                }
            }
            catch
            {
                OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Warn, "Unable to save hotkey profile.");
                return;
            }
        }

        /// <summary>
        /// Opens the hotkey profile for this project.
        /// </summary>
        /// <param name="projectFilePath">The path to the main project file.</param>
        public void Open(String projectFilePath)
        {
            try
            {
                String hotkeyFilePath = this.GetHotkeyFilePathFromProjectFilePath(projectFilePath);

                if (File.Exists(hotkeyFilePath))
                {
                    using (FileStream fileStream = new FileStream(hotkeyFilePath, FileMode.Open, FileAccess.Read))
                    {
                        DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(ProjectItemHotkey[]));
                        ProjectItemHotkey[] projectItemHotkeys = serializer.ReadObject(fileStream) as ProjectItemHotkey[];

                        this.BindHotkeys(projectItemHotkeys);
                    }
                }
            }
            catch
            {
                OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Warn, "Unable to open hotkey profile.");
                return;
            }
        }

        /// <summary>
        /// Imports a hotkey profile for this project.
        /// </summary>
        /// <param name="projectFilePath">The path to the imported project file.</param>
        public void Import(String projectFilePath)
        {
            try
            {
                String hotkeyFilePath = this.GetHotkeyFilePathFromProjectFilePath(projectFilePath);

                if (File.Exists(hotkeyFilePath))
                {
                    using (FileStream fileStream = new FileStream(hotkeyFilePath, FileMode.Open, FileAccess.Read))
                    {
                        DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(ProjectItemHotkey[]));
                        ProjectItemHotkey[] projectItemHotkeys = serializer.ReadObject(fileStream) as ProjectItemHotkey[];

                        this.BindHotkeys(projectItemHotkeys);
                    }
                }
            }
            catch
            {
                OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Warn, "Unable to open hotkey profile.");
                return;
            }
        }

        /// <summary>
        /// Binds deserailized project item hotkeys to the corresponding project items.
        /// </summary>
        /// <param name="projectItemHotkeys">The deserialized project item hotkeys.</param>
        private void BindHotkeys(IEnumerable<ProjectItemHotkey> projectItemHotkeys)
        {
            if (projectItemHotkeys.IsNullOrEmpty())
            {
                return;
            }

            List<ProjectItem> items = ProjectExplorerViewModel.GetInstance().ProjectRoot?.Flatten();

            IEnumerable<KeyValuePair<ProjectItemHotkey, ProjectItem>> bindings = projectItemHotkeys.Join(
                items,
                hotkey => hotkey.ProjectItemGuid,
                item => item.Guid,
                (hotkey, item) => new KeyValuePair<ProjectItemHotkey, ProjectItem>(hotkey, item));

            foreach (KeyValuePair<ProjectItemHotkey, ProjectItem> binding in bindings)
            {
                binding.Value.LoadHotkey(binding.Key.Hotkey);
            }
        }

        /// <summary>
        /// Gets the hotkey file path that corresponds to this project file path.
        /// </summary>
        /// <param name="projectFilePath">The path to the project file.</param>
        /// <returns>The file path to the hotkey file.</returns>
        private String GetHotkeyFilePathFromProjectFilePath(String projectFilePath)
        {
            return Path.Combine(Path.GetDirectoryName(projectFilePath), Path.GetFileNameWithoutExtension(projectFilePath)) + HotkeyManager.HotkeyFileExtension;
        }
    }
    //// End class
}
//// End namespace