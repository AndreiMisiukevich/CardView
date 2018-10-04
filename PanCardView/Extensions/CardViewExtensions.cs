using PanCardView.Controls;
using Xamarin.Forms;

namespace PanCardView.Extensions
{
    public static class CardViewExtensions
    {
        public static CardsView AsCardsView(this BindableObject bindable)
        => bindable as CardsView;

        public static CarouselView AsCarouselView(this BindableObject bindable)
        => bindable as CarouselView;

#pragma warning disable
        public static SceneView AsSceneView(this BindableObject bindable)
        => bindable as SceneView;

        public static CoverFlow AsCoverView(this BindableObject bindable)
        => bindable as CoverFlow;
#pragma warning restore

        public static IndicatorsControl AsIndicatorsControl(this BindableObject bindable)
        => bindable as IndicatorsControl;

        public static CircleFrame AsCircleFrame(this BindableObject bindable)
        => bindable as CircleFrame;

        public static ArrowControl AsArrowControl(this BindableObject bindable)
        => bindable as ArrowControl;

        public static View CreateView(this DataTemplate template)
        => template.CreateContent() as View;

        public static int ToCyclingIndex(this int index, int itemsCount)
        {
            if (itemsCount <= 0)
            {
                return -1;
            }
            if (index < 0)
            {
                while (index < 0)
                {
                    index += itemsCount;
                }
            }
            else
            {
                while (index >= itemsCount)
                {
                    index -= itemsCount;
                }
            }
            return index;
        }
    }
}