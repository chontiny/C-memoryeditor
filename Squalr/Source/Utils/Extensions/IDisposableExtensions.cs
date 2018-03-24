namespace Squalr.Source.Utils.Extensions
{
    using Squalr.Engine.Utils.Observables;
    using System;

    /// <summary>
    /// Extension methods for the IDisposable interface.
    /// </summary>
    public static class IDisposableExtensions
    {
        public static IDisposable WeakSubscribe<T>(this IObservable<T> observable, IObserver<T> observer)
        {
            return new WeakObserver<T>(observable, observer);
        }
    }
    //// End class
}
//// End namespace