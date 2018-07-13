namespace Squalr.Source.PacketEditor
{
    using Squalr.Source.Docking;
    using System;
    using System.Threading;

    /// <summary>
    /// View model for the Packet Editor.
    /// </summary>
    internal class PacketEditorViewModel : ToolViewModel
    {
        /// <summary>
        /// Singleton instance of the <see cref="PacketEditorViewModel" /> class.
        /// </summary>
        private static Lazy<PacketEditorViewModel> packetEditorViewModelInstance = new Lazy<PacketEditorViewModel>(
                () => { return new PacketEditorViewModel(); },
                LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// Prevents a default instance of the <see cref="PacketEditorViewModel" /> class from being created.
        /// </summary>
        private PacketEditorViewModel() : base("Packet Editor")
        {
            DockingViewModel.GetInstance().RegisterViewModel(this);
        }

        /// <summary>
        /// Gets a singleton instance of the <see cref="PacketEditorViewModel"/> class.
        /// </summary>
        /// <returns>A singleton instance of the class.</returns>
        public static PacketEditorViewModel GetInstance()
        {
            return PacketEditorViewModel.packetEditorViewModelInstance.Value;
        }
    }
    //// End class
}
//// End namespace