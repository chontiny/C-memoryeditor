namespace SqualrClient.View
{
    using SqualrClient.Properties;
    using SqualrClient.Source.Browse;
    using SqualrClient.Source.Browse.Library;
    using SqualrClient.Source.Browse.StreamConfig;
    using SqualrClient.Source.Browse.TwitchLogin;
    using SqualrClient.Source.Editors.StreamIconEditor;
    using System.Windows;

    /// <summary>
    /// Provides the template required to view a pane.
    /// </summary>
    internal class ViewTemplateSelector : SqualrCore.View.ViewTemplateSelector
    {
        /// <summary>
        /// The template for the Browser.
        /// </summary>
        public DataTemplate browseViewTemplate;

        /// <summary>
        /// The template for the Library.
        /// </summary>
        public DataTemplate libraryViewTemplate;

        /// <summary>
        /// The template for the Twitch Login.
        /// </summary>
        public DataTemplate twitchLoginViewTemplate;

        /// <summary>
        /// The template for the Stream Config.
        /// </summary>
        public DataTemplate streamConfigViewTemplate;

        /// <summary>
        /// The template for the Stream Icon Editor.
        /// </summary>
        public DataTemplate streamIconEditorViewTemplate;

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewTemplateSelector" /> class.
        /// </summary>
        public ViewTemplateSelector() : base()
        {
        }

        /// <summary>
        /// Gets or sets the template for the Settings.
        /// </summary>
        public DataTemplate SettingsViewTemplate
        {
            get
            {
                return this.settingsViewTemplate;
            }

            set
            {
                this.settingsViewTemplate = value;
                this.DataTemplates[typeof(SettingsViewModel)] = value;
            }
        }

        /// <summary>
        /// Gets or sets the template for the Browser.
        /// </summary>
        public DataTemplate BrowseViewTemplate
        {
            get
            {
                return this.browseViewTemplate;
            }

            set
            {
                this.browseViewTemplate = value;
                this.DataTemplates[typeof(BrowseViewModel)] = value;
            }
        }

        /// <summary>
        /// Gets or sets the template for the Library.
        /// </summary>
        public DataTemplate LibraryViewTemplate
        {
            get
            {
                return this.libraryViewTemplate;
            }

            set
            {
                this.libraryViewTemplate = value;
                this.DataTemplates[typeof(LibraryViewModel)] = value;
            }
        }

        /// <summary>
        /// Gets or sets the template for the Twitch Login.
        /// </summary>
        public DataTemplate TwitchLoginViewTemplate
        {
            get
            {
                return this.twitchLoginViewTemplate;
            }

            set
            {
                this.twitchLoginViewTemplate = value;
                this.DataTemplates[typeof(TwitchLoginViewModel)] = value;
            }
        }

        /// <summary>
        /// Gets or sets the template for the Stream Config.
        /// </summary>
        public DataTemplate StreamConfigViewTemplate
        {
            get
            {
                return this.streamConfigViewTemplate;
            }

            set
            {
                this.streamConfigViewTemplate = value;
                this.DataTemplates[typeof(StreamConfigViewModel)] = value;
            }
        }

        /// <summary>
        /// Gets or sets the template for the Stream Icon Editor.
        /// </summary>
        public DataTemplate StreamIconEditorViewTemplate
        {
            get
            {
                return this.streamIconEditorViewTemplate;
            }

            set
            {
                this.streamIconEditorViewTemplate = value;
                this.DataTemplates[typeof(StreamIconEditorViewModel)] = value;
            }
        }
    }
    //// End class
}
//// End namespace