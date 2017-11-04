namespace SqualrStream.Source.Store
{
    using SqualrCore.Source.ActionScheduler;
    using SqualrCore.Source.Output;
    using SqualrCore.Source.Utils.Extensions;
    using SqualrStream.Properties;
    using SqualrStream.Source.Api;
    using SqualrStream.Source.Api.Models;
    using System;
    using System.Threading;

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

            this.Start();
        }

        /// <summary>
        /// Gets or sets the refresh action.
        /// </summary>
        private Action<Int32> UpdateAction { get; set; }

        /// <summary>
        /// Called when the scheduled task starts.
        /// </summary>
        protected override void OnBegin()
        {
        }

        /// <summary>
        /// Called when the scheduled task is updated.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token for handling canceled tasks.</param>
        protected override void OnUpdate(CancellationToken cancellationToken)
        {
            try
            {
                AccessTokens accessTokens = SettingsViewModel.GetInstance().AccessTokens;

                if (accessTokens == null || accessTokens.AccessToken.IsNullOrEmpty())
                {
                    return;
                }

                User user = SqualrApi.GetTwitchUser(accessTokens.AccessToken);

                this.UpdateAction?.Invoke(user.Coins);
            }
            catch (Exception ex)
            {
                OutputViewModel.GetInstance().Log(OutputViewModel.LogLevel.Error, "Error refreshing user", ex);
            }
        }

        /// <summary>
        /// Called when the repeated task completes.
        /// </summary>
        protected override void OnEnd()
        {
        }
    }
    //// End class
}
//// End namespace