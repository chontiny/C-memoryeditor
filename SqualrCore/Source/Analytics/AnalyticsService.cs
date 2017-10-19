namespace SqualrCore.Source.Analytics
{
    using Garlic;
    using System;
    using System.ComponentModel;
    using System.Threading;

    /// <summary>
    /// A service that handles analytics reporting.
    /// </summary>
    internal class AnalyticsService
    {
        /// <summary>
        /// The url for the Google Analytics associated site.
        /// </summary>
        private const String AnalyticsUrl = "https://www.squalr.com";

        /// <summary>
        /// The Google Analytics code associated with the site.
        /// </summary>
        private const String GoogleAnalyticsCode = "UA-86974038-1";

        /// <summary>
        /// The category for analytics events.
        /// </summary>
        private const String AnalyticsServiceCategory = "DesktopClient";

        public enum AnalyticsAction
        {
            [Description("General")]
            General,

            [Description("Cheat Browser")]
            CheatBrowser,

            [Description("Twitch Login")]
            TwitchLogin,
        }

        /// <summary>
        /// 
        /// </summary>
        private AnalyticsService()
        {
        }

        private AnalyticsSession Session { get; set; }

        /// <summary>
        /// Singleton instance of the <see cref="AnalyticsService" /> class.
        /// </summary>
        private static Lazy<AnalyticsService> analyticsServiceInstance = new Lazy<AnalyticsService>(
                () => { return new AnalyticsService(); },
                LazyThreadSafetyMode.ExecutionAndPublication);

        public static AnalyticsService GetInstance()
        {
            return AnalyticsService.analyticsServiceInstance.Value;
        }

        /// <summary>
        /// Starts the analytics service.
        /// </summary>
        public void Start()
        {
            this.Session = new AnalyticsSession(AnalyticsService.AnalyticsUrl, AnalyticsService.GoogleAnalyticsCode);
        }

        /// <summary>
        /// Sends an analytics event with an empty value.
        /// </summary>
        /// <param name="action">The analytics action.</param>
        /// <param name="label">>The analytics label.</param>
        public void SendEvent(AnalyticsAction action, Exception ex)
        {
            this.SendEvent(action, "Error", ex?.ToString());
        }

        /// <summary>
        /// Sends an analytics event with an empty value.
        /// </summary>
        /// <param name="action">The analytics action.</param>
        /// <param name="label">>The analytics label.</param>
        public void SendEvent(AnalyticsAction action, String label)
        {
            this.SendEvent(action, label, "empty");
        }

        /// <summary>
        /// Sends an analytics event.
        /// </summary>
        /// <param name="action">The analytics action.</param>
        /// <param name="label">>The analytics label.</param>
        /// <param name="value">>The analytics value.</param>
        public void SendEvent(AnalyticsAction action, String label, String value)
        {
            IAnalyticsPageViewRequest page = this.Session.CreatePageViewRequest("/", "Home page");

            page.SendEvent(AnalyticsService.AnalyticsServiceCategory, action.ToString(), label, value);
        }
    }
    //// End class
}
//// End namespace