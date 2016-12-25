namespace Ana.Source.ScriptEngine
{
    using CSScriptLibrary;
    using Graphics;
    using Hook;
    using Input;
    using Memory;
    using System;
    using System.Security.Permissions;
    using System.Threading;
    using System.Threading.Tasks;

    public class ScriptEngine
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
        public ScriptEngine()
        {
            this.MemoryCore = new MemoryCore();
            this.GraphicsCore = new GraphicsCore();
            this.HookCore = new HookCore();
            this.InputCore = new InputCore();
        }

        private IMemoryCore MemoryCore { get; set; }

        private IGraphicsCore GraphicsCore { get; set; }

        private IHookCore HookCore { get; set; }

        private IInputCore InputCore { get; set; }

        /// <summary>
        /// Gets or sets a cancelation request for the update loop
        /// </summary>
        private CancellationTokenSource CancelRequest { get; set; }

        /// <summary>
        /// Gets or sets the task for the update loop
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
                Sandbox.With(SecurityPermissionFlag.Execution)
                       .Execute(() => (new AsmHelper(CSScript.LoadMethod(script))).Invoke("*.OnActivate"));
            }
            catch
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
                        await Task.Delay(ScriptEngine.UpdateTime, this.CancelRequest.Token);
                    }
                },
                this.CancelRequest.Token);

                return;
            }
            catch
            {
                // On failure we do nothing, since the OnUpdate function is optional
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
                    this.Task?.Wait(ScriptEngine.AbortTime);
                }
                catch (Exception)
                {
                }

                return;
            }
            catch
            {
                // On failure we do nothing, since we are deactivating the script anyways
            }
        }
    }
    //// End class
}
//// End namespace