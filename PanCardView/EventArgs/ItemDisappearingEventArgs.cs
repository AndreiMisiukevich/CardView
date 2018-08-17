using PanCardView.Enums;
using PanCardView.Utility;

namespace PanCardView.EventArgs
{
    public class ItemDisappearingEventArgs : System.EventArgs
    {
        public ItemDisappearingEventArgs(InteractionType type, bool isNextSelected, int index, object item)
        {
            Type = type;
            IsNextSelected = isNextSelected;
            Index = index;
            Item = item;
        }

        public InteractionType Type { get; }
        public bool IsNextSelected { get; }
        public int Index { get; }
        public object Item { get; }
    }
}
