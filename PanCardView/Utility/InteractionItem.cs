using System;
using System.Collections.Concurrent;
using PanCardView.Enums;

namespace PanCardView.Utility
{
    public sealed class InteractionItem
    {
        private static readonly ConcurrentBag<InteractionItem> _itemsPool = new ConcurrentBag<InteractionItem>();

        public static InteractionItem GetItem(Guid id, InteractionType type, InteractionState state)
        {
            if(!_itemsPool.TryTake(out InteractionItem item))
            {
                item = new InteractionItem();
            }
            item.Id = id;
            item.Type = type;
            item.State = state;
            item.WasTouchChanged = false;
            return item;
        }

        public static InteractionItem PutItem(InteractionItem item)
        {
            _itemsPool.Add(item);
            return item;
        }

        private InteractionItem()
        {
        }

        public Guid Id { get; set; }
        public InteractionType Type { get; set; }
        public InteractionState State { get; set; }
        public bool WasTouchChanged { get; set; }
    }
}
