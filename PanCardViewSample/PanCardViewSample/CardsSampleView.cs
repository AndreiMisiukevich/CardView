// 11(c) Andrei Misiukevich
using System;
using FFImageLoading.Forms;
using PanCardView;
using PanCardView.Factory;
using Xamarin.Forms;
namespace PanCardViewSample
{
    public class CardsSampleView : ContentPage
    {
        public CardsSampleView()
        {
            var cardsView = new CardsView
            {
                ItemViewFactory = new CardViewItemFactory(RuleHolder.Rule),
                BackgroundColor = Color.Black.MultiplyAlpha(.9),
                //IsOnlyForwardDirection = true
            };
            cardsView.SetBinding(CardsView.ItemsProperty, nameof(PanCardSampleViewModel.Items));
            cardsView.SetBinding(CardsView.CurrentIndexProperty, nameof(PanCardSampleViewModel.CurrentIndex));

            Title = "PanCardViewSample";
            Content = cardsView;
            BindingContext = new PanCardSampleViewModel();
        }
    }

    public static class RuleHolder
    {
        public static CardViewFactoryRule Rule { get; } = new CardViewFactoryRule
        {
            Creator = () =>
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

                //var image = new CachedImage
                //{
                //    Aspect = Aspect.AspectFill
                //};
                //image.SetBinding(CachedImage.SourceProperty, "Source");

                //frame.Content = image;
                return content;
            }
        };
    }
}
