using System;
using System.Collections.Generic;
using Android.Views;
using Android.Widget;
using System.Collections.Specialized;

namespace DroidHelpers.ListView
{
    /// <summary>
    /// A <see cref="BaseAdapter{T}"/> that can be used with an Android ListView. If the <see cref="ItemSource"/> is
    /// implementing <see cref="INotifyCollectionChanged"/>, than it will subscribe for collection changes.
    /// </summary>
    /// <typeparam name="T">The type of the items contained in the <see cref="ItemSource"/>.</typeparam>
    public abstract class ObservableListViewAdapter<T> : BaseAdapter<T>
    {
        private readonly IList<Android.Views.View> _views = new List<Android.Views.View>();
        private IList<T> _itemSource;
        private INotifyCollectionChanged _observableCollection;

        /// <summary>
        /// The item source. If it implements INotifyCollectionChanged 
        /// then the adapter will subscribe for collection changes.
        /// </summary>
        public IList<T> ItemSource
        {
            get
            {
                return _itemSource;
            }
            set
            {
                if (Equals(_itemSource, value)) return;

                if (_observableCollection != null)
                {
                    _observableCollection.CollectionChanged -= NotifierCollectionChanged;
                }

                _itemSource = value;
                _observableCollection = _itemSource as INotifyCollectionChanged;

                if (_observableCollection == null) return;
                _observableCollection.CollectionChanged += NotifierCollectionChanged;
            }
        }

        /// <summary>
        /// Returns the item at the specific position of the <see cref="ItemSource"/>.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <returns></returns>
        public override T this[int position] => ItemSource[position];

        /// <summary>
        /// Returns the item count of the <see cref="ItemSource"/>.
        /// </summary>
        public override int Count => ItemSource?.Count ?? 0;

        /// <summary>
        /// Returns the item id for a specific position.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public override long GetItemId(int position) => 0L;

        public sealed override Android.Views.View GetView(int position, Android.Views.View convertView, ViewGroup parent)
        {
            var view = convertView;
            if(view == null)
            {
                view = CreateView(position, parent);
                view.Tag = new ViewHolder<T>(view);
                _views.Add(view);
            }

            Bind(position, (ViewHolder<T>)view.Tag);
            return view;
        }

        /// <summary>
        /// Creates a view for the specific position
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="parent">The parent view.</param>
        /// <returns></returns>
        protected abstract Android.Views.View CreateView(int position, ViewGroup parent);

        /// <summary>
        /// Bind the view holder for an item at a specific position.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="viewHolder">The ViewHolder.</param>
        protected abstract void Bind(int position, ViewHolder<T> viewHolder);

        protected virtual void NotifierCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            NotifyDataSetChanged();
        }

        /// <summary>
        /// Will dispose all views and ViewHolders. 
        /// The adapter will unsubscribe itself from any INotifyCollectionChanged event.
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing) return;

            ItemSource = null;

            foreach (var itemView in _views)
            {
                itemView.Tag?.Dispose();
                itemView.Dispose();
            }

            _views.Clear();
            Dispose();
        }

        /// <summary>
        /// ViewHolder to cache views from an item view.
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        public class ViewHolder<TItem> : Java.Lang.Object
        {
            public ViewHolder(Android.Views.View view)
            {
                if (view == null) throw new ArgumentNullException(nameof(view));
                Views = new View.ViewCache(view);
            }

            public View.ViewCache Views { get; }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);
                if (!disposing) return;
                Views.Dispose();
                Dispose();
            }
        }
    }
}