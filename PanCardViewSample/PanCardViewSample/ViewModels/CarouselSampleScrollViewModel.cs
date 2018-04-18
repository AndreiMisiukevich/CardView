using System.Collections.ObjectModel;
using System.Linq;

namespace PanCardViewSample.ViewModels
{
	public class CarouselSampleScrollViewModel
	{
		public CarouselSampleScrollViewModel()
		{
			Items = new ObservableCollection<object>(Enumerable.Range(1, 3).Select(i => new { Number = i }).ToArray());
		}

		public ObservableCollection<object> Items { get; }
	}
}