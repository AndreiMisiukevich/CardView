using System;
using Xamarin.Forms;
using System.Threading.Tasks;
using PanCardView.Enums;

namespace PanCardView.Processors
{
    public class BaseFrontViewProcessor : ICardProcessor
    {
        private const double Rad = 57.2957795;

        protected uint ApplyAnimationLength { get; set; } = 200;

        protected uint ResetAnimationLength { get; set; } = 150;

        public virtual void InitView(View view, PanItemPosition panItemPosition)
        => ResetInitialState(view);

        public virtual void HandlePanChanged(View view, double xPos, PanItemPosition panItemPosition)
        {
            var parent = view?.Parent as CardsView;
            if(parent == null)
            {
                return;
            }
            view.TranslationX = xPos;
            view.TranslationY = Math.Abs(xPos) / 10;
            view.Rotation = 0.3 * Math.Min(xPos / parent.Width, 1) * Rad;
        }

        public virtual Task HandlePanReset(View view, PanItemPosition panItemPosition)
        {
            var parent = view.Parent as CardsView;
            var tcs = new TaskCompletionSource<bool>();

            if (!CheckIsInitialPosition(view))
            {
                var animLength = (uint)(ResetAnimationLength * Math.Min(Math.Abs(view.TranslationX / parent.MoveDistance), 1.0));
                new Animation {
                    { 0, 1, new Animation (v => view.TranslationX = v, view.TranslationX, 0) },
                    { 0, 1, new Animation (v => view.TranslationY = v, view.TranslationY, 0) },
                    { 0, 1, new Animation (v => view.Rotation = v, view.Rotation, 0) }
                }.Commit(view, nameof(HandlePanReset), 16, animLength, null, (v, c) => SetInitialResult(view, tcs));
            }
            else
            {
                SetInitialResult(view, tcs);
            }

            return tcs.Task;
        }

        public virtual async Task HandlePanApply(View view, PanItemPosition panItemPosition)
        {
            if (view != null)
            {
                await view.FadeTo(0, ApplyAnimationLength);
                view.IsVisible = false;
            }
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
