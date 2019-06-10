using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Xamarin.Forms;
using System.Linq;
using FFImageLoading;
using PanCardView.Extensions;

namespace PanCardViewSample.ViewModels
{
	public sealed class PanoramaSampleViewModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		public PanoramaSampleViewModel()
		{
			Items = new ObservableCollection<string>
			{
				"pan0",
                "pan1",
                "pan2"
			};
		}

        public ObservableCollection<string> Items { get; }
	}
}