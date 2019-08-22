using Xamarin.Forms;
using static PanCardView.Processors.Constants;

namespace PanCardView.Processors
{
    public class BaseCubeFrontViewProcessor : BaseCarouselFrontViewProcessor
    {
        protected override double GetTranslationX(View view)
            => view.Margin.Left > 0
                ? view.Margin.Left
                : -view.Margin.Right;

        protected override void SetTranslationX(View view, double value, CardsView cardsView)
        {
            try
            {
                view.BatchBegin();
                view.Scale = CalculateFactoredProperty(value, ScaleFactor, cardsView);
                view.Opacity = CalculateFactoredProperty(value, OpacityFactor, cardsView);
                view.Margin = new Thickness(value > 0 ? value : 0, 0, value < 0 ? -value : 0, 0);
                view.RotationY = value * Angle90 / cardsView.Width;
            }
            finally
            {
                view.BatchCommit();
            }
        }
    }
}