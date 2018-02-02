using System;
using Xamarin.Forms;
using System.Threading.Tasks;
using PanCardView.Enums;
using static PanCardView.Processors.Constants;

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

        public virtual void InitView(View view, CardsView cardsView, PanItemPosition panItemPosition)
        {
            if(view != null)
            {
                view.TranslationX = 0;
                view.Rotation = 0;
                view.TranslationY = 0;
                view.Opacity = 1;
                view.IsVisible = false;
                view.Scale = InitialScale;
            }
        }

        public virtual void AutoNavigate(View view, CardsView cardsView, PanItemPosition panItemPosition)
        {
            if (view != null)
            {
                cardsView.AutoNavigatingStarted(view);
                new Animation(v => HandleAutoAnimatingPosChanged(view, cardsView, v, panItemPosition), 0, cardsView.MoveDistance)
                    .Commit(view, nameof(AutoNavigate), 16, AutoNavigateAnimationLength, AutoNavigateEasing, async (v, t) =>
                    {
                        await HandlePanApply(view, cardsView, panItemPosition);
                        cardsView.AutoNavigatingEnded(view);
                    });
            }
        }

        public virtual void HandlePanChanged(View view, CardsView cardsView, double xPos, PanItemPosition panItemPosition)
        {
            var calcScale = InitialScale + Math.Abs((xPos / cardsView.MoveDistance) * (1 - InitialScale));
            view.Scale = Math.Min(calcScale, 1);
        }

        public virtual Task HandlePanReset(View view, CardsView cardsView, PanItemPosition panItemPosition)
        {
            if(view != null)
            {
                var tcs = new TaskCompletionSource<bool>();
                var animLength = (uint)(ResetAnimationLength * (view.Scale - InitialScale) / (1 - InitialScale));
                new Animation(v => view.Scale = v, view.Scale, InitialScale)
                    .Commit(view, nameof(HandlePanReset), 16, animLength, ResetEasing, (v, t) => tcs.SetResult(true));
                return tcs.Task;
            }
            return Task.FromResult(true);
        }

        public virtual async Task HandlePanApply(View view, CardsView cardsView, PanItemPosition panItemPosition)
        {
            if (view != null)
            {
                await view.FadeTo(0, ApplyAnimationLength, ApplyEasing);
                view.IsVisible = false;
            }
        }

        private void HandleAutoAnimatingPosChanged(View view, CardsView cardsView, double xPos, PanItemPosition panItemPosition)
        {
            if(panItemPosition == PanItemPosition.Next)
            {
                xPos = -xPos;
            }

            view.TranslationX = xPos;
            view.TranslationY = Math.Abs(xPos) / 10;
            view.Rotation = 0.3 * Math.Min(xPos / cardsView.Width, 1) * Rad;
        }
    }
}
