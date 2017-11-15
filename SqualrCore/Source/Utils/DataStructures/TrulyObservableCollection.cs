namespace SqualrCore.Source.Utils.DataStructures
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;

    /// <summary>
    /// A collection of items for which changes are observed. Fixes the poor implementation of ObservableCollection in the .NET framework.
    /// </summary>
    /// <typeparam name="T">The type of the items in the collection</typeparam>
    public sealed class TrulyObservableCollection<T> : ObservableCollection<T> where T : INotifyPropertyChanged
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TrulyObservableCollection{T}" /> class.
        /// </summary>
        public TrulyObservableCollection()
        {
            this.CollectionChanged += this.FullObservableCollectionCollectionChanged;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TrulyObservableCollection{T}" /> class.
        /// </summary>
        /// <param name="items">The initial items in the observable collection.</param>
        public TrulyObservableCollection(IEnumerable<T> items) : this()
        {
            foreach (T item in items)
            {
                this.Add(item);
            }
        }

        /// <summary>
        /// Notifies observers that the collection changed. Called when an item property changes.
        /// </summary>
        /// <param name="sender">The sending object.</param>
        /// <param name="propertyChangedEventArgs">The event args.</param>
        private void ItemPropertyChanged(Object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            Dispatcher.Run(() =>
            {
                NotifyCollectionChangedEventArgs args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, sender, sender, IndexOf((T)sender));
                this.OnCollectionChanged(args);
            });
        }

        /// <summary>
        /// Registers a callback function with all items in this collection, such that we can later notify observers that this collection has changed.
        /// </summary>
        /// <param name="sender">The sending object.</param>
        /// <param name="collectionChangedEventArgs">The event args.</param>
        private void FullObservableCollectionCollectionChanged(Object sender, NotifyCollectionChangedEventArgs collectionChangedEventArgs)
        {
            if (collectionChangedEventArgs.NewItems != null)
            {
                foreach (Object item in collectionChangedEventArgs.NewItems)
                {
                    ((INotifyPropertyChanged)item).PropertyChanged += this.ItemPropertyChanged;
                }
            }

            if (collectionChangedEventArgs.OldItems != null)
            {
                foreach (Object item in collectionChangedEventArgs.OldItems)
                {
                    ((INotifyPropertyChanged)item).PropertyChanged -= this.ItemPropertyChanged;
                }
            }
        }
    }
    //// End class
}
//// End namespace