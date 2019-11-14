using PanCardView.Enums;
using PanCardView.Utility;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using static System.Math;
using PanCardView.Extensions;

namespace PanCardView.Processors
{
    public class BaseCoverFlowBackViewProcessor : BaseCarouselBackViewProcessor
    {
        public override void HandleInitView(IEnumerable<View> views, CardsView cardsView, AnimationDirection animationDirection)
        {
            var index = 0;
            foreach (var view in views ?? Enumerable.Empty<View>())
            {
                ++index;
                if (view == null)
                {
                    continue;
                }
                SetTranslationX(view, Sign((int)animationDirection)
                    * (cardsView.IsRightToLeftFlowDirectionEnabled ? -1 : 1)
                    * GetStep(cardsView)
                    * index, cardsView, true);
            }
        }

        public override void HandleCleanView(IEnumerable<View> views, CardsView cardsView)
        {
            foreach (var view in views ?? Enumerable.Empty<View>())
            {
                SetTranslationX(view, cardsView.Size, cardsView, false, true);
            }
        }

        public override void HandlePanChanged(IEnumerable<View> views, CardsView cardsView, double xPos, AnimationDirection animationDirection, IEnumerable<View> inactiveViews)
        {
            var view = views.FirstOrDefault();
            if (animationDirection == AnimationDirection.Null || view == null)
            {
                HandleInitView(views, cardsView, (AnimationDirection)Sign(GetTranslationX(view)));
                HandleInitView(inactiveViews, cardsView, (AnimationDirection)Sign(GetTranslationX(inactiveViews?.FirstOrDefault())));
                return;
            }

            var step = GetStep(cardsView);
            var checkValue = Sign((int)animationDirection) * step + xPos;
            if (Abs(checkValue) > step || (animationDirection == AnimationDirection.Prev && checkValue > 0) || (animationDirection == AnimationDirection.Next && checkValue < 0))
            {
                return;
            }

            var otherViews = views.Union(inactiveViews ?? Enumerable.Empty<View>()).Except(Enumerable.Repeat(view, 1));
            ProceedPositionChanged(Sign((int)animationDirection) * step + xPos, view, otherViews, cardsView);
        }

        public override Task HandleAutoNavigate(IEnumerable<View> views, CardsView cardsView, AnimationDirection animationDirection, IEnumerable<View> inactiveViews)
        {
            var view = views.FirstOrDefault();
            if (view == null)
            {
                return Task.FromResult(false);
            }

            var otherViews = views.Union(inactiveViews ?? Enumerable.Empty<View>()).Except(Enumerable.Repeat(view, 1));
            return new AnimationWrapper(v => ProceedPositionChanged(v, view, otherViews, cardsView), 0, -Sign((int)animationDirection) * GetStep(cardsView))
                .Commit(view, nameof(HandleAutoNavigate), 16, AnimationLength, AnimEasing);
        }

        public override Task HandlePanReset(IEnumerable<View> views, CardsView cardsView, AnimationDirection animationDirection, IEnumerable<View> inactiveViews)
        {
            var view = views.FirstOrDefault();
            if (view == null || view == cardsView.CurrentView)
            {
                return Task.FromResult(true);
            }
            var step = GetStep(cardsView);

            var animTimePercent = (step - Abs(GetTranslationX(view))) / step;
            var animLength = (uint)(AnimationLength * animTimePercent) * 3 / 2;
            if (animLength == 0)
            {
                return Task.FromResult(true);
            }

            var otherViews = views.Union(inactiveViews ?? Enumerable.Empty<View>()).Except(Enumerable.Repeat(view, 1));
            return new AnimationWrapper(v => ProceedPositionChanged(v, view, otherViews, cardsView), GetTranslationX(view), Sign((int)animationDirection) * step)
                .Commit(view, nameof(HandlePanReset), 16, animLength, AnimEasing);
        }

        public override Task HandlePanApply(IEnumerable<View> views, CardsView cardsView, AnimationDirection animationDirection, IEnumerable<View> inactiveViews)
        {
            var view = views.FirstOrDefault();
            if (view == null)
            {
                return Task.FromResult(true);
            }
            var step = GetStep(cardsView);

            var animTimePercent = (step - Abs(GetTranslationX(view))) / step;
            var animLength = (uint)(AnimationLength * animTimePercent);
            if (animLength == 0)
            {
                return Task.FromResult(true);
            }

            var otherViews = views.Union(inactiveViews ?? Enumerable.Empty<View>()).Except(Enumerable.Repeat(view, 1));
            return new AnimationWrapper(v => ProceedPositionChanged(v, view, otherViews, cardsView), GetTranslationX(view), -Sign((int)animationDirection) * step)
                .Commit(view, nameof(HandlePanReset), 16, animLength, AnimEasing);
        }

        private double GetStep(CardsView cardsView)
        {
            var coverFlowView = cardsView.AsCoverFlowView();
            return cardsView.Size * (1 - coverFlowView.PositionShiftPercentage) - coverFlowView.PositionShiftValue;
        }
            
        private void ProceedPositionChanged(double value, View checkView, IEnumerable<View> views, CardsView cardsView)
        {
            var diff = GetTranslationX(checkView) - value;
            SetTranslationX(checkView, value, cardsView);

            foreach (var view in views ?? Enumerable.Empty<View>())
            {
                if(view == null)
                {
                    continue;
                }
                SetTranslationX(view, GetTranslationX(view) - diff, cardsView);
            }
        }
    }
}