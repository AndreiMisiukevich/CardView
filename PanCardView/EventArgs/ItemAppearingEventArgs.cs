using PanCardView.Enums;

namespace PanCardView.EventArgs
{
    public class ItemAppearingEventArgs : System.EventArgs
    {
        public ItemAppearingEventArgs(InteractionType type, bool isNextSelected, object item)
        {
            Type = type;
            IsNextSelected = isNextSelected;
            Item = item;
        }

        public InteractionType Type { get; }
        public bool IsNextSelected { get; }
        public object Item { get; }
    }
}
