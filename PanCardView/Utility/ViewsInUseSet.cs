using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Xamarin.Forms;

namespace PanCardView.Utility
{
    public sealed class ViewsInUseSet
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        private readonly Dictionary<View, int> _viewsSet = new Dictionary<View, int>();

        public IReadOnlyList<View> Views => _viewsSet.Keys?.ToList();

        public void AddRange(IEnumerable<View> views)
        {
            foreach (var view in views.Where(x => x != null))
            {
                Add(view);
            }
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, views));
        }

        public void RemoveRange(IEnumerable<View> views)
        {
            foreach (var view in views)
            {
                Remove(view);
            }
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, views));
        }

        public bool Contains(View view)
        => view != null && _viewsSet.ContainsKey(view);

        private void Add(View view)
        => _viewsSet[view] = Contains(view)
            ? _viewsSet[view] + 1
            : 1;

        private void Remove(View view)
        {
            if (!Contains(view))
            {
                return;
            }

            var currentCount = _viewsSet[view] - 1;
            if (currentCount > 0)
            {
                _viewsSet[view] = currentCount;
            }
            else
            {
                _viewsSet.Remove(view);
            }
            return;
        }
    }
}