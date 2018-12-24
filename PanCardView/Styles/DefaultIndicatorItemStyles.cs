using Xamarin.Forms;
namespace PanCardView.Controls.Styles
{
    public static class DefaultIndicatorItemStyles
    {
        private static Style _defaultSelectedIndicatorItemStyle;
        private static Style _defaultUnselectedIndicatorItemStyle;

        static DefaultIndicatorItemStyles()
        {
        }

        public static Style DefaultSelectedIndicatorItemStyle
        => _defaultSelectedIndicatorItemStyle ?? (_defaultSelectedIndicatorItemStyle = new Style(typeof(Frame))
        {
            Setters = {
                new Setter { Property = VisualElement.BackgroundColorProperty, Value = Color.White.MultiplyAlpha(.8) }
            }
        });

        public static Style DefaultUnselectedIndicatorItemStyle
        => _defaultUnselectedIndicatorItemStyle ?? (_defaultUnselectedIndicatorItemStyle = new Style(typeof(Frame))
        {
            Setters = {
                new Setter { Property = VisualElement.BackgroundColorProperty, Value = Color.Transparent },
                new Setter { Property = Frame.BorderColorProperty, Value = Color.White.MultiplyAlpha(.8) }
            }
        });
    }
}