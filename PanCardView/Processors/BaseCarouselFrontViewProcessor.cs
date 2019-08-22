using PanCardView.Enums;
using PanCardView.Utility;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using static System.Math;

namespace PanCardView.Processors
{
    public class BaseCarouselFrontViewProcessor : ICardProcessor
    {
        public uint AnimationLength { get; set; } = 300;

        public Easing AnimEasing { get; set; } = Easing.SinInOut;

        public double NoItemMaxPanDistance { get; set; } = 25;

        public double ScaleFactor { get; set; } = 1;

        public double OpacityFactor { get; set; } = 1;

        public virtual void HandleInitView(IEnumerable<View> views, CardsView cardsView, AnimationDirection animationDirection)
        {
            var view = views.FirstOrDefault();
            if (view != null)
            {
                view.BatchBegin();
                SetTranslationX(view, 0, cardsView);
                view.IsVisible = true;
                view.BatchCommit();
            }
        }

        public virtual void HandlePanChanged(IEnumerable<View> views, CardsView cardsView, double xPos, AnimationDirection animationDirection, IEnumerable<View> inactiveViews)
        {
            var view = views.FirstOrDefault();
            var inactiveView = inactiveViews.FirstOrDefault();

            if (view != null)
            {
                view.IsVisible = true;
            }
            if (inactiveView != null)
            {
                inactiveView.IsVisible = false;
            }

            if (Abs(xPos) > cardsView.Width || (animationDirection == AnimationDirection.Prev && xPos < 0) || (animationDirection == AnimationDirection.Next && xPos > 0))
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

        public virtual Task HandleAutoNavigate(IEnumerable<View> views, CardsView cardsView, AnimationDirection animationDirection, IEnumerable<View> inactiveViews)
        {
            var view = views.FirstOrDefault();
            if (view == null)
            {
                return Task.FromResult(false);
            }

            view.IsVisible = true;
            return new AnimationWrapper(v => SetTranslationX(view, v, cardsView), GetTranslationX(view), 0)
                .Commit(view, nameof(HandleAutoNavigate), 16, AnimationLength, AnimEasing);
        }

        public virtual Task HandlePanReset(IEnumerable<View> views, CardsView cardsView, AnimationDirection animationDirection, IEnumerable<View> inactiveViews)
        {
            var view = views.FirstOrDefault();
            if (view == null)
            {
                return Task.FromResult(true);
            }
            var animTimePercent = 1 - (cardsView.Width - Abs(GetTranslationX(view))) / cardsView.Width;
            var animLength = (uint)(AnimationLength * animTimePercent) * 3 / 2;
            if (animLength == 0)
            {
                return Task.FromResult(true);
            }
            return new AnimationWrapper(v => SetTranslationX(view, v, cardsView), GetTranslationX(view), 0)
                .Commit(view, nameof(HandlePanApply), 16, animLength, AnimEasing);
        }

        public virtual Task HandlePanApply(IEnumerable<View> views, CardsView cardsView, AnimationDirection animationDirection, IEnumerable<View> inactiveViews)
        {
            var view = views.FirstOrDefault();
            if (view == null)
            {
                return Task.FromResult(true);
            }
            var animTimePercent = 1 - (cardsView.Width - Abs(GetTranslationX(view))) / cardsView.Width;
            var animLength = (uint)(AnimationLength * animTimePercent);
            if (animLength == 0)
            {
                return Task.FromResult(true);
            }
            return new AnimationWrapper(v => SetTranslationX(view, v, cardsView), GetTranslationX(view), 0)
                .Commit(view, nameof(HandlePanApply), 16, animLength, AnimEasing);
        }

        protected virtual double GetTranslationX(View view)
            => view?.TranslationX ?? 0;

        protected virtual void SetTranslationX(View view, double value, CardsView cardsView)
        {
            view.Scale = CalculateFactoredProperty(value, ScaleFactor, cardsView);
            view.Opacity = CalculateFactoredProperty(value, OpacityFactor, cardsView);
            view.TranslationX = value;
        }

        protected virtual double CalculateFactoredProperty(double value, double factor, CardsView cardsView)
            => (factor - 1) / cardsView.Width * Abs(value) + 1;
    }
}