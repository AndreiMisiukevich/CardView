using PanCardView;
using PanCardViewSample.ViewModels;
using Xamarin.Forms;

namespace PanCardViewSample.Views
{
	public class CarouselSampleDoubleView : ContentPage
	{
		public CarouselSampleDoubleView()
		{
			var cardsView = new CarouselView
			{
				ItemTemplate = new DataTemplate(GetCardItem)
			};
			cardsView.SetBinding(CardsView.ItemsProperty, "Items");
			cardsView.SetBinding(CardsView.CurrentIndexProperty, "CurrentIndex");


			Title = "CarouselView Double Carousel";

			Content = cardsView;
			BindingContext = new CarouselSampleDoubleViewModel();
		}


		private View GetCardItem()
		{
			var label = new Label
			{
				TextColor = Color.White,
				FontSize = 50,
				HorizontalTextAlignment = TextAlignment.Center,
				FontAttributes = FontAttributes.Bold
			};
			label.SetBinding(Label.TextProperty, "Number");
			AbsoluteLayout.SetLayoutFlags(label, AbsoluteLayoutFlags.All);
			AbsoluteLayout.SetLayoutBounds(label, new Rectangle(.5, 1, 1, .5));

			var cardsView = new CarouselView
			{
				ItemTemplate = new DataTemplate(() =>
				{
					var subCard = new ContentView();
					subCard.SetBinding(BackgroundColorProperty, "Color");
					return subCard;
				})
			};
			cardsView.SetBinding(CardsView.ItemsProperty, "Items");
			cardsView.SetBinding(CardsView.CurrentIndexProperty, "CurrentIndex");

			AbsoluteLayout.SetLayoutFlags(cardsView, AbsoluteLayoutFlags.All);
			AbsoluteLayout.SetLayoutBounds(cardsView, new Rectangle(.5, 0, 1, .5));

			return new AbsoluteLayout
			{
				BackgroundColor = Color.Black,
				Children =
				{
					cardsView,
					label
				}
			};
		}
	}
}