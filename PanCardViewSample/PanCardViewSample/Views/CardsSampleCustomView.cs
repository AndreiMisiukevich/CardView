using FFImageLoading.Forms;
using PanCardView;
using PanCardView.Factory;
using Xamarin.Forms;
using PanCardView.Processors;
using PanCardViewSample.ViewModels;
using PanCardViewSample.CardsFactory;

namespace PanCardViewSample.Views
{
    public class CardsSampleCustomView : ContentPage
    {
        public CardsSampleCustomView()
        {
            var cardsView = new CardsView()
            {
                ItemViewFactory = new CardViewItemFactory(RuleHolder.Rule),
                BackgroundColor = Color.Black.MultiplyAlpha(.9),
                IsPanInCourse = true
            };

            cardsView.SetBinding(CardsView.CurrentContextProperty, nameof(SharedSampleCustomViewModel.CurrentContext));
            cardsView.SetBinding(CardsView.NextContextProperty, nameof(SharedSampleCustomViewModel.NextContext));
            cardsView.SetBinding(CardsView.PrevContextProperty, nameof(SharedSampleCustomViewModel.PrevContext));

            cardsView.SetBinding(CardsView.PanStartedCommandProperty, nameof(SharedSampleCustomViewModel.PanStartedCommand));
            cardsView.SetBinding(CardsView.PositionChangedCommandProperty, nameof(SharedSampleCustomViewModel.PanPositionChangedCommand));

            Title = "CardsView";
            Content = cardsView;
            BindingContext = new SharedSampleCustomViewModel();
        }
    }
}
