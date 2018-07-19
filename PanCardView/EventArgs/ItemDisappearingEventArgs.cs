using PanCardView.Enums;
using PanCardView.Utility;

namespace PanCardView.EventArgs
{
	public class ItemDisappearingEventArgs
	{
		public ItemDisappearingEventArgs(InteractionType type, bool isNextSelected, object item)
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
