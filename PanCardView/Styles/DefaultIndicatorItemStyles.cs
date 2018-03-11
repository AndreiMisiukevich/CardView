using System;
using Xamarin.Forms;
namespace PanCardView.Controls.Styles
{
	public static class DefaultIndicatorItemStyles
	{
		private static Style _defaultBaseIndicatorItemStyle;
		private static Style _defaultSelectedIndicatorItemStyle;
		private static Style _defaultUnselectedIndicatorItemStyle;

		static DefaultIndicatorItemStyles()
		{
		}

		public static Style DefaultBaseIndicatorItemStyle
		=> _defaultBaseIndicatorItemStyle ?? (_defaultBaseIndicatorItemStyle = new Style(typeof(Frame))
		{
			Setters = {
				new Setter { Property = VisualElement.HeightRequestProperty, Value = 10.0 },
				new Setter { Property = VisualElement.WidthRequestProperty, Value = 10.0 },
				new Setter { Property = Frame.CornerRadiusProperty, Value = 5.0f }
			}
		});

		public static Style DefaultSelectedIndicatorItemStyle
		=> _defaultSelectedIndicatorItemStyle ?? (_defaultSelectedIndicatorItemStyle = new Style(typeof(Frame))
		{
			BasedOn = DefaultBaseIndicatorItemStyle,
			Setters = {
				new Setter { Property = VisualElement.BackgroundColorProperty, Value = Color.White.MultiplyAlpha(.8) }
			}
		});

		public static Style DefaultUnselectedIndicatorItemStyle
		=> _defaultUnselectedIndicatorItemStyle ?? (_defaultUnselectedIndicatorItemStyle = new Style(typeof(Frame))
		{
			BasedOn = DefaultBaseIndicatorItemStyle,
			Setters = {
				new Setter { Property = VisualElement.BackgroundColorProperty, Value = Color.Transparent },
				new Setter { Property = Frame.OutlineColorProperty, Value = Color.White.MultiplyAlpha(.8) }
			}
		});
	}
}
