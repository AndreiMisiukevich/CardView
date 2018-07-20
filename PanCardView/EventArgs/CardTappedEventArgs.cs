using System;
namespace PanCardView.EventArgs
{
	public class CardTappedEventArgs : System.EventArgs
	{
		public CardTappedEventArgs(object item)
		{
			Item = item;
		}

		public object Item { get; }
	}
}
