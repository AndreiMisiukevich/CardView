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

			var toCarouselScrollBtn = new Button { Text = "CarouselView scroll" };
			toCarouselScrollBtn.Clicked += (sender, e) =>
			{
				this.Navigation.PushAsync(new CarouselSampleSrollView());
			};

			var toCarouselDoubleBtn = new Button { Text = "CarouselView DoubleView" };
			toCarouselDoubleBtn.Clicked += (sender, e) =>
			{
				this.Navigation.PushAsync(new CarouselSampleDoubleView());
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

            Content = new ScrollView
			{
				Content = new StackLayout
				{
					Children = {
						toCardsBtn,
						toCarouselXamlBtn,
						toCoverFlowBtn,
						toCubeBtn,
						toCarouselScrollBtn,
						toCarouselDoubleBtn,
						toCarouselNoTemplateBtn,
						toCarouselListBtn,
						toCarouselEmbBtn
                    }
				}
			};
        }
	}
}