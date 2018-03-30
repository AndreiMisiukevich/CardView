using Xamarin.Forms;
using PanCardView.Controls;

namespace PanCardView.Extensions
{
	public static class CardViewExtensions
	{
		public static CardsView AsCardView(this BindableObject bindable)
		=> bindable as CardsView;

		public static IndicatorsControl AsIndicatorsControl(this BindableObject bindable)
		=> bindable as IndicatorsControl;

		public static View CreateView(this DataTemplate template)
		=> template.CreateContent() as View;

		public static int ToCyclingIndex(this int index, int itemsCount)
		{
			if (itemsCount <= 0)
			{
				return -1;
			}

			if (index < 0)
			{
				while (index < 0)
				{
					index += itemsCount;
				}
				return index;
			}

			while (index >= itemsCount)
			{
				index -= itemsCount;
			}
			return index;
		}
	}
}
