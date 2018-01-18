using FFImageLoading.Forms;
using PanCardView;
using PanCardView.Factory;
using Xamarin.Forms;
using PanCardView.Processors;
using PanCardViewSample.ViewModels;
using PanCardViewSample.CardsFactory;

namespace PanCardViewSample.Views
{
    public class CardsSampleView : ContentPage
    {
        public CardsSampleView()
        {
            var cardsView = new CardsView()
            {
                ItemViewFactory = new CardViewItemFactory(RuleHolder.Rule),
                BackgroundColor = Color.Black.MultiplyAlpha(.9),
                IsPanInCourse = true
            };
            cardsView.SetBinding(CardsView.ItemsProperty, nameof(SharedSampleViewModel.Items));
            cardsView.SetBinding(CardsView.CurrentIndexProperty, nameof(SharedSampleViewModel.CurrentIndex));

            Title = "CardsView";
            Content = cardsView;
            BindingContext = new SharedSampleViewModel();
        }
    }
}
