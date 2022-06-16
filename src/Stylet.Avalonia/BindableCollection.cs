﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Stylet.Avalonia
{
    /// <summary>
    /// Represents a collection which is observasble
    /// </summary>
    /// <typeparam name="T">The type of elements in the collections</typeparam>
    public interface IObservableCollection<T> : IList<T>, INotifyPropertyChanged, INotifyCollectionChanged
    {
        /// <summary>
        /// Add a range of items
        /// </summary>
        /// <param name="items">Items to add</param>
        void AddRange(IEnumerable<T> items);

        /// <summary>
        /// Remove a range of items
        /// </summary>
        /// <param name="items">Items to remove</param>
        void RemoveRange(IEnumerable<T> items);
    }

    /// <summary>
    /// Interface encapsulating IReadOnlyList and INotifyCollectionChanged
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection</typeparam>
    public interface IReadOnlyObservableCollection<out T> : IReadOnlyList<T>, INotifyCollectionChanged,  INotifyCollectionChanging
    { }

    /// <summary>
    /// ObservableCollection subclass which supports AddRange and RemoveRange
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection</typeparam>
    public class BindableCollection<T> : ObservableCollection<T>, IObservableCollection<T>, IReadOnlyObservableCollection<T>
    {
        /// <summary>
        ///  We have to disable notifications when adding individual elements in the AddRange and RemoveRange implementations
        /// </summary>
        private bool isNotifying = true;

        /// <summary>
        /// Initialises a new instance of the <see cref="BindableCollection{T}"/> class
        /// </summary>
        public BindableCollection()
        { }

        /// <summary>
        /// Initialises a new instance of the <see cref="BindableCollection{T}"/> class that contains the given members
        /// </summary>
        /// <param name="collection">The collection from which the elements are copied</param>
        public BindableCollection(IEnumerable<T> collection) : base(collection) { }

        /// <summary>
        /// Occurs when the collection will change
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanging;

        /// <summary>
        /// Raises the System.Collections.ObjectModel.ObservableCollection{T}.PropertyChanged event with the provided arguments.
        /// </summary>
        /// <param name="e">Arguments of the event being raised.</param>
        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            // Avoid doing a dispatch if nothing's subscribed....
            if (isNotifying)
                base.OnPropertyChanged(e);
        }

        /// <summary>
        /// Raises the CollectionChanging event with the provided arguments.
        /// </summary>
        /// <param name="e">Arguments of the event being raised.</param>
        protected virtual void OnCollectionChanging(NotifyCollectionChangedEventArgs e)
        {
            if (isNotifying)
            {
                var handler = CollectionChanging;
                if (handler != null)
                {
                    using (BlockReentrancy())
                    {
                        handler(this, e);
                    }
                }
            }
        }

        /// <summary>
        /// Raises the System.Collections.ObjectModel.ObservableCollection{T}.CollectionChanged event with the provided arguments.
        /// </summary>
        /// <param name="e">Arguments of the event being raised.</param>
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (isNotifying)
                base.OnCollectionChanged(e);
        }

        /// <summary>
        /// Add a range of items
        /// </summary>
        /// <param name="items">Items to add</param>
        public virtual void AddRange(IEnumerable<T> items)
        {
            Execute.OnUIThreadSync(() =>
            {
                OnCollectionChanging(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

                var previousNotificationSetting = isNotifying;
                isNotifying = false;
                var index = Count;
                foreach (var item in items)
                {
                    base.InsertItem(index, item);
                    index++;
                }
                isNotifying = previousNotificationSetting;
                OnPropertyChanged(new PropertyChangedEventArgs("Count"));
                OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
                // Can't add with a range, or it throws an exception
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            });
        }

        /// <summary>
        /// Remove a range of items
        /// </summary>
        /// <param name="items">Items to remove</param>
        public virtual void RemoveRange(IEnumerable<T> items)
        {
            Execute.OnUIThreadSync(() =>
            {
                OnCollectionChanging(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

                var previousNotificationSetting = isNotifying;
                isNotifying = false;
                foreach (var item in items)
                {
                    var index = IndexOf(item);
                    if (index >= 0)
                        base.RemoveItem(index);
                }
                isNotifying = previousNotificationSetting;
                OnPropertyChanged(new PropertyChangedEventArgs("Count"));
                OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
                // Can't remove with a range, or it throws an exception
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            });
        }

        /// <summary>
        /// Raise a change notification indicating that all bindings should be refreshed
        /// </summary>
        public void Refresh()
        {
            Execute.OnUIThreadSync(() =>
            {
                OnPropertyChanged(new PropertyChangedEventArgs("Count"));
                OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
                OnCollectionChanging(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            });
        }

        /// <summary>
        /// Called by base class Collection&lt;T&gt; when an item is added to list;
        /// raises a CollectionChanged event to any listeners.
        /// </summary>
        /// <param name="index">Index at which to insert the item</param>
        /// <param name="item">Item to insert</param>
        protected override void InsertItem(int index, T item)
        {
            Execute.OnUIThreadSync(() =>
            {
                OnCollectionChanging(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
                base.InsertItem(index, item);
            });
        }

        /// <summary>
        /// Called by base class Collection{T} when an item is set in list;
        /// raises a CollectionChanged event to any listeners.
        /// </summary>
        /// <param name="index">Index of the item to set</param>
        /// <param name="item">Item to set</param>
        protected override void SetItem(int index, T item)
        {
            Execute.OnUIThreadSync(() =>
            {
                OnCollectionChanging(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, item, this[index], index));
                base.SetItem(index, item);
            });
        }

        /// <summary>
        /// Called by base class Collection&lt;T&gt; when an item is removed from list;
        /// raises a CollectionChanged event to any listeners.
        /// </summary>
        /// <param name="index">Index of the item to remove</param>
        protected override void RemoveItem(int index)
        {
            Execute.OnUIThreadSync(() =>
            {
                OnCollectionChanging(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, this[index], index));
                base.RemoveItem(index);
            });
        }

        /// <summary>
        /// Called by base class Collection&lt;T&gt; when the list is being cleared;
        /// raises a CollectionChanged event to any listeners.
        /// </summary>
        protected override void ClearItems()
        {
            Execute.OnUIThreadSync(() =>
            {
                OnCollectionChanging(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                base.ClearItems();
            });
        }
    }
}
