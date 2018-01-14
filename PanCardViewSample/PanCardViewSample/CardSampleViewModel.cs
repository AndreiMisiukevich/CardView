using System.Collections.ObjectModel;
using System.ComponentModel;

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
                if (value + 1 >= Items.Count)
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
            _ImageCount++;
            return $"http://lorempixel.com/400/400/sports/IMAGE{_ImageCount}/";
        }
    }
}
