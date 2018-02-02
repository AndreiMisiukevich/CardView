using System;
using Xamarin.Forms;
using System.Threading.Tasks;
using PanCardView.Enums;
using static PanCardView.Processors.Constants;

namespace PanCardView.Processors
{
    public class BaseCardFrontViewProcessor : ICardProcessor
    {
        public uint ResetAnimationLength { get; set; } = 150;

        public uint AutoNavigateAnimationLength { get; set; } = 150;

        public Easing ResetEasing { get; set; } = Easing.CubicInOut;

        public Easing AutoNavigateEasing { get; set; } = Easing.Linear;

        public virtual void InitView(View view, CardsView cardsView, PanItemPosition panItemPosition)
        => ResetInitialState(view);

        public virtual void AutoNavigate(View view, CardsView cardsView, PanItemPosition panItemPosition)
        {
            if (view != null)
            {
                view.IsVisible = true;
                new Animation(v => view.Scale = v, view.Scale, 1)
                    .Commit(view, nameof(AutoNavigate), 16, AutoNavigateAnimationLength, AutoNavigateEasing);
            }
        }

        public virtual void HandlePanChanged(View view, CardsView cardsView, double xPos, PanItemPosition panItemPosition)
        {
            view.TranslationX = xPos;
            view.TranslationY = Math.Abs(xPos) / 10;
            view.Rotation = 0.3 * Math.Min(xPos / cardsView.Width, 1) * Rad;
        }

        public virtual Task HandlePanReset(View view, CardsView cardsView, PanItemPosition panItemPosition)
        {
            var tcs = new TaskCompletionSource<bool>();

            if (!CheckIsInitialPosition(view))
            {
                var animLength = (uint)(ResetAnimationLength * Math.Min(Math.Abs(view.TranslationX / cardsView.MoveDistance), 1.0));
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

        public virtual Task HandlePanApply(View view, CardsView cardsView, PanItemPosition panItemPosition)
        {
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
