using System.ComponentModel;
using Xamarin.Forms;
using static PanCardView.Resources.ResourcesInfo;

namespace PanCardView.Controls
{
    public class LeftArrowControl : ArrowControl
    {
        public LeftArrowControl()
        {
            IsRight = false;
            AbsoluteLayout.SetLayoutBounds(this, new Rectangle(0, .5, -1, -1));
            ImageSource = WhiteLeftArrowImageSource;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public new static void Preserve()
        {
        }
    }
}
