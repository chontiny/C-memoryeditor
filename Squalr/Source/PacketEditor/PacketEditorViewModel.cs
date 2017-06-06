namespace Squalr.Source.Output
{
    using Docking;
    using Main;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// View model for the Packet Editor.
    /// </summary>
    internal class PacketEditorViewModel : ToolViewModel
    {
        /// <summary>
        /// The content id for the docking library associated with this view model.
        /// </summary>
        public const String ToolContentId = nameof(PacketEditorViewModel);

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
            this.ContentId = PacketEditorViewModel.ToolContentId;

            Task.Run(() => MainViewModel.GetInstance().RegisterTool(this));
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