using System;
using Xamarin.Forms;
using PanCardViewSample.ViewModels;
using PanCardView;
using System.Collections.Generic;
using PanCardView.Extensions;
namespace PanCardViewSample.Views
{
	public class CarouselSampleListView : ContentPage
	{
		public CarouselSampleListView()
		{
			BindingContext = new CarouselSampleListViewModel();

			var carousel = new CarouselView
			{
				ItemTemplate = new ListTemplateSelector()
			};

			carousel.SetBinding(CardsView.ItemsProperty, nameof(CarouselSampleListViewModel.Cards));

			Content = carousel;
		}
	}

	public class ListTemplateSelector : DataTemplateSelector
	{
		public DataTemplate RedTemplate { get; }
		public DataTemplate GreenTemplate { get; }

		public ListTemplateSelector()
		{
			RedTemplate = new DataTemplate(() => BuildCard(Color.Red));
			GreenTemplate = new DataTemplate(() => BuildCard(Color.Green));
		}

		protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
		{
			var carousel = container.AsCardsView();
			return carousel.Items[0] == item 
				           ? RedTemplate 
				           : GreenTemplate;
		}

		private object BuildCard(Color color)
		{
			var list = new ListView
			{
				BackgroundColor = color,
				ItemTemplate = new DataTemplate(() =>
				{
					var cell = new TextCell
					{
						TextColor = Color.White
					};
					cell.SetBinding(TextCell.TextProperty, nameof(ListItemModel.Text));
					cell.SetBinding(TextCell.CommandProperty, nameof(ListItemModel.Command));
					return cell;
				})
			};
			list.SetBinding(ItemsView<Cell>.ItemsSourceProperty, nameof(ListCardModel.Items));

			return new ContentView
			{
				Margin = new Thickness(30, 20),
				Content = list
			};
		}
	}
}
