using System.Threading.Tasks;
using PanCardView.Enums;
using Xamarin.Forms;

using static System.Math;

namespace PanCardView.Processors
{
    public class BaseCarouselBackViewProcessor : ICardProcessor
    {
        public uint AnimationLength { get; set; } = 300;

        public Easing AnimEasing { get; set; } = Easing.SinInOut;

        public virtual void InitView(View view, CardsView cardsView, PanItemPosition panItemPosition)
        {
            if (view != null)
            {
                view.TranslationX = Sign((int)panItemPosition) * cardsView.Width;
                view.IsVisible = false;
            }
        }

        public virtual void AutoNavigate(View view, CardsView cardsView, PanItemPosition panItemPosition)
        {
            if (view != null)
            {
                var destinationPos = panItemPosition == PanItemPosition.Prev
                   ? cardsView.Width
                   : -cardsView.Width;
                
                cardsView.AutoNavigatingStarted(view);
                new Animation(v => view.TranslationX = v, 0, destinationPos)
                    .Commit(view, nameof(AutoNavigate), 16, AnimationLength, AnimEasing, (v, t) =>
                    {
                        cardsView.AutoNavigatingEnded(view);
                    });
            }
        }

        public virtual void HandlePanChanged(View view, CardsView cardsView, double xPos, PanItemPosition panItemPosition)
        {
            if (panItemPosition == PanItemPosition.Null)
            {
                return;
            }

            var value = Sign((int)panItemPosition) * cardsView.Width + xPos;
            if(Abs(value) > cardsView.Width || (panItemPosition == PanItemPosition.Prev && value > 0) || (panItemPosition == PanItemPosition.Next && value < 0))
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

                var animTimePercent = (cardsView.Width - Abs(view.TranslationX)) / cardsView.Width;
                var animLength = (uint)(AnimationLength * animTimePercent) * 3 / 2;
                new Animation(v => view.TranslationX = v, view.TranslationX, Sign((int)panItemPosition) * cardsView.Width)
                    .Commit(view, nameof(HandlePanReset), 16, animLength, AnimEasing, (v, t) => tcs.SetResult(true));
                return tcs.Task;
            }
            return Task.FromResult(true);
        }

        public virtual Task HandlePanApply(View view, CardsView cardsView, PanItemPosition panItemPosition)
        {
            if (view != null)
            {
                var tcs = new TaskCompletionSource<bool>();

                var animTimePercent = (cardsView.Width - Abs(view.TranslationX)) / cardsView.Width;
                var animLength = (uint)(AnimationLength * animTimePercent);
                new Animation(v => view.TranslationX = v, view.TranslationX, -Sign((int)panItemPosition) * cardsView.Width)
                    .Commit(view, nameof(HandlePanReset), 16, animLength, AnimEasing, (v, t) => tcs.SetResult(true));
                return tcs.Task;
            }
            return Task.FromResult(true);
        }
    }
}
