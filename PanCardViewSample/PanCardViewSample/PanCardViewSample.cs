using Xamarin.Forms;
using PanCardViewSample.Views;

namespace PanCardViewSample
{
    public class App : Application
    {
        public App()
        {
            MainPage = new NavigationPage(new StartPage());
        }
    }

    public class StartPage : ContentPage
    {
        public StartPage()
        {
            var toCardsBtn = new Button { Text = "CardsView" };
            toCardsBtn.Clicked += (sender, e) => {
                this.Navigation.PushAsync(new CardsSampleView());
            };

            var toCarouselBtn = new Button { Text = "CarouselView" };
            toCarouselBtn.Clicked += (sender, e) => {
                this.Navigation.PushAsync(new CarouselSampleView());
            };

            Content = new StackLayout
            {
                Children = {
                    toCardsBtn,
                    toCarouselBtn
                }
            };
        }
    }
}
