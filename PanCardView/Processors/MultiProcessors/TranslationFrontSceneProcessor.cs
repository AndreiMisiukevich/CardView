using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PanCardView;
using PanCardView.Enums;
using Xamarin.Forms;
using static System.Math;

namespace PanCardView.Processors.MultiProcessors
{
    public class TranslationFrontSceneProcessor : BaseTranslationProcessor
    {
        public double NoItemMaxPanDistance { get; set; } = 25;

        public override void HandleInitView(IEnumerable<View> views, CardsView cardsView, AnimationDirection animationDirection)
        {
            var view = views.FirstOrDefault();
            if (view != null)
            {
                view.TranslationX = 0;
                view.IsVisible = true;
            }

            views = SetupAppearingContexts(views);
        }

        public override void HandlePanChanged(IEnumerable<View> views, CardsView cardsView, double xPos, AnimationDirection animationDirection, IEnumerable<View> inactiveViews)
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

            var width = GetInitialPosition(cardsView);
            if (Abs(xPos) > width || (animationDirection == AnimationDirection.Prev && xPos < 0) || (animationDirection == AnimationDirection.Next && xPos > 0))
            {
                return;
            }

            if (animationDirection == AnimationDirection.Null)
            {
                xPos = Sign(xPos) * Min(Abs(xPos / 4), NoItemMaxPanDistance);
            }

            if (view != null)
            {
                view.TranslationX = xPos;
            }
        }

        public override Task HandleAutoNavigate(IEnumerable<View> views, CardsView cardsView, AnimationDirection animationDirection, IEnumerable<View> inactiveViews)
        {
            var view = views.FirstOrDefault();
            if (view == null)
            {
                return Task.FromResult(false);
            }

            var tcs = new TaskCompletionSource<bool>();
            view.IsVisible = true;
            new Animation(v => view.TranslationX = v, view.TranslationX, 0)
                .Commit(view, nameof(HandleAutoNavigate), 16, AnimationLength, AnimEasing, (d, b) => tcs.SetResult(true));
            return tcs.Task;
        }

        public override Task HandlePanReset(IEnumerable<View> views, CardsView cardsView, AnimationDirection animationDirection, IEnumerable<View> inactiveViews)
        {
            var view = views.FirstOrDefault();
            if (view != null)
            {
                var tcs = new TaskCompletionSource<bool>();
                var width = GetInitialPosition(cardsView);
                var animTimePercent = 1 - (width - Abs(view.TranslationX)) / width;
                var animLength = (uint)(AnimationLength * animTimePercent) * 3 / 2;
                new Animation(v => view.TranslationX = v, view.TranslationX, 0)
                    .Commit(view, nameof(HandlePanApply), 16, animLength, AnimEasing, (v, t) => tcs.SetResult(true));
                return tcs.Task;
            }
            return Task.FromResult(true);
        }

        public override Task HandlePanApply(IEnumerable<View> views, CardsView cardsView, AnimationDirection animationDirection, IEnumerable<View> inactiveViews)
        {
            views = SetupAppearingContexts(views);

            var view = views.FirstOrDefault();
            if (view != null)
            {
                var tcs = new TaskCompletionSource<bool>();
                var width = GetInitialPosition(cardsView);
                var animTimePercent = 1 - (width - Abs(view.TranslationX)) / width;
                var animLength = (uint)(AnimationLength * animTimePercent);
                new Animation(v =>
                {
                    view.TranslationX = v;
                }, view.TranslationX, 0)
                    .Commit(view, nameof(HandlePanApply), 16, animLength, AnimEasing, (v, t) => tcs.SetResult(true));

                return tcs.Task;
            }
            return Task.FromResult(true);
        }
    }
}
