using Xamarin.Forms;
using System.ComponentModel;

namespace PanCardView.Controls
{
    public class RightArrowControl : ArrowControl
    {
        public RightArrowControl()
        {
            AbsoluteLayout.SetLayoutBounds(this, new Rectangle(1, .5, -1, -1));
            Content = new Label
            {
                TextColor = Color.White.MultiplyAlpha(.7),
                VerticalTextAlignment = TextAlignment.Center,
                HorizontalTextAlignment = TextAlignment.Center,
                FontAttributes = FontAttributes.Bold,
                Margin = new Thickness(4, 0, 0, 0),
                FontSize = 20,
                Text = "➤"
            };
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public new static void Preserve()
        {
        }
    }
}
