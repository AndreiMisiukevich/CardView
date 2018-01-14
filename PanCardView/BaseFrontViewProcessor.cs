using System;
using Xamarin.Forms;
using System.Threading.Tasks;

namespace PanCardView
{
    public class BaseFrontViewProcessor : ICardProcessor
    {
        private const double Rad = 57.2957795;

        protected uint AnimationLength { get; } = 200;

        public virtual void InitView(View view)
        => ResetInitialState(view);

        public virtual void HandlePanChanged(View view, double xPos)
        {
            var parent = view.Parent as CardsView;
            view.TranslationX = xPos;
            view.TranslationY = Math.Abs(xPos) / 10;
            view.Rotation = 0.3 * Math.Min(xPos / parent.Width, 1) * Rad;
        }

        public virtual Task HandlePanReset(View view)
        {
            var parent = view.Parent as CardsView;
            var animLength = (uint)(AnimationLength * Math.Min(Math.Abs(view.TranslationX / parent.MoveDistance), 1.0));
            var tcs = new TaskCompletionSource<bool>();
            new Animation {
                { 0, 1, new Animation (v => view.TranslationX = v, view.TranslationX, 0) },
                { 0, 1, new Animation (v => view.TranslationY = v, view.TranslationY, 0) },
                { 0, 1, new Animation (v => view.Rotation = v, view.Rotation, 0) }
            }.Commit(view, nameof(HandlePanReset), 16, animLength, null, (v, c) => {
                ResetInitialState(view);
                tcs.SetResult(true);
            });
            return tcs.Task;
        }

        public virtual Task HandlePanApply(View view)
        {
            var tcs = new TaskCompletionSource<bool>();
            Device.BeginInvokeOnMainThread(async () =>
            {
                await view.FadeTo(0, AnimationLength);
                view.IsVisible = false;
                tcs.SetResult(true);
            });
            return tcs.Task;
        }

        protected virtual void ResetInitialState(View view, bool isVisible = true)
        {
            if (view != null)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    view.Scale = 1;
                    view.Opacity = 1;
                    view.TranslationX = 0;
                    view.Rotation = 0;
                    view.TranslationY = 0;
                    view.IsVisible = isVisible;
                });
            }
        }
    }
}
