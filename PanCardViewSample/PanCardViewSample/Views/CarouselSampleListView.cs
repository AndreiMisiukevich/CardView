using Xamarin.Forms;
using PanCardViewSample.ViewModels;
using PanCardView;
using PanCardView.Extensions;
using CardCarouselView = PanCardView.CarouselView;

namespace PanCardViewSample.Views
{
	public class CarouselSampleListView : ContentPage
	{
		public CarouselSampleListView()
		{
			BindingContext = new CarouselSampleListViewModel();

			var carousel = new CardCarouselView
            {
                OppositePanDirectionDisablingThreshold = 1,
				ItemTemplate = new ListTemplateSelector()
			};

			carousel.SetBinding(CardsView.ItemsSourceProperty, nameof(CarouselSampleListViewModel.Cards));

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
			return carousel[0] == item 
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
					var label = new Label
					{
						TextColor = Color.White
					};
					label.SetBinding(Label.TextProperty, nameof(ListItemModel.Text));

					var content = new ContentView();
					content.Content = label;

					var tapGesture = new TapGestureRecognizer();
					tapGesture.SetBinding(TapGestureRecognizer.CommandProperty, nameof(ListItemModel.Command));
					content.GestureRecognizers.Add(tapGesture);

					return new ViewCell
					{
						View = content
					};
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
