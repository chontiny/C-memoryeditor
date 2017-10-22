namespace SqualrClient.View
{
    using SqualrClient.Source.Browse;
    using SqualrClient.Source.Browse.StreamConfig;
    using SqualrClient.Source.Browse.TwitchLogin;
    using SqualrClient.View.Editors;
    using System;
    using System.Windows;

    /// <summary>
    /// Provides the template required to view a pane.
    /// </summary>
    internal class ViewTemplateSelector : SqualrCore.View.ViewTemplateSelector
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ViewTemplateSelector" /> class.
        /// </summary>
        public ViewTemplateSelector()
        {
        }

        /// <summary>
        /// Gets or sets the template for the Browser.
        /// </summary>
        public DataTemplate BrowseViewTemplate { get; set; }

        /// <summary>
        /// Gets or sets the template for the Cheat Browser.
        /// </summary>
        public DataTemplate CheatBrowserViewTemplate { get; set; }

        /// <summary>
        /// Gets or sets the template for the Twitch Login.
        /// </summary>
        public DataTemplate TwitchLoginViewTemplate { get; set; }

        /// <summary>
        /// Gets or sets the template for the Stream Weaver.
        /// </summary>
        public DataTemplate TwitchConfigViewTemplate { get; set; }

        /// <summary>
        /// Gets or sets the template for the Stream Icon Editor.
        /// </summary>
        public DataTemplate StreamIconEditorViewTemplate { get; set; }

        /// <summary>
        /// Returns the required template to display the given view model.
        /// </summary>
        /// <param name="item">The view model.</param>
        /// <param name="container">The dependency object.</param>
        /// <returns>The template associated with the provided view model.</returns>
        public override DataTemplate SelectTemplate(Object item, DependencyObject container)
        {
            if (item is BrowseViewModel)
            {
                return this.BrowseViewTemplate;
            }
            else if (item is TwitchLoginViewModel)
            {
                return this.TwitchLoginViewTemplate;
            }
            else if (item is StreamConfigViewModel)
            {
                return this.TwitchConfigViewTemplate;
            }
            else if (item is StreamIconEditor)
            {
                return this.StreamIconEditorViewTemplate;
            }

            return base.SelectTemplate(item, container);
        }
    }
    //// End class
}
//// End namespace