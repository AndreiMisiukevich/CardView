using PanCardView;
using PanCardView.Controls;
using Xamarin.Forms;
namespace PanCardViewSample.Views
{
    public class CarouselSampleViewNoTemplate : ContentPage
    {
        public CarouselSampleViewNoTemplate()
        {
            Title = "CarouselSampleViewNoTemplate";

            var carousel = new CarouselView
            {
                HeightRequest = 200,
                ItemsSource = new[] {
                    new BoxView { Color = Color.Red },
                    new BoxView { Color = Color.Blue},
                    new BoxView { Color = Color.Yellow}
                },
                IsCyclical = false
            };

            var button = new Button { Text = "Select last" };
            button.Clicked += (sender, args) => { carousel.SelectedIndex = 2; };

            Content = new ParentScrollView
            {
                Content = new StackLayout
                {
                    Children = {
                        new StackLayout
                        {
                            Children = {
                                carousel
                            }
                        },
                        button
                    }
                }
            };
        }
    }
}