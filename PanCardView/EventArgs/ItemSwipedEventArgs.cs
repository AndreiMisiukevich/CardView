using System;
using PanCardView.Enums;
namespace PanCardView.EventArgs
{
    public class ItemSwipedEventArgs : System.EventArgs
    {
        public ItemSwipedEventArgs(SwipeDirection direction, int index, object item)
        {
            Direction = direction;
            Index = index;
            Item = item;
        }

        public SwipeDirection Direction { get; }
        public object Item { get; }
        public int Index { get; }
    }
}
