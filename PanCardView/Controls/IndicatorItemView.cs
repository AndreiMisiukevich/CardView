using Xamarin.Forms;

namespace PanCardView.Controls
{
    public class IndicatorItemView : Frame
	{
		public IndicatorItemView()
		{
			VerticalOptions = LayoutOptions.Center;
			HorizontalOptions = LayoutOptions.Center;
			HasShadow = false;
			Padding = 0;
			HeightRequest = 10;
			WidthRequest = 10;
			CornerRadius = 5;
		}
	}
}
