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

        public static SceneView AsSceneView(this BindableObject bindable)
        => bindable as SceneView;

        public static IndicatorsControl AsIndicatorsControl(this BindableObject bindable)
        => bindable as IndicatorsControl;

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
                return index;
            }

            while (index >= itemsCount)
            {
                index -= itemsCount;
            }
            return index;
        }
    }
}