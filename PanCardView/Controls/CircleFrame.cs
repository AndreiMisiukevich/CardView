using System.ComponentModel;
using PanCardView.Extensions;
using Xamarin.Forms;
using static System.Math;

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
            VerticalOptions = LayoutOptions.Center;
            HorizontalOptions = LayoutOptions.Center;
            HasShadow = false;
            Padding = 0;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void Preserve()
        {
        }

        public double Size
        {
            get => (double)GetValue(SizeProperty);
            set => SetValue(SizeProperty, value);
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            if(width > 0 && height > 0)
            {
                SetCornerRadius(Min(width, height));
            }
        }

        protected void OnSizeUpdated()
        {
            var size = Size;
            if(size < 0)
            {
                return;
            }

            try
            {
                BatchBegin();
                HeightRequest = size;
                WidthRequest = size;
                SetCornerRadius(size);
            }
            finally
            {
                BatchCommit();
            }
        }

        private void SetCornerRadius(double size)
            => CornerRadius = (float)size / 2;
    }
}
