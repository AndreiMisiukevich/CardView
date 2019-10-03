using Xamarin.Forms;
using static System.Math;
using static PanCardView.Processors.Constants;

namespace PanCardView.Processors
{
    public class BaseCubeFrontViewProcessor : BaseCarouselFrontViewProcessor
    {
        protected override double GetTranslationX(View view)
        {
            if(view == null)
            {
                return 0;
            }

            var value = view.Margin.Left > 0
                ? view.Margin.Left
                : -view.Margin.Right;
            value += Sign(value) * view.Width * 0.5 * (1 - view.Scale);
            return value;
        }

        protected override void SetTranslationX(View view, double value, CardsView cardsView, bool? isVisible = null)
        {
            if (view == null)
            {
                return;
            }

            try
            {
                view.BatchBegin();
                view.Scale = CalculateFactoredProperty(value, ScaleFactor, cardsView);
                view.Opacity = CalculateFactoredProperty(value, OpacityFactor, cardsView);
                var margin = value - Sign(value) * view.Width * 0.5 * (1 - view.Scale);
                view.Margin = new Thickness(margin > 0 ? margin : 0, 0, margin < 0 ? -margin : 0, 0);
                view.RotationY = value * Angle90 / cardsView.Width;
                view.IsVisible = isVisible ?? view.IsVisible;
                cardsView.Diff = value;
            }
            finally
            {
                view.BatchCommit();
            }
        }
    }
}