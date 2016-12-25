namespace Ana.Source.ScriptEngine
{
    using System;
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
        public Boolean RunActivationFunction()
        {
            return true;
        }

        /// <summary>
        /// Continously runs the update function in the script
        /// </summary>
        public void RunUpdateFunction()
        {

        }

        /// <summary>
        /// Runs the deactivation function in the script
        /// </summary>
        public void RunDeactivationFunction()
        {
        }
    }
    //// End class
}
//// End namespace