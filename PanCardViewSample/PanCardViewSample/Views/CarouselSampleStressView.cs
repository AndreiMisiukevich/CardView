using PanCardView;
using Xamarin.Forms;
using PanCardViewSample.ViewModels;
using System;
using static System.Math;

namespace PanCardViewSample.Views
{
    public class CarouselSampleStressView : ContentPage
    {
        private readonly CarouselView _carouselView;

        public CarouselSampleStressView()
        {
            _carouselView = new CarouselView
            {
                DataTemplate = new DataTemplate(GetCardItem),
                IsRecycled = true
            };
            _carouselView.SetBinding(CardsView.ItemsProperty, nameof(CarouselSampleStressViewModel.Items));

            Title = "CarouselView StressTest";

            Content = _carouselView;
            BindingContext = new CarouselSampleStressViewModel();
        }


        private View GetCardItem() => new CardItem();
    }

    public class CardItem : ContentView, ICardItem
    {
        private readonly ScrollView _scroller;

        private double _prevScrollY;
        private bool? _handled;

        public CardItem()
        {
            var label = new Label
            {
                TextColor = Color.White,
                FontSize = 70,
                WidthRequest = 280,
                HorizontalTextAlignment = TextAlignment.Center,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                FontAttributes = FontAttributes.Bold
            };
            label.SetBinding(Label.TextProperty, "Number");

            _scroller = new ScrollView
            {
                WidthRequest = 280,
                BackgroundColor = Color.Black,
                Content = new StackLayout
                {
                    WidthRequest = 280,
                    Children =
                    {
                        label,
                        new Label
                        {
                            WidthRequest = 280,
                            TextColor = Color.Gold,
                            FontSize = 50,
                            Text = "\nasfla jlasjf lkasj flkasjf lkajslk fjasl fjlas jflkjf alk sjflkasj lkasj flkasj flkajs lfjasl kfjaslk fjlaks jflask jfslka jlkaj flkasj faskf jalks fjlkas jflkas jfasj flas jfalks fjlaf "
                        }
                    }
                },
                IsEnabled = false
            };

            Content = _scroller;
        }

        public bool HandeTouchChanged(double totalX, double totalY)
        {
            if(!_handled.HasValue)
            {
                var absX = Abs(totalX);
                var absY = Abs(totalY);
                _handled = absY > absX;
            }

            if(_handled.Value)
            {
                var y = _scroller.ScrollY - totalY + _prevScrollY;
                if(y < 0)
                {
                    _prevScrollY = 0;
                    return _handled.Value;
                }
                if(y > _scroller.Content.Height)
                {
                    _prevScrollY = _scroller.Content.Height;
                    return _handled.Value;
                }
                _scroller.ScrollToAsync(0, y, false);
                _prevScrollY = totalY;
            }

            return _handled.Value;
        }

        public void HandleTouchEnded(Guid touchId)
        {
            _handled = null;
            _prevScrollY = 0.0;
        }

        public void HandleTouchStarted(Guid touchId)
        {
            _handled = null;
            _prevScrollY = 0.0;
        }
    }
}
