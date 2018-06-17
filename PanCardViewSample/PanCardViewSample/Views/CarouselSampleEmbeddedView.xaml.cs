using Xamarin.Forms;

namespace PanCardViewSample.Views
{
	public partial class CarouselSampleEmbeddedView : ContentPage
	{
		public CarouselSampleEmbeddedView()
		{
			InitializeComponent();
		}

		protected override void LayoutChildren(double x, double y, double width, double height)
		{
			base.LayoutChildren(x, y, width, height);

			if (Device.RuntimePlatform == Device.iOS)
			{
				if (width > height)
				{
					Padding = new Thickness(0, 0, 0, 0);
				}
				else
				{
					Padding = new Thickness(0, 20, 0, 0);
				}
			}
		}
	}
}
