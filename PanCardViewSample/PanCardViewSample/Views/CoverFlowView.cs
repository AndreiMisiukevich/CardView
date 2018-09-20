using System;
using FFImageLoading.Forms;
using PanCardView;
using PanCardViewSample.ViewModels;
using Xamarin.Forms;

namespace PanCardViewSample.Views
{
    public class CoverFlowView : ContentPage
    {
        public CoverFlowView(double width)
        {
            var coverFlow = new CoverFlow
            {
                ItemTemplate = new DataTemplate(() =>
                {
                    var layout = new AbsoluteLayout();
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
                })
            };

            coverFlow.SetBinding(CoverFlow.ItemsSourceProperty, nameof(CoverFlowViewModel.Items));

            BackgroundColor = Color.Black;
            Title = "CoverFlowView";
            Content = coverFlow;
            BindingContext = new CoverFlowViewModel();
        }
    }
}