using PanCardView;
using PanCardViewSample.ViewModels;
using Xamarin.Forms;

namespace PanCardViewSample.Views
{
    public class SceneSampleView : ContentPage
	{
		public SceneSampleView(double width)
		{
			var carousel = new SceneView
			{
				ItemTemplate = new DataTemplate(() => {
					var c = new ContentView();
					c.SetBinding(BackgroundColorProperty, "Color");
					var l = new AbsoluteLayout();
					l.Children.Add(c, new Rectangle(.5, .5, width / 4, width / 4), AbsoluteLayoutFlags.PositionProportional);
					return l;
				})
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

			carousel.SetBinding(CardsView.ItemsProperty, nameof(SharedSampleViewModel.Items));
			carousel.SetBinding(CardsView.CurrentIndexProperty, nameof(SharedSampleViewModel.CurrentIndex));

			BackgroundColor = Color.Black;
			Title = "SceneView";
			Content = carousel;
			BindingContext = new SharedSampleViewModel();
		}
	}
}
