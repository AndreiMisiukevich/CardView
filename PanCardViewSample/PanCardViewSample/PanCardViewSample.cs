using PanCardViewSample.Views;
using Xamarin.Forms;

namespace PanCardViewSample
{
	public class App : Application
	{
		public App()
		{
            if (Device.RuntimePlatform == Device.GTK)
            {
                MainPage = new Views.Gtk.CoverFlowSampleXamlView();
                return;
            }
            MainPage = new NavigationPage(new StartPage());
		}
	}

	public class StartPage : ContentPage
	{
		public StartPage()
		{
			Title = "PanCardViewSample";
			BackgroundColor = Color.White;

			var toCardsBtn = new Button { Text = "CardsView Items", FontSize = 20, TextColor = Color.Black };
			toCardsBtn.Clicked += (sender, e) =>
			{
				this.Navigation.PushAsync(new CardsSampleView());
			};

			var toCoverFlowBtn = new Button { Text = "CoverFlowView", FontSize = 20, TextColor = Color.Black };
            toCoverFlowBtn.Clicked += (sender, e) =>
			{
                this.Navigation.PushAsync(new CoverFlowSampleXamlView());
			};

			var toCarouselNestedBtn = new Button { Text = "CarouselView Nested" };
			toCarouselNestedBtn.Clicked += (sender, e) =>
			{
				this.Navigation.PushAsync(new CarouselSampleNestedXamlView());
			};

			var toCarouselScrollBtn = new Button { Text = "CarouselView scroll" };
			toCarouselScrollBtn.Clicked += (sender, e) =>
			{
				this.Navigation.PushAsync(new CarouselSampleSrollView());
			};

			var toCarouselNoTemplateBtn = new Button { Text = "CarouselView No template" };
			toCarouselNoTemplateBtn.Clicked += (sender, e) =>
			{
				this.Navigation.PushAsync(new CarouselSampleViewNoTemplate());
			};

			var toCarouselXamlBtn = new Button { Text = "CarouselView Xaml", FontSize = 20, TextColor = Color.Black };
			toCarouselXamlBtn.Clicked += (sender, e) =>
			{
				this.Navigation.PushAsync(new CarouselSampleXamlView());
			};

			var toCarouselListBtn = new Button { Text = "Carousel ListView" };
			toCarouselListBtn.Clicked += (sender, e) =>
			{
				this.Navigation.PushAsync(new CarouselSampleListView());
			};

			var toCarouselEmbBtn = new Button { Text = "Carousel Embedded views" };
			toCarouselEmbBtn.Clicked += (sender, e) =>
			{
				this.Navigation.PushAsync(new CarouselSampleEmbeddedView());
			};

            var toCubeBtn = new Button { Text = "CubeView Xaml", FontSize = 20, TextColor = Color.Black };
            toCubeBtn.Clicked += (sender, e) =>
            {
                this.Navigation.PushAsync(new CubeSampleXamlView());
            };

            var toXFIndicatorViewBtn = new Button { Text = "XF indicator view", FontSize = 20, TextColor = Color.Black };
            toXFIndicatorViewBtn.Clicked += (sender, e) =>
            {
                this.Navigation.PushAsync(new CarouselSampleXamlViewXFIndicatorView());
            };

            Content = new ScrollView
			{
				Content = new StackLayout
				{
					Margin = 20,
					Children = {
						toCardsBtn,
						toCarouselXamlBtn,
						toCoverFlowBtn,
						toCubeBtn,
						toCarouselNestedBtn,
						toCarouselScrollBtn,
						toCarouselNoTemplateBtn,
						toCarouselListBtn,
						toCarouselEmbBtn,
                        toXFIndicatorViewBtn
                    }
				}
			};
        }
	}
}