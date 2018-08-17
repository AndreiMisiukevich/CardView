using PanCardView.Enums;

namespace PanCardView.EventArgs
{
    public class ItemAppearingEventArgs : System.EventArgs
    {
        public ItemAppearingEventArgs(InteractionType type, bool isNextSelected, int index, object item)
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
