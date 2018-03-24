namespace Squalr.Engine.Input
{
    using Controller;
    using Keyboard;
    using Mouse;

    /// <summary>
    /// An interface defining an object which is responsable for managing all input devices
    /// </summary>
    public interface IInputManager
    {
        /// <summary>
        /// Gets the keyboard capture interface
        /// </summary>
        /// <returns>The keyboard capture interface</returns>
        KeyboardCapture GetKeyboardCapture();

        /// <summary>
        /// Gets the mouse capture interface
        /// </summary>
        /// <returns>The mouse capture interface</returns>
        IMouseSubject GetMouseCapture();

        /// <summary>
        /// Gets the controller capture interface
        /// </summary>
        /// <returns>The controller capture interface</returns>
        IControllerSubject GetControllerCapture();
    }
    //// End class
}
//// End namespace