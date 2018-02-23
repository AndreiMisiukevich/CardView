using PanCardView;
using PanCardView.Factory;
using Xamarin.Forms;
using PanCardViewSample.ViewModels;
using PanCardViewSample.CardsFactory;

namespace PanCardViewSample.Views
{
    public class CarouselSampleDoubleView : ContentPage
    {
        public CarouselSampleDoubleView()
        {
            var cardsView = new CarouselView
            {
                DataTemplate = new DataTemplate(GetCardItem),
                IsRecycled = true
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
                ItemViewFactory = new CardViewItemFactory(() => {
                    var subCard = new ContentView();
                    subCard.SetBinding(BackgroundColorProperty, "Color");
                    return subCard;
                }),
                IsRecycled = true
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
