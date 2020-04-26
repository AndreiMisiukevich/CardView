using PanCardView.Enums;
using PanCardView.Extensions;
using PanCardView.Utility;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using static PanCardView.Processors.Constants;
using static System.Math;

namespace PanCardView.Processors
{
    public class CarouselProcessor: IProcessor
    {
        public uint AnimationLength { get; set; } = 300;

        public Easing AnimationEasing { get; set; } = Easing.SinInOut;

        public double NoViewMaxChangeValue { get; set; } = 25;

        public double ScaleFactor { get; set; } = 1;

        public double OpacityFactor { get; set; } = 1;

        public double RotationFactor { get; set; } = 0;

        public double RotationXFactor { get; set; } = 0;

        public double RotationYFactor { get; set; } = 0;

        public virtual void Init(CardsView cardsView, params ProcessorItem[] items)
        {
            foreach (var item in items)
            {
                if (item.IsFront)
                {
                    SetTranslationX(item.Views.FirstOrDefault(), 0, cardsView, true);
                    continue;
                }
                SetTranslationX(item.Views.FirstOrDefault(), Sign((int)item.Direction) * cardsView.GetSize(), cardsView, false);
            }
        }

        public virtual void Clean(CardsView cardsView, params ProcessorItem[] items)
        {
            foreach (var item in items)
            {
                SetTranslationX(item.Views.FirstOrDefault(), cardsView.GetSize(), cardsView, false, true);
            }
        }

        public virtual void Change(CardsView cardsView, double xPos, params ProcessorItem[] items)
        {
            foreach(var item in items)
            {
                var view = item.Views.FirstOrDefault();
                var inactiveView = item.InactiveViews.FirstOrDefault();

                if (view != null)
                {
                    view.IsVisible = true;
                }

                if (inactiveView != null && inactiveView != view)
                {
                    inactiveView.IsVisible = false;
                }

                if (item.IsFront)
                {
                    if (Abs(xPos) > cardsView.GetSize() || (item.Direction == AnimationDirection.Prev && xPos < 0) || (item.Direction == AnimationDirection.Next && xPos > 0))
                    {
                        continue;
                    }

                    if (item.Direction == AnimationDirection.Null)
                    {
                        xPos = Sign(xPos) * Min(Abs(xPos / 4), NoViewMaxChangeValue);
                    }

                    SetTranslationX(view, xPos, cardsView);
                    continue;
                }

                if (item.Direction == AnimationDirection.Null)
                {
                    continue;
                }

                var value = Sign((int)item.Direction) * cardsView.GetSize() + xPos;
                if (Abs(value) > cardsView.GetSize() || (item.Direction == AnimationDirection.Prev && value > 0) || (item.Direction == AnimationDirection.Next && value < 0))
                {
                    continue;
                }
                SetTranslationX(view, value, cardsView);
            }
        }

        public virtual Task Proceed(CardsView cardsView, params ProcessorItem[] items)
        {
            var animation = new AnimationWrapper();
            var lenght = AnimationLength;
            foreach (var item in items)
            {
                var view = item.Views.FirstOrDefault();
                if (view == null)
                {
                    continue;
                }

                if (item.IsFront)
                {
                    var animTimePercent = 1 - (cardsView.GetSize() - Abs(GetTranslationX(view, cardsView))) / cardsView.GetSize();
                    var animLength = (uint)(AnimationLength * animTimePercent);
                    if (animLength == 0)
                    {
                        return Task.FromResult(true);
                    }
                    lenght = animLength;
                    animation.Add(0, 1, new AnimationWrapper(v => SetTranslationX(view, v, cardsView), GetTranslationX(view, cardsView), 0));
                    continue;
                }

                animation.Add(0, 1, new AnimationWrapper(v => SetTranslationX(view, v, cardsView), GetTranslationX(view, cardsView), -Sign((int)item.Direction) * cardsView.GetSize()));
            }

            return animation.Commit(cardsView, Path.GetRandomFileName(), 16, lenght, AnimationEasing);
        }

        public virtual Task Reset(CardsView cardsView, params ProcessorItem[] items)
        {
            var animation = new AnimationWrapper();
            var lenght = AnimationLength;
            foreach (var item in items)
            {
                var view = item.Views.FirstOrDefault();
                if (view == null)
                {
                    continue;
                }

                if(item.IsFront)
                {
                    var animTimePercent = 1 - (cardsView.GetSize() - Abs(GetTranslationX(view, cardsView))) / cardsView.GetSize();
                    var animLength = (uint)(AnimationLength * animTimePercent) * 3 / 2;
                    if (animLength == 0)
                    {
                        return Task.FromResult(true);
                    }
                    lenght = animLength;
                    animation.Add(0, 1, new AnimationWrapper(v => SetTranslationX(view, v, cardsView), GetTranslationX(view, cardsView), 0));
                }

                if (view == null || view == cardsView.CurrentView)
                {
                    continue;
                }
                animation.Add(0, 1, new AnimationWrapper(v => SetTranslationX(view, v, cardsView), GetTranslationX(view, cardsView), Sign((int)item.Direction) * cardsView.GetSize()));
            }

            return animation.Commit(cardsView, Path.GetRandomFileName(), 16, lenght, AnimationEasing);
        }

        public virtual Task Navigate(CardsView cardsView, params ProcessorItem[] items)
        {
            var animation = new AnimationWrapper();
            var lenght = AnimationLength;
            foreach (var item in items)
            {
                var view = item.Views.FirstOrDefault();
                if (view == null)
                {
                    continue;
                }
                view.IsVisible = true;
                if (item.IsFront)
                {
                    animation.Add(0, 1, new AnimationWrapper(v => SetTranslationX(view, v, cardsView), GetTranslationX(view, cardsView), 0));
                }

                var destinationPos = item.Direction == AnimationDirection.Prev
                    ? cardsView.GetSize()
                    : -cardsView.GetSize();

                animation.Add(0, 1, new AnimationWrapper(v => SetTranslationX(view, v, cardsView), 0, destinationPos));
            }
            return animation.Commit(cardsView, Path.GetRandomFileName(), 16, AnimationLength, AnimationEasing);
        }

        protected virtual double GetTranslationX(View view, CardsView cardsView)
        {
            if (view == null)
            {
                return 0;
            }
            var value = cardsView.IsHorizontalOrientation ? view.TranslationX : view.TranslationY;
            value += Sign(value) * cardsView.GetSize(view) * 0.5 * (1 - view.Scale);
            return value;
        }

        protected virtual void SetTranslationX(View view, double value, CardsView cardsView, bool? isVisible = null, bool isClean = false)
        {
            if (view == null)
            {
                return;
            }

            void OnViewSizeChanged(object sender, System.EventArgs e)
            {
                if (cardsView.GetSize(view) < 0)
                {
                    return;
                }
                view.SizeChanged -= OnViewSizeChanged;
                SetTranslationX(view, value, cardsView, isVisible);
            }
            view.SizeChanged -= OnViewSizeChanged;

            if (cardsView.GetSize(view) < 0 && !isClean)
            {
                view.SizeChanged += OnViewSizeChanged;
                return;
            }

            try
            {
                view.BatchBegin();
                view.Scale = CalculateFactoredProperty(value, ScaleFactor, cardsView);
                view.Opacity = CalculateFactoredProperty(value, OpacityFactor, cardsView);
                view.Rotation = CalculateFactoredProperty(value, RotationFactor, cardsView, 0) * Angle360 * Sign(-value);
                view.RotationX = CalculateFactoredProperty(value, RotationXFactor, cardsView, 0) * Angle180 * Sign(-value);
                view.RotationY = CalculateFactoredProperty(value, RotationYFactor, cardsView, 0) * Angle180 * Sign(-value);
                var translation = value - Sign(value) * cardsView.GetSize(view) * 0.5 * (1 - view.Scale);
                if (cardsView.IsHorizontalOrientation)
                {
                    view.TranslationX = translation;
                }
                else
                {
                    view.TranslationY = translation;
                }
                view.IsVisible = isVisible ?? view.IsVisible;
            }
            finally
            {
                view.BatchCommit();
            }
        }

        protected virtual double CalculateFactoredProperty(double value, double factor, CardsView cardsView, double defaultFactorValue = 1)
            => Abs(value) * (factor - defaultFactorValue) / cardsView.GetSize() + defaultFactorValue;
    }
}
