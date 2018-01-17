using Xamarin.Forms;

namespace PanCardViewSample
{
    public class App : Application
    {
        public App()
        {
            MainPage = new NavigationPage(new CardsSampleView());
        }
    }
}
