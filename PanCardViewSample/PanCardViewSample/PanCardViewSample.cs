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
                ItemViewFactory = new SampleFactory()
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
        private readonly CardViewFactoryRule _rule = new CardViewFactoryRule
        {
            Creator = () =>
            {
                var view = new ContentView();
                view.SetBinding(VisualElement.BackgroundColorProperty, "Color");
                return view;
            }
        };

        public override CardViewFactoryRule GetRule(object context) => _rule;
    }

    public sealed class SampleViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private int _currentIndex;

        public SampleViewModel()
        {
            Items = new ObservableCollection<object>
            {
                new { Color = Color.Red },
                new { Color = Color.Yellow },
                new { Color = Color.Green },
                new { Color = Color.Blue },
                new { Color = Color.Black }
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
                    Items.Add(new { Color = Color.FromRgb(rand.Next(0, 255), rand.Next(0, 255), rand.Next(0, 255)) });
                    Items.RemoveAt(0);
                }
                _currentIndex = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentIndex)));
            }
        }

        public ObservableCollection<object> Items { get; }
    }
}
