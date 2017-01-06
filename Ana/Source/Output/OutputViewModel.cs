namespace Ana.Source.Output
{
    using Docking;
    using Main;
    using System;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// View model for the Output.
    /// </summary>
    internal class OutputViewModel : ToolViewModel
    {
        /// <summary>
        /// The content id for the docking library associated with this view model.
        /// </summary>
        public const String ToolContentId = nameof(OutputViewModel);

        private const Int32 LogCapacity = UInt16.MaxValue;

        /// <summary>
        /// Singleton instance of the <see cref="OutputViewModel" /> class.
        /// </summary>
        private static Lazy<OutputViewModel> outputViewModelInstance = new Lazy<OutputViewModel>(
                () => { return new OutputViewModel(); },
                LazyThreadSafetyMode.ExecutionAndPublication);

        private StringBuilder logText;

        /// <summary>
        /// Prevents a default instance of the <see cref="OutputViewModel" /> class from being created.
        /// </summary>
        private OutputViewModel() : base("Output")
        {
            this.ContentId = OutputViewModel.ToolContentId;
            this.logText = new StringBuilder(OutputViewModel.LogCapacity);

            Task.Run(() => MainViewModel.GetInstance().Subscribe(this));
        }

        public String LogText
        {
            get
            {
                return this.logText.ToString();
            }
        }

        public static OutputViewModel GetInstance()
        {
            return OutputViewModel.outputViewModelInstance.Value;
        }

        public void Log(LogLevel logLevel, String message)
        {
            message = DateTime.Now + "\t" + message;
            Console.WriteLine(message);

            this.logText.AppendLine(message);

            // Over capacity, remove the first line
            if (this.logText.Length > OutputViewModel.LogCapacity)
            {
                String currentText = this.LogText;
                this.logText = new StringBuilder(currentText.Substring(currentText.IndexOf(Environment.NewLine) + 1));
            }

            this.RaisePropertyChanged(nameof(this.LogText));
        }

        [Flags]
        public enum LogLevel
        {
            /// <summary>
            /// Highly detailed information.
            /// </summary>
            Trace = 1 << 0,

            /// <summary>
            /// Debugging information.
            /// </summary>
            Debug = 1 << 1,

            /// <summary>
            /// Standard information.
            /// </summary>
            Info = 1 << 2,

            /// <summary>
            /// Warning messages.
            /// </summary>
            Warn = 1 << 3,

            /// <summary>
            /// Error messages.
            /// </summary>
            Error = 1 << 4,

            /// <summary>
            /// Severe error messages.
            /// </summary>
            Fatal = 1 << 5,
        }
    }
    //// End class
}
//// End namespace