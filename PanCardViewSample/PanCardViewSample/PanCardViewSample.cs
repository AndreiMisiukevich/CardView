// 01(c) Andrei Misiukevich
using System;

using Xamarin.Forms;
using PanCardView;
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections.ObjectModel;
using FFImageLoading.Forms;

namespace PanCardViewSample
{
    public class App : Application
    {
        public App()
        {
            var cardsView = new CardsView
            {
                ItemViewFactory = new CardViewItemFactory(SampleFactory.Rule),
                BackgroundColor = Color.Black.MultiplyAlpha(.9)
            };
            cardsView.SetBinding(CardsView.ItemsProperty, nameof(SampleViewModel.Items));
            cardsView.SetBinding(CardsView.CurrentIndexProperty, nameof(SampleViewModel.CurrentIndex));

            var content = new ContentPage
            {
                Title = "PanCardViewSample",
                Content = cardsView,
                BindingContext = new SampleViewModel()
            };

            MainPage = new NavigationPage(content);
        }
    }

    public sealed class SampleFactory : CardViewItemFactory
    {
        public static CardViewFactoryRule Rule { get; } = new CardViewFactoryRule
        {
            Creator = () =>
            {
                var content = new AbsoluteLayout();
                var frame = new Frame 
                {
                    Padding = 0, 
                    HasShadow = false,
                    CornerRadius = 10,
                    IsClippedToBounds = true
                };
                content.Children.Add(frame, new Rectangle(.5, .5, 300, 300), AbsoluteLayoutFlags.PositionProportional);

                var image = new CachedImage
                {
                    Aspect = Aspect.AspectFill
                };
                image.SetBinding(CachedImage.SourceProperty, "Source");

                frame.Content = image;
                return content;
            }
        };

        public override CardViewFactoryRule GetRule(object context) => Rule;
    }

    public sealed class SampleViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private int _currentIndex;
        private int _imageSize = 500;

        public SampleViewModel()
        {
            Items = new ObservableCollection<object>
            {
                new { Source = CreateSource() },
                new { Source = CreateSource() },
                new { Source = CreateSource() },
                new { Source = CreateSource() },
                new { Source = CreateSource() }
            };
        }

        public int CurrentIndex
        {
            get => _currentIndex;
            set
            {
                if(value + 1 >= Items.Count)
                {
                    Items.Add(new 
                    {
                        Source = CreateSource()
                    });
                    Items.RemoveAt(0);
                }
                _currentIndex = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentIndex)));
            }
        }

        public ObservableCollection<object> Items { get; }

        private string CreateSource()
        {
            _imageSize++;
            return $"http://lorempixel.com/{_imageSize}/{_imageSize}/";
        }
    }
}
