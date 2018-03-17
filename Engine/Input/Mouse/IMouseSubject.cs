namespace Squalr.Engine.Input.Mouse
{
    /// <summary>
    /// Interface for an object which will capture mouse input
    /// </summary>
    public interface IMouseSubject : IInputCapture
    {
        /// <summary>
        /// Subscribes to mouse capture events
        /// </summary>
        /// <param name="subject">The observer to subscribe</param>
        void Subscribe(IMouseObserver subject);

        /// <summary>
        /// Unsubscribes from mouse capture events
        /// </summary>
        /// <param name="subject">The observer to unsubscribe</param>
        void Unsubscribe(IMouseObserver subject);
    }
    //// End interface
}
//// End namespace