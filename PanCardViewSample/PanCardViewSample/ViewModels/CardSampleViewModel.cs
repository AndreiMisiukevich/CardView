using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Xamarin.Forms;
using System.Linq;

namespace PanCardViewSample.ViewModels
{
	public sealed class SharedSampleViewModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		private int _currentIndex;
		private int _ImageCount = 500;

		public SharedSampleViewModel()
		{
			Items = new ObservableCollection<object>
			{
				new { Source = CreateSource(), Ind = _ImageCount++, Color = Color.Red },
				new { Source = CreateSource(), Ind = _ImageCount++, Color = Color.Green },
				new { Source = CreateSource(), Ind = _ImageCount++, Color = Color.Gold },
				new { Source = CreateSource(), Ind = _ImageCount++, Color = Color.Silver },
				new { Source = CreateSource(), Ind = _ImageCount++, Color = Color.Blue }
			};

			PanPositionChangedCommand = new Command(v =>
			{
				var val = (bool)v;
				if (val)
				{
					CurrentIndex += 1;
					return;
				}

				CurrentIndex -= 1;
			});

			RemoveCurrentItemCommand = new Command(() =>
			{
				if (!Items.Any())
				{ 
					return;
				}
				Items.RemoveAt(CurrentIndex);
			});
		}

		public ICommand PanPositionChangedCommand { get; }

		public ICommand RemoveCurrentItemCommand { get; }

		public int CurrentIndex
		{
			get => _currentIndex;
			set
			{
				_currentIndex = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentIndex)));
			}
		}

		public ObservableCollection<object> Items { get; }

		private string CreateSource()
		{
			return $"http://lorempixel.com/300/300/animals/text{_ImageCount}/";
		}
	}
}