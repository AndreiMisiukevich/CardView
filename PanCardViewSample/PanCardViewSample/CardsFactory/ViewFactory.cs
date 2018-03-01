using System;
using FFImageLoading.Forms;
using Xamarin.Forms;

namespace PanCardViewSample.CardsFactory
{
    public static class ViewFactory
    {
        public static Func<View> Creator { get; } = () =>
        {
            var content = new AbsoluteLayout();
            var frame = new Frame
            {
                Padding = 0,
                HasShadow = false,
                CornerRadius = 10,
                IsClippedToBounds = true
            };
            frame.SetBinding(VisualElement.BackgroundColorProperty, "Color");
            content.Children.Add(frame, new Rectangle(.5, .5, 300, 300), AbsoluteLayoutFlags.PositionProportional);

            var image = new CachedImage
            {
                Aspect = Aspect.AspectFill
            };

            frame.Content = image;


            content.BindingContextChanged += (sender, e) =>
            {
                if (content?.BindingContext == null)
                {
                    return;
                }

                image.Source = ((dynamic)content.BindingContext).Source;
            };

            return content;
        };
    }
}
