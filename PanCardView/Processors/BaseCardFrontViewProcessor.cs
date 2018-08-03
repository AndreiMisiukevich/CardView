using PanCardView.Enums;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using static PanCardView.Processors.Constants;
using static System.Math;

namespace PanCardView.Processors
{
    public class BaseCardFrontViewProcessor : ICardProcessor
    {
        public uint ResetAnimationLength { get; set; } = 150;

        public uint AutoNavigateAnimationLength { get; set; } = 150;

        public Easing ResetEasing { get; set; } = Easing.CubicInOut;

        public Easing AutoNavigateEasing { get; set; } = Easing.Linear;

        public double NoItemMaxPanDistance { get; set; } = 25;

        public virtual void HandleInitView(IEnumerable<View> views, CardsView cardsView, AnimationDirection animationDirection)
        => ResetInitialState(views.FirstOrDefault());

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

            var multiplier = 1;
            if (animationDirection == AnimationDirection.Null)
            {
                xPos = Sign(xPos) * Min(Abs(xPos / 4), NoItemMaxPanDistance);
                multiplier = -multiplier;
            }

            if (view != null)
            {
                view.TranslationX = xPos;
                view.TranslationY = multiplier * Abs(xPos) / 10;
                view.Rotation = multiplier * 0.3 * Rad * (xPos / cardsView.Width);
            }
        }

        public virtual Task HandleAutoNavigate(IEnumerable<View> views, CardsView cardsView, AnimationDirection animationDirection, IEnumerable<View> inactiveViews)
        {
            var view = views.FirstOrDefault();
            if (view == null)
            {
                return Task.FromResult(false);
            }

            var tcs = new TaskCompletionSource<bool>();
            view.IsVisible = true;
            new Animation(v => view.Scale = v, view.Scale, 1)
                .Commit(view, nameof(HandleAutoNavigate), 16, AutoNavigateAnimationLength, AutoNavigateEasing, (d, b) => tcs.SetResult(true));
            return tcs.Task;
        }

        public virtual Task HandlePanReset(IEnumerable<View> views, CardsView cardsView, AnimationDirection animationDirection, IEnumerable<View> inactiveViews)
        {
            var view = views.FirstOrDefault();
            var tcs = new TaskCompletionSource<bool>();

            if (!CheckIsInitialPosition(view))
            {
                var animLength = (uint)(ResetAnimationLength * Min(Abs(view.TranslationX / cardsView.MoveDistance), 1.0));

                if (animLength == 0)
                {
                    SetInitialResult(view, tcs);
                    return tcs.Task;
                }

                new Animation {
                    { 0, 1, new Animation (v => view.TranslationX = v, view.TranslationX, 0) },
                    { 0, 1, new Animation (v => view.TranslationY = v, view.TranslationY, 0) },
                    { 0, 1, new Animation (v => view.Rotation = v, view.Rotation, 0) }
                }.Commit(view, nameof(HandlePanReset), 16, animLength, ResetEasing, (v, c) => SetInitialResult(view, tcs));
            }
            else
            {
                SetInitialResult(view, tcs);
            }

            return tcs.Task;
        }

        public virtual Task HandlePanApply(IEnumerable<View> views, CardsView cardsView, AnimationDirection animationDirection, IEnumerable<View> inactiveViews)
        {
            var view = views.FirstOrDefault();
            if (view != null)
            {
                view.Scale = 1;
            }
            return Task.FromResult(true);
        }

        protected virtual void ResetInitialState(View view, bool isVisible = true)
        {
            if (view != null)
            {
                view.Scale = 1;
                view.Opacity = 1;
                view.TranslationX = 0;
                view.Rotation = 0;
                view.TranslationY = 0;
                view.IsVisible = isVisible;
            }
        }

        private bool CheckIsInitialPosition(View view)
        => (int)view.TranslationX == 0 && (int)view.TranslationY == 0 && (int)view.Rotation == 0;

        private void SetInitialResult(View view, TaskCompletionSource<bool> tcs)
        {
            ResetInitialState(view);
            tcs.SetResult(true);
        }
    }
}