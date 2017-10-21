namespace Squalr.Source.Editors.ValueEditor
{
    using SqualrCore.Source.Docking;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// View model for the Value Editor.
    /// </summary>
    internal class ValueEditorViewModel : ToolViewModel
    {
        /// <summary>
        /// The content id for the docking library associated with this view model.
        /// </summary>
        public const String ToolContentId = nameof(ValueEditorViewModel);

        /// <summary>
        /// Singleton instance of the <see cref="ValueEditorViewModel" /> class.
        /// </summary>
        private static Lazy<ValueEditorViewModel> valueEditorViewModelInstance = new Lazy<ValueEditorViewModel>(
                () => { return new ValueEditorViewModel(); },
                LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// Prevents a default instance of the <see cref="ValueEditorViewModel" /> class.
        /// </summary>
        public ValueEditorViewModel() : base("Value Editor")
        {
            this.ContentId = ValueEditorViewModel.ToolContentId;

            Task.Run(() => DockingViewModel.GetInstance().RegisterViewModel(this));
        }

        /// <summary>
        /// Gets or sets the value being edited.
        /// </summary>
        public Object Value { get; set; }

        /// <summary>
        /// Gets a singleton instance of the <see cref="ValueEditorViewModel" /> class.
        /// </summary>
        /// <returns>A singleton instance of the class.</returns>
        public static ValueEditorViewModel GetInstance()
        {
            return ValueEditorViewModel.valueEditorViewModelInstance.Value;
        }
    }
    //// End class
}
//// End namespace