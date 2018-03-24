namespace Squalr.Engine.Input
{
    using Controller;
    using Keyboard;
    using Mouse;
    using Squalr.Engine.TaskScheduler;
    using System;
    using System.Threading;

    /// <summary>
    /// Manages all input devices and is responsible for updating them.
    /// </summary>
    public class InputManager : ScheduledTask, IInputManager
    {
        /// <summary>
        /// Singleton instance of the <see cref="WindowsAdapter"/> class
        /// </summary>
        private static Lazy<InputManager> inputManagerInstance = new Lazy<InputManager>(
            () => { return new InputManager(); },
            LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// The rate at which to collect input in ms. Currently ~60 times per second.
        /// </summary>
        private const Int32 InputCollectionIntervalMs = 1000 / 60;

        /// <summary>
        /// Prevents a default instance of the <see cref="InputManager" /> class.
        /// </summary>
        private InputManager() : base("Input Manager", isRepeated: true, trackProgress: false)
        {
            this.ControllerSubject = new ControllerCapture();
            this.KeyboardSubject = new KeyboardCapture();
            this.MouseSubject = new MouseCapture();

            this.Start();
        }

        public static InputManager GetInstance()
        {
            return InputManager.inputManagerInstance.Value;
        }

        /// <summary>
        /// Gets or sets the controller capture interface.
        /// </summary>
        private IControllerSubject ControllerSubject { get; set; }

        /// <summary>
        /// Gets or sets the keyboard capture interface.
        /// </summary>
        private KeyboardCapture KeyboardSubject { get; set; }

        /// <summary>
        /// Gets or sets the mouse capture interface.
        /// </summary>
        private IMouseSubject MouseSubject { get; set; }

        /// <summary>
        /// Gets the keyboard capture interface.
        /// </summary>
        /// <returns>The keyboard capture interface.</returns>
        public KeyboardCapture GetKeyboardCapture()
        {
            return this.KeyboardSubject;
        }

        /// <summary>
        /// Gets the mouse capture interface.
        /// </summary>
        /// <returns>The mouse capture interface.</returns>
        public IControllerSubject GetControllerCapture()
        {
            return this.ControllerSubject;
        }

        /// <summary>
        /// Gets the controller capture interface.
        /// </summary>
        /// <returns>The controller capture interface.</returns>
        public IMouseSubject GetMouseCapture()
        {
            return this.MouseSubject;
        }

        /// <summary>
        /// Begins input capture.
        /// </summary>
        protected override void OnBegin()
        {
            this.UpdateInterval = InputManager.InputCollectionIntervalMs;
        }

        /// <summary>
        /// Updates the input capture devices, polling the system for changes on each device.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token for handling canceled tasks.</param>
        protected override void OnUpdate(CancellationToken cancellationToken)
        {
            this.ControllerSubject.Update();
            this.KeyboardSubject.Update();
            this.MouseSubject.Update();
        }

        /// <summary>
        /// Called when the repeated task completes.
        /// </summary>
        protected override void OnEnd()
        {
        }
    }
    //// End class
}
//// End namespace