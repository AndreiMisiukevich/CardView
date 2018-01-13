// 171(c) Andrei Misiukevich
using System;
using Xamarin.Forms;

namespace PanCardView
{
    public class BaseFrontViewProcessor : ICardProcessor
    {
        private const double Rad = 57.2957795;
        private const int AnimationLength = 150;

        public virtual void InitView(View view)
        {
            if (view != null)
            {
                view.Scale = 1;
                view.Opacity = 1;
                view.IsVisible = true;
            }
        }

        public virtual void HandlePanChanged(View view, double xPos)
        {
            var parent = view.Parent as View;
            view.TranslationX = xPos;
            view.Rotation = 0.3 * Math.Min(xPos / parent.Width, 1) * Rad;
        }

        public virtual void HandlePanReset(View view)
        {
            if (view != null)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    view.TranslationX = 0;
                    view.Rotation = 0;
                });
            }
        }

        public virtual async void HandlePanApply(View view)
        {
            await view.FadeTo(0, AnimationLength);
            view.IsVisible = false;
            view.Opacity = 1;
            HandlePanReset(view);
        }
    }
}
