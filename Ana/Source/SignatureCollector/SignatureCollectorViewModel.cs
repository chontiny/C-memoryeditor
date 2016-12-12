namespace Ana.Source.SignatureCollector
{
    using Docking;
    using Engine;
    using Main;
    using Mvvm.Command;
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Input;

    /// <summary>
    /// View model for the Signature Collector
    /// </summary>
    internal class SignatureCollectorViewModel : ToolViewModel
    {
        /// <summary>
        /// The content id for the docking library associated with this view model
        /// </summary>
        public const String ToolContentId = nameof(SignatureCollectorViewModel);

        private String signature;

        private String binaryVersion;

        private String binaryHeaderHash;

        private String binaryImportHash;

        private String mainModuleHash;

        private String windowTitle;

        private String emulatorHash;

        /// <summary>
        /// Singleton instance of the <see cref="SignatureCollectorViewModel" /> class
        /// </summary>
        private static Lazy<SignatureCollectorViewModel> signatureCollectorViewModelInstance = new Lazy<SignatureCollectorViewModel>(
                () => { return new SignatureCollectorViewModel(); },
                LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// Prevents a default instance of the <see cref="SignatureCollectorViewModel" /> class from being created
        /// </summary>
        private SignatureCollectorViewModel() : base("Signature Collector")
        {
            this.ContentId = SignatureCollectorViewModel.ToolContentId;
            this.CollectSignatureCommand = new RelayCommand(() => Task.Run(() => this.CollectSignature()), () => true);

            MainViewModel.GetInstance().Subscribe(this);
        }

        public ICommand CollectSignatureCommand { get; private set; }

        public String Signature
        {
            get
            {
                return this.signature;
            }

            set
            {
                this.signature = value;
            }
        }

        public String WindowTitle
        {
            get
            {
                return this.windowTitle;
            }

            set
            {
                this.windowTitle = value;
            }
        }

        public String BinaryVersion
        {
            get
            {
                return this.binaryVersion;
            }

            set
            {
                this.binaryVersion = value;
            }
        }

        public String BinaryHeaderHash
        {
            get
            {
                return this.binaryHeaderHash;
            }

            set
            {
                this.binaryHeaderHash = value;
            }
        }

        public String BinaryImportHash
        {
            get
            {
                return this.binaryImportHash;
            }

            set
            {
                this.binaryImportHash = value;
            }
        }

        public String MainModuleHash
        {
            get
            {
                return this.mainModuleHash;
            }

            set
            {
                this.mainModuleHash = value;
            }
        }

        public String EmulatorHash
        {
            get
            {
                return this.emulatorHash;
            }

            set
            {
                this.emulatorHash = value;
            }
        }

        /// <summary>
        /// Gets a singleton instance of the <see cref="SignatureCollectorViewModel"/> class
        /// </summary>
        /// <returns>A singleton instance of the class</returns>
        public static SignatureCollectorViewModel GetInstance()
        {
            return SignatureCollectorViewModel.signatureCollectorViewModelInstance.Value;
        }

        private void CollectSignature()
        {
            this.WindowTitle = EngineCore.GetInstance().OperatingSystemAdapter.CollectWindowTitle();
            this.BinaryVersion = EngineCore.GetInstance().OperatingSystemAdapter.CollectBinaryVersion();
            this.BinaryHeaderHash = EngineCore.GetInstance().OperatingSystemAdapter.CollectBinaryHeaderHash();
            this.BinaryImportHash = EngineCore.GetInstance().OperatingSystemAdapter.CollectBinaryImportHash();
            this.MainModuleHash = EngineCore.GetInstance().OperatingSystemAdapter.CollectMainModuleHash();
            this.EmulatorHash = EngineCore.GetInstance().OperatingSystemAdapter.CollectEmulatorHash();

            // TODO: Base64 encode
            this.Signature = this.WindowTitle + this.BinaryVersion + this.BinaryHeaderHash + this.MainModuleHash + this.EmulatorHash;
        }
    }
    //// End class
}
//// End namespace