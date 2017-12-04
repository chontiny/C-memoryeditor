namespace SqualrCore.Source.Utils
{
    using System;
    using System.Windows;

    /// <summary>
    /// Class for running tasks on the same thread as the UI.
    /// </summary>
    public class Dispatcher
    {
        /// <summary>
        /// Runs the action on the same thread as the UI.
        /// </summary>
        /// <param name="action">The action to run on the UI thread.</param>
        public static void Run(Action action)
        {
            Application.Current?.Dispatcher?.Invoke(delegate
            {
                action?.Invoke();
            });
        }
    }
    //// End class
}
//// End namespace