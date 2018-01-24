using System;
using System.Threading.Tasks;
using PanCardView.Enums;
using Xamarin.Forms;

namespace PanCardView.Processors
{
    public class BaseCarouselBackViewProcessor : ICardProcessor
    {
        protected uint AnimationLength { get; } = 250;

        public virtual void InitView(View view, CardsView cardsView, PanItemPosition panItemPosition)
        {
            if (view != null)
            {
                view.TranslationX = Math.Sign((int)panItemPosition) * cardsView.Width;
                view.IsVisible = false;
            }
        }

        public virtual void HandlePanChanged(View view, CardsView cardsView, double xPos, PanItemPosition panItemPosition)
        {
            var value = Math.Sign((int)panItemPosition) * cardsView.Width + xPos;
            if(Math.Abs(value) > cardsView.Width || (panItemPosition == PanItemPosition.Prev && value > 0) || (panItemPosition == PanItemPosition.Next && value < 0))
            {
                return;
            }
            view.TranslationX = value;
        }

        public virtual Task HandlePanReset(View view, CardsView cardsView, PanItemPosition panItemPosition)
        {
            if (view != null)
            {
                var tcs = new TaskCompletionSource<bool>();

                var animTimePercent = (cardsView.Width - Math.Abs(view.TranslationX)) / cardsView.Width;
                var animLength = (uint)(AnimationLength * animTimePercent);
                new Animation(v => view.TranslationX = v, view.TranslationX, Math.Sign((int)panItemPosition) * cardsView.Width)
                    .Commit(view, nameof(HandlePanReset), 16, animLength, Easing.SinIn, (v, t) => tcs.SetResult(true));
                return tcs.Task;
            }
            return Task.FromResult(true);
        }

        public virtual Task HandlePanApply(View view, CardsView cardsView, PanItemPosition panItemPosition)
        {
            if (view != null)
            {
                var tcs = new TaskCompletionSource<bool>();
                var animTimePercent = 1 - (cardsView.Width - Math.Abs(view.TranslationX)) / cardsView.Width;
                var animLength = (uint)(AnimationLength * animTimePercent);
                new Animation(v => view.TranslationX = v, view.TranslationX, 0)
                    .Commit(view, nameof(HandlePanApply), 16, animLength, Easing.SinIn, (v, t) => tcs.SetResult(true));
                return tcs.Task;
            }
            return Task.FromResult(true);
        }
    }
}
