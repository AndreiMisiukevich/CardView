using System.ComponentModel;
using System.Collections.Generic;
using System.Windows.Input;
using Xamarin.Forms;

namespace PanCardViewSample.ViewModels
{
	public class CarouselSampleListViewModel
	{
		public CarouselSampleListViewModel()
		{
			var itemModels = new List<ListItemModel>
			{
				new ListItemModel(),
				new ListItemModel()
			};

			Cards = new List<ListCardModel>
			{
				new ListCardModel(itemModels),
				new ListCardModel(itemModels)
			};
		}

		public List<ListCardModel> Cards { get; }
	}

	public class ListCardModel
	{
		public List<ListItemModel> Items { get; }

		public ListCardModel(List<ListItemModel> items)
		{
			Items = items;
		}
	}

	public class ListItemModel 
	{
		public string Text { get; } = "Test Item";

		private ICommand _command;
		public ICommand Command => _command ?? (_command = new Command(() =>
		{
			Application.Current.MainPage.DisplayAlert("Tap", null, "Ok");
		}));
	}
}
