using PanCardView.Enums;
using PanCardView.Extensions;
using PanCardView.Utility;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using static System.Math;

namespace PanCardView.Processors
{
    public class BaseCoverFlowFrontViewProcessor : BaseCarouselFrontViewProcessor
    {
        public BaseCoverFlowFrontViewProcessor()
        {
            NoItemMaxPanDistance = 0;
        }

        public override void HandleInitView(IEnumerable<View> views, CardsView cardsView, AnimationDirection animationDirection)
        {
            var view = views.FirstOrDefault();
            if (view != null)
            {
                SetTranslationX(view, 0, cardsView, true);
            }
        }

        public override void HandlePanChanged(IEnumerable<View> views, CardsView cardsView, double xPos, AnimationDirection animationDirection, IEnumerable<View> inactiveViews)
        {
            var view = views.FirstOrDefault();

            if (Abs(xPos) > GetStep(cardsView) || (animationDirection == AnimationDirection.Prev && xPos < 0) || (animationDirection == AnimationDirection.Next && xPos > 0))
            {
                return;
            }

            if (animationDirection == AnimationDirection.Null)
            {
                xPos = Sign(xPos) * Min(Abs(xPos / 4), NoItemMaxPanDistance);
            }

            if (view != null)
            {
                SetTranslationX(view, xPos, cardsView);
            }
        }

        public override Task HandleAutoNavigate(IEnumerable<View> views, CardsView cardsView, AnimationDirection animationDirection, IEnumerable<View> inactiveViews)
        {
            var view = views.FirstOrDefault();
            if (view == null)
            {
                return Task.FromResult(false);
            }

            return new AnimationWrapper(v => SetTranslationX(view, v, cardsView), GetTranslationX(view), 0)
                .Commit(view, nameof(HandleAutoNavigate), 16, AnimationLength, AnimEasing);
        }

        public override Task HandlePanReset(IEnumerable<View> views, CardsView cardsView, AnimationDirection animationDirection, IEnumerable<View> inactiveViews)
        {
            var view = views.FirstOrDefault();
            if (view == null)
            {
                return Task.FromResult(true);
            }
            var step = GetStep(cardsView);

            var animTimePercent = 1 - (step - Abs(GetTranslationX(view))) / step;
            var animLength = (uint)(AnimationLength * animTimePercent) * 3 / 2;
            if (animLength == 0)
            {
                return Task.FromResult(true);
            }
            return new AnimationWrapper(v => SetTranslationX(view, v, cardsView), GetTranslationX(view), 0)
                .Commit(view, nameof(HandlePanApply), 16, animLength, AnimEasing);
        }

        public override Task HandlePanApply(IEnumerable<View> views, CardsView cardsView, AnimationDirection animationDirection, IEnumerable<View> inactiveViews)
        {
            var view = views.FirstOrDefault();
            if (view == null)
            {
                return Task.FromResult(true);
            }
            var step = GetStep(cardsView);

            var animTimePercent = 1 - (step - Abs(GetTranslationX(view))) / step;
            var animLength = (uint)(AnimationLength * animTimePercent);
            if (animLength == 0)
            {
                return Task.FromResult(true);
            }
            return new AnimationWrapper(v => SetTranslationX(view, v, cardsView), GetTranslationX(view), 0)
                .Commit(view, nameof(HandlePanApply), 16, animLength, AnimEasing);
        }

        private double GetStep(CardsView cardsView)
        {
            var coverFlowView = cardsView.AsCoverFlowView();
            return cardsView.Size * (1 - coverFlowView.PositionShiftPercentage) - coverFlowView.PositionShiftValue;
        }
    }
}