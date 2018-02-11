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
            var toCardsBtn = new Button { Text = "CardsView Items" };
            toCardsBtn.Clicked += (sender, e) => {
                this.Navigation.PushAsync(new CardsSampleView());
            };

            var toCardsBtnCustom = new Button { Text = "CardsView CustomContexts" };
            toCardsBtnCustom.Clicked += (sender, e) => {
                this.Navigation.PushAsync(new CardsSampleCustomView());
            };

            var toCarouselBtn = new Button { Text = "CarouselView" };
            toCarouselBtn.Clicked += (sender, e) => {
                this.Navigation.PushAsync(new CarouselSampleView());
            };

            var toCarouselStressBtn = new Button { Text = "CarouselView StressTest" };
            toCarouselStressBtn.Clicked += (sender, e) => {
                this.Navigation.PushAsync(new CarouselSampleStressView());
            };

            Content = new StackLayout
            {
                Children = {
                    toCardsBtn,
                    toCardsBtnCustom,
                    toCarouselBtn,
                    toCarouselStressBtn
                }
            };
        }
    }
}
