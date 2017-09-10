using System;
using System.Collections.Generic;
using Android.App;

namespace DroidHelpers.View
{
    /// <summary>
    /// ViewCache caches all views that are retrieved by id from an activity or view.
    /// It helps you to dispose all views, that were requested.
    /// </summary>
    public class ViewCache : IDisposable
    {
        private readonly Dictionary<int, Android.Views.View> _cache = new Dictionary<int, Android.Views.View>();
        private Activity _activityContext;
        private Android.Views.View _viewContext;

        /// <summary>
        /// Creates a new ViewCache.
        /// </summary>
        /// <param name="host">The host activity which is used to find views by id.</param>
        public ViewCache(Activity host)
        {
            _activityContext = host ?? throw new ArgumentNullException(nameof(host));
        }

        /// <summary>
        /// Creates a new ViewCache.
        /// </summary>
        /// <param name="host">The host view which is used to find views by id.</param>
        public ViewCache(Android.Views.View host)
        {
            _viewContext = host ?? throw new ArgumentNullException(nameof(host));
        }

        /// <summary>
        /// Returns either a cached view or tries to find it by id.
        /// </summary>
        /// <typeparam name="T">The view type.</typeparam>
        /// <param name="id">The id of the view.</param>
        /// <returns></returns>
        public T GetView<T>(int id) where T : Android.Views.View
        {
            if (!_cache.ContainsKey(id))
            {
                _cache.Add(id, FindViewById<T>(id));
            }

            return (T)_cache[id];
        }

        private T FindViewById<T>(int id) where T : Android.Views.View
            => _activityContext?.FindViewById<T>(id)
            ?? _viewContext.FindViewById<T>(id);

        /// <summary>
        /// Disposing all cached views. The host activity or host view will not be disposed.
        /// </summary>
        public void Dispose()
        {
            foreach (var item in _cache.Values)
            {
                item.Dispose();
            }
            _cache.Clear();
            _activityContext = null;
            _viewContext = null;
        }
    }
}