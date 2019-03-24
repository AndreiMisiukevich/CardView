using System.ComponentModel;
using Xamarin.Forms;

namespace PanCardView.Controls
{
    public class LeftArrowControl : ArrowControl
    {
        public LeftArrowControl()
        {
            IsRight = false;
            AbsoluteLayout.SetLayoutBounds(this, new Rectangle(0, .5, -1, -1));
            Content = new Label
            {
                TextColor = Color.White.MultiplyAlpha(.7),
                VerticalTextAlignment = TextAlignment.Center,
                HorizontalTextAlignment = TextAlignment.Center,
                FontAttributes = FontAttributes.Bold,
                Margin = new Thickness(0, 0, 4, 0),
                FontSize = 20,
                Text = "➤",
                Rotation = 180
            };
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public new static void Preserve()
        {
        }
    }
}
