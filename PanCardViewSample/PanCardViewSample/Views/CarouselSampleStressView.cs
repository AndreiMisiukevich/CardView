using PanCardView;
using PanCardView.Factory;
using Xamarin.Forms;
using PanCardViewSample.ViewModels;
using PanCardViewSample.CardsFactory;

namespace PanCardViewSample.Views
{
    public class CarouselSampleStressView : ContentPage
    {
        public CarouselSampleStressView()
        {
            var cardsView = new CarouselView
            {
                ItemViewFactory = new CardViewItemFactory(GetCardItem),
                IsRecycled = true
            };
            cardsView.SetBinding(CardsView.ItemsProperty, nameof(CarouselSampleStressViewModel.Items));

            Title = "CarouselView StressTest";

            Content = cardsView;
            BindingContext = new CarouselSampleStressViewModel();
        }


        private View GetCardItem()
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

            var content = new ContentView
            {
                BackgroundColor = Color.Black,
                Content = label
            };


            AbsoluteLayout.SetLayoutFlags(content, AbsoluteLayoutFlags.All);
            AbsoluteLayout.SetLayoutBounds(content, new Rectangle(0, 0, 1, 1));
            return content;
        }
    }
}
