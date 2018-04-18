using System.Collections.ObjectModel;
using System.ComponentModel;
using Xamarin.Forms;

namespace PanCardViewSample.ViewModels
{
	public class CarouselSampleDoubleViewModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		private int _currentIndex;

		public CarouselSampleDoubleViewModel()
		{
			Items = new ObservableCollection<SubItem>
			{
				new SubItem(1),
				new SubItem(2),
				new SubItem(3),
				new SubItem(4),
				new SubItem(5)
			};
		}

		public ObservableCollection<SubItem> Items { get; }

		public int CurrentIndex
		{
			get => _currentIndex;
			set
			{
				_currentIndex = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentIndex)));
			}
		}
	}

	public class SubItem : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		private int _currentIndex;

		public SubItem(int number)
		{
			Number = number;
		}

		public int Number { get; }

		public ObservableCollection<object> Items { get; } = new ObservableCollection<object>
		{
			new
			{
				Color = Color.Yellow
			},
			new
			{
				Color = Color.Red
			}
		};

		public int CurrentIndex
		{
			get => _currentIndex;
			set
			{
				_currentIndex = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentIndex)));
			}
		}
	}
}