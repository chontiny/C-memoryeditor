namespace Ana.Source.Engine.Input.Keyboard
{
    using SharpDX.DirectInput;
    using System.Collections.Generic;

    /// <summary>
    /// Interface for an object which will capture keyboard input
    /// </summary>
    internal interface IKeyboardSubject : IInputCapture
    {
        /// <summary>
        /// Subscribes to keyboard capture events
        /// </summary>
        /// <param name="subject">The observer to subscribe</param>
        void Subscribe(IKeyboardObserver subject);

        /// <summary>
        /// Unsubscribes from keyboard capture events
        /// </summary>
        /// <param name="subject">The observer to unsubscribe</param>
        void Unsubscribe(IKeyboardObserver subject);

        /// <summary>
        /// Notifies observers of a key press event
        /// </summary>
        /// <param name="key">The key that was pressed</param>
        void NotifyKeyPress(Key key);

        /// <summary>
        /// Notifies observers of a key release event
        /// </summary>
        /// <param name="key">The key that was released</param>
        void NotifyKeyRelease(Key key);

        /// <summary>
        /// Notifies observers of a key down event
        /// </summary>
        /// <param name="key">The key that is down</param>
        void NotifyKeyDown(Key key);

        /// <summary>
        /// Notifies observers of a set of key down events
        /// </summary>
        /// <param name="downKeys">The keys that are down</param>
        void NotifyAllDownKeys(HashSet<Key> downKeys);
    }
    //// End interface
}
//// End namespace