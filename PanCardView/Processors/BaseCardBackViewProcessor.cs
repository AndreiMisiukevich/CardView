using System;
using Xamarin.Forms;
using System.Threading.Tasks;
using PanCardView.Enums;

namespace PanCardView.Processors
{
    public class BaseCardBackViewProcessor : ICardProcessor
    {
        protected double InitialScale { get; } = 0.8;
        protected uint ResetAnimationLength { get; } = 150;

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
                var animLength = (uint)(ResetAnimationLength * (view.Scale - InitialScale) * 5);
                new Animation(v => view.Scale = v, view.Scale, InitialScale)
                    .Commit(view, nameof(HandlePanReset), 16, animLength, finished: (v, t) => tcs.SetResult(true));
                return tcs.Task;
            }
            return Task.FromResult(true);
        }

        public virtual Task HandlePanApply(View view, CardsView cardsView, PanItemPosition panItemPosition)
        {
            if (view != null)
            {
                view.Scale = 1;
            }
            return Task.FromResult(true);
        }
    }
}
