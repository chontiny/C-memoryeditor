using System;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

namespace Squalr.Engine.Scripting
{
    /// <summary>
    /// Instance of a single script.
    /// </summary>
    public class ModScript
    {
        /// <summary>
        /// Gets or sets a cancelation request for the update loop.
        /// </summary>
        private CancellationTokenSource CancelRequest { get; set; }

        /// <summary>
        /// Time to wait for the update loop to finish on deactivation.
        /// </summary>
        private const Int32 AbortTime = 500;

        /// <summary>
        /// Update time in milliseconds.
        /// </summary>
        private const Int32 UpdateTime = 1000 / 15;

        /// <summary>
        /// Gets or sets the task for the update loops.
        /// </summary>
        private Task Task { get; set; }

        public ModScript() : this(String.Empty)
        {
        }

        public ModScript(String script)
        {
            this.SetScript(script);
        }

        public String Text { get; set; }

        public String Name { get; set; }

        public Boolean IsActivated { get; set; }

        /// <summary>
        /// Gets or sets the compiled assembly object of a script.
        /// </summary>
        private dynamic ScriptObject { get; set; }

        public static ModScript FromCompressedAssembly(String compressedAssembly)
        {
            ModScript modScript = new ModScript();
            Byte[] assemblyBytes = Loader.DecompressCompiledScript(compressedAssembly);

            modScript.ScriptObject = Compiler.LoadCompiledScript(assemblyBytes);

            return modScript;
        }

        public async void SetScript(String script)
        {
            Byte[] assemblyBytes = await Task.Run(() => Compiler.CompileScript(script));

            this.ScriptObject = Compiler.LoadCompiledScript(assemblyBytes);
        }

        /// <summary>
        /// Runs the activation function in the script.
        /// </summary>
        /// <param name="assembly">The script to run.</param>
        /// <returns>Returns true if the function successfully ran, otherwise false.</returns>
        public Boolean RunActivationFunction()
        {
            try
            {
                // Bind the deactivation function such that scripts can deactivate themselves
                // this.ScriptObject.Deactivate = new Action(() => script.IsActivated = false);

                // Call OnActivate function in the script
                this.ScriptObject.OnActivate();

                Output.Output.Log(Output.LogLevel.Info, "Script activated: " + this.Name);
            }
            catch (SecurityException ex)
            {
                Output.Output.Log(Output.LogLevel.Error, "Invalid operation in sandbox environment", ex);
                return false;
            }
            catch (Exception ex)
            {
                Output.Output.Log(Output.LogLevel.Error, "Unable to activate script", ex);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Continously runs the update function in the script.
        /// </summary>
        /// <param name="script">The script to run.</param>
        public void RunUpdateFunction()
        {
            this.CancelRequest = new CancellationTokenSource();

            try
            {
                this.Task = Task.Run(
                async () =>
                {
                    TimeSpan elapsedTime;
                    DateTime previousTime = DateTime.Now;

                    while (true)
                    {
                        DateTime currentTime = DateTime.Now;
                        elapsedTime = currentTime - previousTime;

                        try
                        {
                            // Call the update function, giving the elapsed milliseconds since the previous call
                            ScriptObject.OnUpdate((Single)elapsedTime.TotalMilliseconds);
                        }
                        catch (Exception ex)
                        {
                            String exception = ex.ToString();

                            if (exception.ToString().Contains("does not contain a definition for 'OnUpdate'"))
                            {
                                Output.Output.Log(Output.LogLevel.Warn, "Optional update function not executed");
                            }
                            else
                            {
                                Output.Output.Log(Output.LogLevel.Error, "Error running update function: ", ex);
                            }

                            return;
                        }

                        previousTime = currentTime;

                        // Await with cancellation
                        await Task.Delay(ModScript.UpdateTime, this.CancelRequest.Token);
                    }
                },
                this.CancelRequest.Token);

                return;
            }
            catch
            {
                Output.Output.Log(Output.LogLevel.Error, "Error executing update loop.");
            }
        }

        /// <summary>
        /// Runs the deactivation function in the script.
        /// </summary>
        /// <param name="scriptItem">The script to run.</param>
        public void RunDeactivationFunction()
        {
            // Abort the update loop
            try
            {
                this.ScriptObject.OnDeactivate();

                Output.Output.Log(Output.LogLevel.Info, "Script deactivated: " + this.Name);

                try
                {
                    this.CancelRequest?.Cancel();
                    this.Task?.Wait(ModScript.AbortTime);
                }
                catch
                {
                }
            }
            catch (Exception ex)
            {
                Output.Output.Log(Output.LogLevel.Error, "Error when deactivating script", ex);
            }

            return;
        }
    }
    //// End class
}
//// End namespace