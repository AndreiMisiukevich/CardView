using PanCardView.Extensions;
using Xamarin.Forms;

namespace PanCardView.Controls
{
    public class CircleFrame : Frame
    {
        public static readonly BindableProperty SizeProperty = BindableProperty.Create(nameof(Size), typeof(double), typeof(CircleFrame), 0.0, propertyChanged: (bindable, oldValue, newValue) =>
        {
            bindable.AsCircleFrame().OnSizeUpdated();
        });

        public CircleFrame()
        {
            HasShadow = false;
            VerticalOptions = LayoutOptions.Center;
            HorizontalOptions = LayoutOptions.Center;
            HasShadow = false;
            Padding = 0;
        }

        public double Size
        {
            get => (double)GetValue(SizeProperty);
            set => SetValue(SizeProperty, value);
        }

        protected void OnSizeUpdated()
        {
            try
            {
                BatchBegin();
                HeightRequest = Size;
                WidthRequest = Size;
                CornerRadius = (float)Size / 2;
            }
            finally
            {
                BatchCommit();
            }
        }
    }
}
