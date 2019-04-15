using PanCardView.Controls;
namespace PanCardView
{
    public static class Preserver
    {
        public static void Preserve()
        {
            CardsView.Preserve();
            CarouselView.Preserve();
            CubeView.Preserve();
            CoverFlowView.Preserve();
            ArrowControl.Preserve();
            LeftArrowControl.Preserve();
            RightArrowControl.Preserve();
            CircleFrame.Preserve();
            IndicatorItemView.Preserve();
            IndicatorsControl.Preserve();
            ParentScrollView.Preserve();
        }
    }
}
