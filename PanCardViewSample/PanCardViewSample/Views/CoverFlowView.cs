using System;
using FFImageLoading.Forms;
using PanCardView;
using PanCardView.Enums;
using PanCardViewSample.ViewModels;
using Xamarin.Forms;

namespace PanCardViewSample.Views
{
    public class CoverFlowView : ContentPage
    {
        public CoverFlowView(double width)
        {
            var itemTemplate = new DataTemplate(() =>
            {
                var layout = new AbsoluteLayout()
                {
                    WidthRequest = width / 3,
                };
                var fLabel = new Frame()
                {
                    CornerRadius = 5,
                    Padding = 0
                };
                var frame = new Frame()
                {
                    Padding = 0,
                    HasShadow = false,
                    CornerRadius = 10,
                    IsClippedToBounds = true
                };
                layout.Children.Add(frame, new Rectangle(.5, .5, width / 3, width / 3), AbsoluteLayoutFlags.PositionProportional);
                layout.Children.Add(fLabel, new Rectangle(.5, .6, width / 8, width / 12), AbsoluteLayoutFlags.PositionProportional);

                fLabel.SetBinding(BackgroundColorProperty, "Color");
                frame.SetBinding(BackgroundColorProperty, "Color");
                var label = new Label
                {
                    TextColor = Color.White,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center,
                    FontAttributes = FontAttributes.Bold,
                    FontSize = 16
                };
                label.SetBinding(Label.TextProperty, "Text");
                fLabel.Content = label;

                var image = new CachedImage
                {
                    Aspect = Aspect.AspectFill,
                };
                //image.SetBinding(CachedImage.SourceProperty, "Source");
                //frame.Content = image;

                return layout;
            });

            var coverFlowLeft = new CoverFlow
            {
                ItemTemplate = itemTemplate,
                ViewPosition = Position.Left,
                FirstItemPosition = Position.Left,
                Spacing = 20,
                IsCyclical = false
            };
            var coverFlowCentered = new CoverFlow
            {
                ItemTemplate = itemTemplate,
                ViewPosition = Position.Center,
                FirstItemPosition = Position.Center,
                Spacing = 20,
                IsCyclical = true
            };
            var coverFlowRight = new CoverFlow
            {
                ItemTemplate = itemTemplate,
                ViewPosition = Position.Left,
                FirstItemPosition = Position.Left,
                Spacing = 20,
                IsCyclical = true
            };

            coverFlowLeft.SetBinding(CoverFlow.ItemsSourceProperty, nameof(CoverFlowViewModel.Items));
            coverFlowCentered.SetBinding(CoverFlow.ItemsSourceProperty, nameof(CoverFlowViewModel.Items));
            coverFlowRight.SetBinding(CoverFlow.ItemsSourceProperty, nameof(CoverFlowViewModel.Items));

            BackgroundColor = Color.Black;
            Title = "CoverFlowView";

            var scrollView = new ScrollView()
            {
                Orientation = ScrollOrientation.Vertical,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand,
            };
            var stack = new StackLayout
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand,
                Spacing = 5,
            };


            stack.Children.Add(new Label() {Text = "Simple List positionned on Left", TextColor = Color.White });
            stack.Children.Add(coverFlowLeft);
            stack.Children.Add(new Label() { Text = "Circular List positionned on Center", TextColor = Color.White });
            stack.Children.Add(coverFlowCentered);
            stack.Children.Add(new Label() { Text = "Cicular List positionned on Left", TextColor = Color.White});
            stack.Children.Add(coverFlowRight);

            scrollView.Content = stack;

            Content = scrollView;
            BindingContext = new CoverFlowViewModel();
        }
    }
}