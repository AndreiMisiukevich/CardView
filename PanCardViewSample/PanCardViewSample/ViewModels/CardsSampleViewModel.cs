﻿using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Xamarin.Forms;
using System.Linq;
using FFImageLoading;
using PanCardView.Extensions;

namespace PanCardViewSample.ViewModels
{
	public sealed class CardsSampleViewModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		private int _currentIndex;
		private int _ImageCount = 500;

		public CardsSampleViewModel()
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
                if(IsAutoAnimationRunning || IsUserInteractionRunning)
                {
                    return;
                }

                var index = CurrentIndex + ((bool)v ? 1 : -1);
                if (index < 0 || index >= Items.Count)
                {
                    return;
                }
                CurrentIndex = index;
			});

			RemoveCurrentItemCommand = new Command(() =>
			{
				if (!Items.Any())
				{ 
					return;
				}
				Items.RemoveAt(CurrentIndex.ToCyclingIndex(Items.Count));
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

        public bool IsAutoAnimationRunning { get; set; }

        public bool IsUserInteractionRunning { get; set; }

        public ObservableCollection<object> Items { get; }

		private string CreateSource()
		{
			var source = $"https://picsum.photos/500/500?image={_ImageCount}";
			return source;
		}
	}
}