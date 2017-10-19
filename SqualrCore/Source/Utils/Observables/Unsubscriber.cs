namespace Squalr.Source.Utils.Observables
{
    using System;
    using System.Collections.Generic;

    internal class Unsubscriber<T> : IDisposable
    {
        private HashSet<IObserver<T>> observers;
        private IObserver<T> observer;

        internal Unsubscriber(HashSet<IObserver<T>> observers, IObserver<T> observer)
        {
            this.observers = observers;
            this.observer = observer;
        }

        public void Dispose()
        {
            if (observers.Contains(observer))
            {
                observers.Remove(observer);
            }
        }
    }
    //// End class
}
//// End namespace