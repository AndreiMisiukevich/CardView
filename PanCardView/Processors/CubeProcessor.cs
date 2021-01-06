using PanCardView.Extensions;
using Xamarin.Forms;
using static System.Math;
using static PanCardView.Processors.Constants;

namespace PanCardView.Processors
{
    public class CubeProcessor : CarouselProcessor
    {
        protected override double GetTranslationX(View view, CardsView cardsView)
            => view != null
            ? (cardsView.IsHorizontalOrientation ? view.RotationY : view.RotationX) * cardsView.GetSize() / Angle90
            : 0;

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

                if (Device.RuntimePlatform == Device.Android)
                {
                    var anchor = .5 * (1 - Sin(rotation * PI / Angle180));
                    if (cardsView.IsHorizontalOrientation)
                    {
                        view.AnchorX = anchor;
                        view.AnchorY = .5;
                    }
                    else
                    {
                        view.AnchorX = .5;
                        view.AnchorY = anchor;
                    }
                }

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
