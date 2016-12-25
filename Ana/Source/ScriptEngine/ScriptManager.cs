namespace Ana.Source.ScriptEngine
{
    using CSScriptLibrary;
    using System;
    using System.Security;
    using System.Security.Permissions;
    using System.Threading;
    using System.Threading.Tasks;

    public class ScriptManager
    {
        /// <summary>
        /// Time to wait for the update loop to finish on deactivation
        /// </summary>
        private const Int32 AbortTime = 500;

        /// <summary>
        /// Update time in milliseconds
        /// </summary>
        private const Int32 UpdateTime = 1000 / 15;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptManager" /> class
        /// </summary>
        public ScriptManager()
        {
        }

        /// <summary>
        /// Gets or sets a cancelation request for the update loop
        /// </summary>
        private CancellationTokenSource CancelRequest { get; set; }

        /// <summary>
        /// Gets or sets the task for the update loops
        /// </summary>
        private Task Task { get; set; }

        /// <summary>
        /// Runs the activation function in the script
        /// </summary>
        /// <returns></returns>
        public Boolean RunActivationFunction(string script)
        {
            try
            {
                MethodDelegate onActivate = CSScript.LoadMethod(script).GetStaticMethod("*.OnActivate");
                Sandbox.With(SecurityPermissionFlag.Execution)
                       .Execute(() => onActivate());
            }
            catch (SecurityException ex)
            {
                // TODO: Log to user
                return false;
            }
            catch (Exception ex)
            {
                // TODO: Log to user
                return false;
            }

            return true;
        }

        /// <summary>
        /// Continously runs the update function in the script
        /// </summary>
        public void RunUpdateFunction()
        {
            DateTime previousTime = DateTime.Now;
            TimeSpan elapsedTime;
            this.CancelRequest = new CancellationTokenSource();

            try
            {
                this.Task = Task.Run(
                async () =>
                {
                    while (true)
                    {
                        DateTime currentTime = DateTime.Now;
                        elapsedTime = currentTime - previousTime;

                        // Call the update function, giving the elapsed milliseconds since the previous call
                        // function?.Call(elapsedTime.TotalMilliseconds);

                        previousTime = currentTime;

                        // Await with cancellation
                        await Task.Delay(ScriptManager.UpdateTime, this.CancelRequest.Token);
                    }
                },
                this.CancelRequest.Token);

                return;
            }
            catch
            {
                // TODO: Log to user
            }
        }

        /// <summary>
        /// Runs the deactivation function in the script
        /// </summary>
        public void RunDeactivationFunction()
        {
            try
            {
                // Abort the update loop
                try
                {
                    this.CancelRequest?.Cancel();
                    this.Task?.Wait(ScriptManager.AbortTime);
                }
                catch (Exception)
                {
                }

                return;
            }
            catch
            {
                // TODO: Log to user
            }
        }
    }
    //// End class
}
//// End namespace