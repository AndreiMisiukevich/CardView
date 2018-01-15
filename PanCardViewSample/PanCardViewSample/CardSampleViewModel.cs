using System.Collections.ObjectModel;
using System.ComponentModel;
using Xamarin.Forms;
using System;

namespace PanCardViewSample
{
    public sealed class PanCardSampleViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private int _currentIndex;
        private int _ImageCount = 500;

        public PanCardSampleViewModel()
        {
            Items = new ObservableCollection<object>
            {
                new { Source = CreateSource(), Ind = _ImageCount++, Color = Color.Red },
                new { Source = CreateSource(), Ind = _ImageCount++, Color = Color.Green },
                new { Source = CreateSource(), Ind = _ImageCount++, Color = Color.Gold },
                new { Source = CreateSource(), Ind = _ImageCount++, Color = Color.Silver },
                new { Source = CreateSource(), Ind = _ImageCount++, Color = Color.Blue }
            };
        }

        public int CurrentIndex
        {
            get => _currentIndex;
            set
            {
                var rnd = new Random();
                if (value + 1 >= Items.Count)
                {
                    Items.Add(new
                    {
                        Source = CreateSource(),
                        Ind = _ImageCount++,
                        Color = Color.FromRgb(rnd.Next(0, 255), rnd.Next(0, 255), rnd.Next(0, 255))
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
            return $"http://lorempixel.com/400/400/sports/IMAGE{_ImageCount}/";
        }
    }
}
