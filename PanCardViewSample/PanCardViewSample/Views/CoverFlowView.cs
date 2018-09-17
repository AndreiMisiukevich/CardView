using System;
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
                    var c = new Frame()
                    {
                        CornerRadius = 20
                    };
                    c.SetBinding(BackgroundColorProperty, "Color");
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
                    c.Content = label;
                    var l = new AbsoluteLayout();
                    l.Children.Add(c, new Rectangle(.5, .5, width / 4, width / 4), AbsoluteLayoutFlags.PositionProportional);
                    return l;
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