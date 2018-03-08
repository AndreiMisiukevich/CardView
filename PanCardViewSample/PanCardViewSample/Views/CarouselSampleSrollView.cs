using PanCardView;
using Xamarin.Forms;
using PanCardViewSample.ViewModels;
using PanCardView.Controls;

namespace PanCardViewSample.Views
{
    public class CarouselSampleSrollView : ContentPage
    {
        private readonly CarouselView _carouselView;

        public CarouselSampleSrollView()
        {
            _carouselView = new CarouselView
            {
                DataTemplate = new DataTemplate(GetCardItem),
				Children = {
					new CustomIndicatorsControl()
				}
            };
            _carouselView.SetBinding(CardsView.ItemsProperty, nameof(CarouselSampleScrollViewModel.Items));

            Title = "CarouselView scroll";

            Content = _carouselView;
            BindingContext = new CarouselSampleScrollViewModel();
        }


        private View GetCardItem() => new CardItem();
    }

    public class CardItem : ContentView
    {
        public CardItem()
        {
            var label = new Label
            {
                TextColor = Color.White,
                FontSize = 70,
                WidthRequest = 280,
                HorizontalTextAlignment = TextAlignment.Center,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                FontAttributes = FontAttributes.Bold
            };
            label.SetBinding(Label.TextProperty, "Number");

			var scroller = new ScrollView
            {
                WidthRequest = 280,
                BackgroundColor = Color.Black,
                Content = new StackLayout
                {
                    WidthRequest = 280,
                    Children =
                    {
                        label,
                        new Label
                        {
                            WidthRequest = 280,
                            TextColor = Color.Gold,
                            FontSize = 50,
                            Text = "\nasfla jlasjf lkasj flkasjf lkajslk fjasl fjlas jflkjf alk sjflkasj lkasj flkasj flkajs lfjasl kfjaslk fjlaks jflask jfslka jlkaj flkasj faskf jalks fjlkas jflkas jfasj flas jfalks fjlaf "
                        }
                    }
                }
            };

            Content = scroller;
        }
    }

	public class CustomIndicatorsControl : IndicatorsControl
	{
		public CustomIndicatorsControl() : base(15)
		{
			Spacing = 7;
		}

		protected override void ApplySelectedStyle(View view, int index)
		{
			view.BackgroundColor = Color.Red.MultiplyAlpha(.95);
		}

		protected override void ApplyUnselectedStyle(View view, int index)
		{
			view.BackgroundColor = Color.Silver.MultiplyAlpha(.95);
		}
	}
}
