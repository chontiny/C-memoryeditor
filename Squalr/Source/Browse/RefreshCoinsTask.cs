namespace Squalr.Source.Browse.StreamConfig
{
    using ActionScheduler;
    using Squalr.Properties;
    using Squalr.Source.Api;
    using Squalr.Source.Api.Models;
    using Squalr.Source.Utils.Extensions;
    using System;

    /// <summary>
    /// Task to refresh the coin amount.
    /// </summary>
    internal class RefreshCoinsTask : ScheduledTask
    {
        /// <summary>
        /// The interval between refreshes.
        /// </summary>
        private const Int32 RefreshInterval = 30000;

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamVotePollTask" /> class.
        /// </summary>
        public RefreshCoinsTask(Action<Int32> updateAction) : base(taskName: "Refresh Coins", isRepeated: true, trackProgress: false)
        {
            this.UpdateAction = updateAction;
            this.UpdateInterval = RefreshCoinsTask.RefreshInterval;

            this.Schedule();
        }

        /// <summary>
        /// Gets or sets the refresh action.
        /// </summary>
        private Action<Int32> UpdateAction { get; set; }

        /// <summary>
        /// Called when the scheduled task is updated.
        /// </summary>
        protected override void OnUpdate()
        {
            AccessTokens accessTokens = SettingsViewModel.GetInstance().AccessTokens;

            if (accessTokens == null || accessTokens.AccessToken.IsNullOrEmpty())
            {
                return;
            }

            User user = SqualrApi.GetTwitchUser(accessTokens.AccessToken);

            this.UpdateAction?.Invoke(user.Coins);
            base.OnUpdate();
        }
    }
    //// End class
}
//// End namespace