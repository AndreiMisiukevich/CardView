using PanCardView;
using PanCardViewSample.CardsFactory;
using PanCardViewSample.ViewModels;
using Xamarin.Forms;

namespace PanCardViewSample.Views
{
    public class CardsSampleCustomView : ContentPage
    {
        public CardsSampleCustomView()
        {
            var cardsView = new CardsView
            {
				ItemTemplate = new DataTemplate(() => new DefaultCardItemView()),
                BackgroundColor = Color.Black.MultiplyAlpha(.9),
                IsPanInCourse = true
            };

            var prevItem = new ToolbarItem
            {
                Text = "**Prev**",
                Icon = "prev",
                CommandParameter = false
            };
            prevItem.SetBinding(MenuItem.CommandProperty, nameof(SharedSampleCustomViewModel.PanPositionChangedCommand));

            var nextItem = new ToolbarItem
            {
                Text = "**Next**",
                Icon = "next",
                CommandParameter = true
            };
            nextItem.SetBinding(MenuItem.CommandProperty, nameof(SharedSampleCustomViewModel.PanPositionChangedCommand));

            ToolbarItems.Add(prevItem);
            ToolbarItems.Add(nextItem);

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
