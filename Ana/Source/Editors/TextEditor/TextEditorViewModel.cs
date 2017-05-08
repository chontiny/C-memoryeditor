namespace Ana.Source.Editors.TextEditor
{
    using Docking;
    using Main;
    using Mvvm.Command;
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Input;

    /// <summary>
    /// View model for the Text Editor.
    /// </summary>
    internal class TextEditorViewModel : ToolViewModel
    {
        /// <summary>
        /// The content id for the docking library associated with this view model.
        /// </summary>
        public const String ToolContentId = nameof(TextEditorViewModel);

        /// <summary>
        /// Singleton instance of the <see cref="TextEditorViewModel" /> class.
        /// </summary>
        private static Lazy<TextEditorViewModel> textEditorViewModelInstance = new Lazy<TextEditorViewModel>(
                () => { return new TextEditorViewModel(); },
                LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// Prevents a default instance of the <see cref="TextEditorViewModel" /> class.
        /// </summary>
        private TextEditorViewModel() : base("Text Editor")
        {
            this.ContentId = TextEditorViewModel.ToolContentId;
            this.UpdateTextCommand = new RelayCommand<String>((text) => this.UpdateText(text), (text) => true);
            this.SaveTextCommand = new RelayCommand<String>((text) => this.SaveText(text), (text) => true);

            Task.Run(() => MainViewModel.GetInstance().RegisterTool(this));
        }

        /// <summary>
        /// Gets a command to save the active text.
        /// </summary>
        public ICommand SaveTextCommand { get; private set; }

        /// <summary>
        /// Gets a command to update the active text.
        /// </summary>
        public ICommand UpdateTextCommand { get; private set; }

        /// <summary>
        /// Gets the active text.
        /// </summary>
        public String Text { get; private set; }

        /// <summary>
        /// Gets a singleton instance of the <see cref="TextEditorViewModel" /> class.
        /// </summary>
        /// <returns>A singleton instance of the class.</returns>
        public static TextEditorViewModel GetInstance()
        {
            return TextEditorViewModel.textEditorViewModelInstance.Value;
        }

        /// <summary>
        /// Updates the active text.
        /// </summary>
        /// <param name="text">The raw text.</param>
        private void UpdateText(String text)
        {
            this.Text = text;
        }

        /// <summary>
        /// Saves the provided text.
        /// </summary>
        /// <param name="text">The raw text to save.</param>
        private void SaveText(String text)
        {
            this.UpdateText(text);
        }
    }
    //// End class
}
//// End namespace