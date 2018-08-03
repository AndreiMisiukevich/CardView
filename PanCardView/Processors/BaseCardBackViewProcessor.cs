﻿using PanCardView.Enums;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using static PanCardView.Processors.Constants;
using static System.Math;

namespace PanCardView.Processors
{
    public class BaseCardBackViewProcessor : ICardProcessor
    {
        public double InitialScale { get; set; } = 0.8;

        public uint ApplyAnimationLength { get; set; } = 200;

        public uint ResetAnimationLength { get; set; } = 150;

        public uint AutoNavigateAnimationLength { get; set; } = 150;

        public Easing ApplyEasing { get; set; } = Easing.Linear;

        public Easing ResetEasing { get; set; } = Easing.CubicInOut;

        public Easing AutoNavigateEasing { get; set; } = Easing.Linear;

        public virtual void HandleInitView(IEnumerable<View> views, CardsView cardsView, AnimationDirection animationDirection)
        {
            var view = views.FirstOrDefault();
            if (view != null)
            {
                view.TranslationX = 0;
                view.Rotation = 0;
                view.TranslationY = 0;
                view.Opacity = 1;
                view.IsVisible = false;
                view.Scale = InitialScale;
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

            if (animationDirection == AnimationDirection.Null)
            {
                return;
            }

            var calcScale = InitialScale + Abs((xPos / cardsView.MoveDistance) * (1 - InitialScale));
            view.Scale = Min(calcScale, 1);
        }

        public virtual Task HandleAutoNavigate(IEnumerable<View> views, CardsView cardsView, AnimationDirection animationDirection, IEnumerable<View> inactiveViews)
        {
            var view = views.FirstOrDefault();
            if (view == null)
            {
                return Task.FromResult(false);
            }

            var tcs = new TaskCompletionSource<bool>();
            new Animation(v => HandleAutoAnimatingPosChanged(view, cardsView, v, animationDirection), 0, cardsView.MoveDistance)
                .Commit(view, nameof(HandleAutoNavigate), 16, AutoNavigateAnimationLength, AutoNavigateEasing, async (v, t) =>
                {
                    await HandlePanApply(views, cardsView, animationDirection, inactiveViews);
                    tcs.SetResult(true);
                });
            return tcs.Task;
        }

        public virtual Task HandlePanReset(IEnumerable<View> views, CardsView cardsView, AnimationDirection animationDirection, IEnumerable<View> inactiveViews)
        {
            var view = views.FirstOrDefault();
            if (view == null)
            {
                return Task.FromResult(true);
            }
            var animLength = (uint)(ResetAnimationLength * (view.Scale - InitialScale) / (1 - InitialScale));
            if (animLength == 0)
            {
                return Task.FromResult(true);
            }
            var tcs = new TaskCompletionSource<bool>();
            new Animation(v => view.Scale = v, view.Scale, InitialScale)
                .Commit(view, nameof(HandlePanReset), 16, animLength, ResetEasing, (v, t) => tcs.SetResult(true));
            return tcs.Task;
        }

        public virtual async Task HandlePanApply(IEnumerable<View> views, CardsView cardsView, AnimationDirection animationDirection, IEnumerable<View> inactiveViews)
        {
            var view = views.FirstOrDefault();
            if (view != null)
            {
                await view.FadeTo(0, ApplyAnimationLength, ApplyEasing);
                view.IsVisible = false;
            }
        }

        private void HandleAutoAnimatingPosChanged(View view, CardsView cardsView, double xPos, AnimationDirection animationDirection)
        {
            if (animationDirection == AnimationDirection.Next)
            {
                xPos = -xPos;
            }

            view.TranslationX = xPos;
            view.TranslationY = Abs(xPos) / 10;
            view.Rotation = 0.3 * Min(xPos / cardsView.Width, 1) * Rad;
        }
    }
}