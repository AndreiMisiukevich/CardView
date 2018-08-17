using PanCardView;
using PanCardViewSample.CardsFactory;
using PanCardViewSample.ViewModels;
using Xamarin.Forms;

namespace PanCardViewSample.Views
{
	public class CardsSampleView : ContentPage
	{
		public CardsSampleView()
		{
			var cardsView = new CardsView
			{
				ItemTemplate = new DataTemplate(() => new DefaultCardItemView()),
				BackgroundColor = Color.Black.MultiplyAlpha(.9)
			};
			AbsoluteLayout.SetLayoutFlags(cardsView, AbsoluteLayoutFlags.All);
			AbsoluteLayout.SetLayoutBounds(cardsView, new Rectangle(0, 0, 1, 1));

			var prevItem = new ToolbarItem
			{
				Text = "**Prev**",
				Icon = "prev",
				CommandParameter = false
			};
			prevItem.SetBinding(MenuItem.CommandProperty, nameof(CardsSampleViewModel.PanPositionChangedCommand));

			var nextItem = new ToolbarItem
			{
				Text = "**Next**",
				Icon = "next",
				CommandParameter = true
			};
			nextItem.SetBinding(MenuItem.CommandProperty, nameof(CardsSampleViewModel.PanPositionChangedCommand));

			ToolbarItems.Add(prevItem);
			ToolbarItems.Add(nextItem);

			cardsView.SetBinding(CardsView.ItemsSourceProperty, nameof(CardsSampleViewModel.Items));
			cardsView.SetBinding(CardsView.SelectedIndexProperty, nameof(CardsSampleViewModel.CurrentIndex));

			Title = "Cards View";


			var removeButton = new Button
			{
				Text = "Remove current",
				FontAttributes = FontAttributes.Bold,
				TextColor = Color.Black,
				BackgroundColor = Color.Yellow.MultiplyAlpha(.7),
				Margin = new Thickness(0, 0, 0, 10)
			};

			removeButton.SetBinding(Button.CommandProperty, nameof(CardsSampleViewModel.RemoveCurrentItemCommand));

			AbsoluteLayout.SetLayoutFlags(removeButton, AbsoluteLayoutFlags.PositionProportional);
			AbsoluteLayout.SetLayoutBounds(removeButton, new Rectangle(.5, 1, 150, 40));



			Content = new AbsoluteLayout()
			{
				Children =
				{
					cardsView,
					removeButton
				}
			};

			BindingContext = new CardsSampleViewModel();
		}
	}
}