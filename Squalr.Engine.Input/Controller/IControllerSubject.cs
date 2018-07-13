namespace Squalr.Engine.Input.Controller
{
    /// <summary>
    /// Interface for an object which will capture controller input
    /// </summary>
    public interface IControllerSubject : IInputCapture
    {
        /// <summary>
        /// Subscribes to controller capture events
        /// </summary>
        /// <param name="subject">The observer to subscribe</param>
        void Subscribe(IControllerObserver subject);

        /// <summary>
        /// Unsubscribes from controller capture events
        /// </summary>
        /// <param name="subject">The observer to unsubscribe</param>
        void Unsubscribe(IControllerObserver subject);
    }
    //// End interface
}
//// End namespace