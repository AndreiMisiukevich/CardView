// 01(c) Andrei Misiukevich
using System;

using Xamarin.Forms;
using PanCardView;
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace PanCardViewSample
{
    public class App : Application
    {
        public App()
        {
            var cardsView = new CardsView
            {
                ItemViewFactory = new CardViewItemFactory(SampleFactory.Rule),
                BackgroundColor = Color.Black.MultiplyAlpha(.8)
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
                var frame = new Frame { Padding = 0, HasShadow = false, CornerRadius = 10 };
                //view.SetBinding(VisualElement.BackgroundColorProperty, "Color");
                frame.Content = new Image();
                frame.Content.SetBinding(Image.SourceProperty, "Source");
                return frame;
            }
        };

        public override CardViewFactoryRule GetRule(object context) => Rule;
    }

    public sealed class SampleViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private int _currentIndex;
        private int _imageSize = 400;

        public SampleViewModel()
        {
            Items = new ObservableCollection<object>
            {
                new { Color = Color.Red, Source = CreateSource() },
                new { Color = Color.Yellow, Source = CreateSource() },
                new { Color = Color.Green, Source = CreateSource() },
                new { Color = Color.Blue, Source = CreateSource() },
                new { Color = Color.Black, Source = CreateSource() }
            };
        }

        public int CurrentIndex
        {
            get => _currentIndex;
            set
            {
                if(value + 1 >= Items.Count)
                {
                    var rand = new Random();
                    Items.Add(new 
                    {
                        Color = Color.FromRgb(rand.Next(0, 255), rand.Next(0, 255), rand.Next(0, 255)),
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
            var source = $"http://lorempixel.com/{_imageSize}/{_imageSize}/";
            _imageSize++;
            return source;
        }
    }
}
