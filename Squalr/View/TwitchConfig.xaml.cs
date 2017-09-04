namespace Squalr.View
{
    using Squalr.Properties;
    using System;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for TwitchConfig.xaml.
    /// </summary>
    internal partial class TwitchConfig : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TwitchConfig" /> class.
        /// </summary>
        public TwitchConfig()
        {
            this.InitializeComponent();

            this.twitchChannel.Text = SettingsViewModel.GetInstance().TwitchChannel;
            this.twitchChannel.TextChanged += TwitchChannelTextChanged;
        }

        /// <summary>
        /// Event fired when twitch channel text is updated.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event args.</param>
        private void TwitchChannelTextChanged(Object sender, TextChangedEventArgs e)
        {
            SettingsViewModel.GetInstance().TwitchChannel = this.twitchChannel.Text;
        }
    }
    //// End class
}
//// End namespace