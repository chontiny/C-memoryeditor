namespace Squalr.Source.Editors.ValueEditor
{
    using Squalr.Source.Docking;
    using System;
    using System.Threading;

    /// <summary>
    /// View model for the Value Editor.
    /// </summary>
    public class ValueEditorViewModel : ToolViewModel
    {
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
            DockingViewModel.GetInstance().RegisterViewModel(this);
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