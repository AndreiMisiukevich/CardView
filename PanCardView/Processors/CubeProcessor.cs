using PanCardView.Extensions;
using Xamarin.Forms;
using static System.Math;
using static PanCardView.Processors.Constants;

namespace PanCardView.Processors
{
    public class CubeProcessor : CarouselProcessor
    {
        protected override double GetTranslationX(View view, CardsView cardsView)
        {
            if (view == null)
            {
                return 0;
            }

            var value = cardsView.IsHorizontalOrientation
                ? view.Margin.Left > 0
                    ? view.Margin.Left
                    : -view.Margin.Right
                : view.Margin.Top > 0
                    ? view.Margin.Top
                    : -view.Margin.Bottom;
            value += Sign(value) * cardsView.GetSize(view) * 0.5 * (1 - view.Scale);
            return value;
        }

        protected override void SetTranslationX(View view, double value, CardsView cardsView, bool isFront, bool? isVisible = null, bool isClean = false)
        {
            if (view == null || !CheckSize(view, cardsView, value, isVisible, isFront, isClean))
            {
                return;
            }

            try
            {
                view.BatchBegin();
                view.Scale = CalculateFactoredProperty(value, ScaleFactor, cardsView);
                view.Opacity = CalculateFactoredProperty(value, OpacityFactor, cardsView);
                var margin = value - Sign(value) * cardsView.GetSize(view) * 0.5 * (1 - view.Scale);
                var rotation = value * Angle90 / cardsView.GetSize();
                if (cardsView.IsHorizontalOrientation)
                {
                    view.Margin = new Thickness(margin > 0 ? margin : 0, 0, margin < 0 ? -margin : 0, 0);
                    view.RotationY = rotation;
                }
                else
                {
                    view.Margin = new Thickness(0, margin > 0 ? margin : 0, 0, margin < 0 ? -margin : 0);
                    view.RotationX = rotation;
                }
                view.IsVisible = isVisible ?? view.IsVisible;
                if (isFront)
                {
                    cardsView.ProcessorDiff = value;
                }
            }
            finally
            {
                view.BatchCommit();
            }
        }
    }
}
