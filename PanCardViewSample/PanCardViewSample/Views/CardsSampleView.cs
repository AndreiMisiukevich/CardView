using FFImageLoading.Forms;
using PanCardView;
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
            var cardsView = new CardsView
            {
                DataTemplate = new DataTemplate(() => ViewFactory.Creator.Invoke()),
                BackgroundColor = Color.Black.MultiplyAlpha(.9),
                IsRecycled = true
            };

            var prevItem = new ToolbarItem
            {
                Text = "**Prev**",
                Icon = "prev",
                CommandParameter = false
            };
            prevItem.SetBinding(MenuItem.CommandProperty, nameof(SharedSampleViewModel.PanPositionChangedCommand));

            var nextItem = new ToolbarItem
            {
                Text = "**Next**",
                Icon = "next",
                CommandParameter = true
            };
            nextItem.SetBinding(MenuItem.CommandProperty, nameof(SharedSampleViewModel.PanPositionChangedCommand));

            ToolbarItems.Add(prevItem);
            ToolbarItems.Add(nextItem);

            cardsView.SetBinding(CardsView.ItemsProperty, nameof(SharedSampleViewModel.Items));
            cardsView.SetBinding(CardsView.CurrentIndexProperty, nameof(SharedSampleViewModel.CurrentIndex));

            Title = "CardsView";
            Content = cardsView;
            BindingContext = new SharedSampleViewModel();
        }
    }
}
