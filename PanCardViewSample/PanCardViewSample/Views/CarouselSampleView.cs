using PanCardView;
using Xamarin.Forms;
using PanCardViewSample.ViewModels;
using PanCardViewSample.CardsFactory;
using PanCardView.Controls;

namespace PanCardViewSample.Views
{
    public class CarouselSampleView : ContentPage
    {
        public CarouselSampleView()
        {
			var carousel = new CarouselView
            {
				ItemTemplate = new DataTemplate(() => new DefaultCardItemView()),
                BackgroundColor = Color.Black.MultiplyAlpha(.9)
            };

            carousel.Children.Add(new IndicatorsControl());

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

            carousel.SetBinding(CardsView.ItemsProperty, nameof(SharedSampleViewModel.Items));
            carousel.SetBinding(CardsView.CurrentIndexProperty, nameof(SharedSampleViewModel.CurrentIndex));

            Title = "CarouselView";
			Content = carousel;
            BindingContext = new SharedSampleViewModel();
        }
    }
}
