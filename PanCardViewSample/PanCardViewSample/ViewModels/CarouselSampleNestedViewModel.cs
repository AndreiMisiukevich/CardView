using System;
using System.Collections.ObjectModel;
using Xamarin.Forms;

namespace PanCardViewSample.ViewModels
{
	public sealed class CarouselSampleNestedViewModel
	{
		public ObservableCollection<Item> Items { get; } = new ObservableCollection<Item>
		{
			new Item
            {
				Text = "FIRST",
				Color = Color.LightBlue,
				Items = new ObservableCollection<NestedItem>
                {
					new NestedItem { Text = "1/2 NESTED ITEM", Source = "https://picsum.photos/500/500?image=250" },
					new NestedItem { Text = "2/2 NESTED ITEM", Source = "https://picsum.photos/500/500?image=251" },
				}
            },
			new Item
			{
				Text = "SECOND",
				Color = Color.LightPink,
				Items = new ObservableCollection<NestedItem>
				{
					new NestedItem { Text = "1/2 NESTED ITEM", Source = "https://picsum.photos/500/500?image=252" },
					new NestedItem { Text = "2/2 NESTED ITEM", Source = "https://picsum.photos/500/500?image=253" },
				}
			},
			new Item
			{
				Text = "THIRD",
				Color = Color.LightSkyBlue,
				Items = new ObservableCollection<NestedItem>
				{
					new NestedItem { Text = "1/2 NESTED ITEM", Source = "https://picsum.photos/500/500?image=254" },
					new NestedItem { Text = "2/2 NESTED ITEM", Source = "https://picsum.photos/500/500?image=255" },
				}
			},
			new Item
			{
				Text = "FOURTH",
				Color = Color.LightGray,
				Items = new ObservableCollection<NestedItem>
				{
					new NestedItem { Text = "1/2 NESTED ITEM", Source = "https://picsum.photos/500/500?image=256" },
					new NestedItem { Text = "2/2 NESTED ITEM", Source = "https://picsum.photos/500/500?image=257" },
				}
			},
			new Item
			{
				Text = "FIFTH",
				Color = Color.LightGoldenrodYellow,
				Items = new ObservableCollection<NestedItem>
				{
					new NestedItem { Text = "1/2 NESTED ITEM", Source = "https://picsum.photos/500/500?image=257" },
					new NestedItem { Text = "2/2 NESTED ITEM", Source = "https://picsum.photos/500/500?image=258" },
				}
			},
		};

		public sealed class Item
		{
			public string Text { get; set; }
			public Color Color { get; set; }

			public ObservableCollection<NestedItem> Items { get; set; }
		}

		public sealed class NestedItem
        {
			public string Text { get; set; }
			public Color Color { get; set; }
			public string Source { get; set; }
		}
	}

	public class ColorItem
    {
		public Color Color { get; set; }
    }
}