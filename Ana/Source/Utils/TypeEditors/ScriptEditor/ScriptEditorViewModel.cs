namespace Ana.Source.Utils.ScriptEditor
{
    using Docking;
    using Main;
    using Mvvm.Command;
    using System;
    using System.Threading.Tasks;
    using System.Windows.Input;

    /// <summary>
    /// View model for the Script Editor
    /// </summary>
    internal class ScriptEditorViewModel : ToolViewModel
    {
        /// <summary>
        /// The content id for the docking library associated with this view model
        /// </summary>
        public const String ToolContentId = nameof(ScriptEditorViewModel);

        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptEditorViewModel" /> class
        /// </summary>
        public ScriptEditorViewModel() : base("Script Editor")
        {
            this.ContentId = ScriptEditorViewModel.ToolContentId;
            this.UpdateScriptCommand = new RelayCommand<String>((script) => this.UpdateScript(script), (script) => true);
            this.SaveScriptCommand = new RelayCommand<String>((script) => this.SaveScript(script), (script) => true);
            this.ExitCommand = new RelayCommand(() => this.Exit(), () => true);

            Task.Run(() => MainViewModel.GetInstance().Subscribe(this));
        }

        /// <summary>
        /// Gets a command to save the active script.
        /// </summary>
        public ICommand SaveScriptCommand { get; private set; }

        /// <summary>
        /// Gets a command to exit this window.
        /// </summary>
        public ICommand ExitCommand { get; private set; }

        /// <summary>
        /// Gets a command to update the active script text.
        /// </summary>
        public ICommand UpdateScriptCommand { get; private set; }

        /// <summary>
        /// Gets the active script text.
        /// </summary>
        public String Script { get; private set; }

        /// <summary>
        /// Updates the active script.
        /// </summary>
        /// <param name="script">The raw script text.</param>
        private void UpdateScript(String script)
        {
            this.Script = script;
        }

        /// <summary>
        /// Saves the provided script.
        /// </summary>
        /// <param name="script">The raw script to save.</param>
        private void SaveScript(String script)
        {
            this.UpdateScript(script);
        }

        /// <summary>
        /// Closes this window.
        /// </summary>
        private void Exit()
        {
            //// this.Close();
        }
    }
    //// End class
}
//// End namespace