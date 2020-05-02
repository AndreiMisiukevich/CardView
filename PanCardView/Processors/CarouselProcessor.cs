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
    public class CarouselProcessor : IProcessor
    {
        public uint AnimationLength { get; set; } = 300;

        public Easing AnimationEasing { get; set; } = Easing.SinInOut;

        public double NoViewMaxShiftValue { get; set; } = 25;

        public double ScaleFactor { get; set; } = 1;

        public double OpacityFactor { get; set; } = 1;

        public double RotationFactor { get; set; } = 0;

        public double RotationXFactor { get; set; } = 0;

        public double RotationYFactor { get; set; } = 0;

        private readonly object _viewSizeChangedMapLocker = new object();

        private readonly Dictionary<View, ViewSizeInfoItem> _viewSizeChangedMap = new Dictionary<View, ViewSizeInfoItem>();

        public virtual void Init(CardsView cardsView, params ProcessorItem[] items)
        {
            foreach (var item in items)
            {
                if (item.IsFront)
                {
                    SetTranslationX(item.Views.FirstOrDefault(), 0, cardsView, true, true);
                    continue;
                }
                SetTranslationX(item.Views.FirstOrDefault(), Sign((int)item.Direction) * cardsView.GetSize(), cardsView, false, false);
            }
        }

        public virtual void Clean(CardsView cardsView, params ProcessorItem[] items)
        {
            foreach (var view in items.SelectMany(x => x.Views) ?? Enumerable.Empty<View>())
            {
                SetTranslationX(view, cardsView.GetSize(), cardsView, false, false, true);
            }
        }

        public virtual void Change(CardsView cardsView, double value, params ProcessorItem[] items)
        {
            foreach (var item in items)
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
                    if (Abs(value) > cardsView.GetSize() || (item.Direction == AnimationDirection.Prev && value < 0) || (item.Direction == AnimationDirection.Next && value > 0))
                    {
                        continue;
                    }

                    if (item.Direction == AnimationDirection.Null)
                    {
                        value = Sign(value) * Min(Abs(value / 4), NoViewMaxShiftValue);
                    }

                    SetTranslationX(view, value, cardsView, true);
                    continue;
                }

                if (item.Direction == AnimationDirection.Null)
                {
                    continue;
                }

                value = Sign((int)item.Direction) * cardsView.GetSize() + value;
                if (Abs(value) > cardsView.GetSize() || (item.Direction == AnimationDirection.Prev && value > 0) || (item.Direction == AnimationDirection.Next && value < 0))
                {
                    continue;
                }
                SetTranslationX(view, value, cardsView, false);
            }
        }

        public virtual Task Proceed(CardsView cardsView, params ProcessorItem[] items)
        {
            var animation = new AnimationWrapper();
            var animLength = AnimationLength;
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
                    animLength = Max((uint)(AnimationLength * animTimePercent), 1);
                    animation.Add(0, 1, new AnimationWrapper(v => SetTranslationX(view, v, cardsView, true), GetTranslationX(view, cardsView), 0));
                    continue;
                }

                animation.Add(0, 1, new AnimationWrapper(v => SetTranslationX(view, v, cardsView, false), GetTranslationX(view, cardsView), -Sign((int)item.Direction) * cardsView.GetSize()));
            }

            return animation.Commit(cardsView, Path.GetRandomFileName(), 16, animLength, AnimationEasing);
        }

        public virtual Task Reset(CardsView cardsView, params ProcessorItem[] items)
        {
            var animation = new AnimationWrapper();
            var animLength = AnimationLength;
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
                    animLength = Max((uint)(AnimationLength * animTimePercent) * 3 / 2, 1);
                    animation.Add(0, 1, new AnimationWrapper(v => SetTranslationX(view, v, cardsView, true), GetTranslationX(view, cardsView), 0));
                    continue;
                }

                if (view == cardsView.CurrentView)
                {
                    continue;
                }
                animation.Add(0, 1, new AnimationWrapper(v => SetTranslationX(view, v, cardsView, false), GetTranslationX(view, cardsView), Sign((int)item.Direction) * cardsView.GetSize()));
            }

            return animation.Commit(cardsView, Path.GetRandomFileName(), 16, animLength, AnimationEasing);
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
                    animation.Add(0, 1, new AnimationWrapper(v => SetTranslationX(view, v, cardsView, true), GetTranslationX(view, cardsView), 0));
                    continue;
                }

                var destinationPos = item.Direction == AnimationDirection.Prev
                    ? cardsView.GetSize()
                    : -cardsView.GetSize();

                animation.Add(0, 1, new AnimationWrapper(v => SetTranslationX(view, v, cardsView, false), 0, destinationPos));
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

        protected virtual void SetTranslationX(View view, double value, CardsView cardsView, bool isFront, bool? isVisible = null, bool isClean = false)
        {
            if (view == null || !CheckSize(view, cardsView, value, isVisible, isFront, isClean))
            {
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
                if (isFront)
                {
                    cardsView.ProcessorDiff = value;
                }
            }
            finally
            {
                view.BatchCommit();
            }
        }

        protected virtual double CalculateFactoredProperty(double value, double factor, CardsView cardsView, double defaultFactorValue = 1)
            => Abs(value) * (factor - defaultFactorValue) / cardsView.GetSize() + defaultFactorValue;

        protected bool CheckSize(View view, CardsView cardsView, double value, bool? isVisible, bool isFront, bool isClean)
        {
            lock (_viewSizeChangedMapLocker)
            {
                CleanViewSizeChanged(view);
                if (cardsView.GetSize(view) < 0 && !isClean)
                {
                    _viewSizeChangedMap[view] = new ViewSizeInfoItem
                    {
                        CardsView = cardsView,
                        Value = value,
                        IsVisible = isVisible,
                        IsFront = isFront
                    };
                    view.SizeChanged += OnViewSizeChanged;
                    return false;
                }
            }
            return true;
        }

        private void OnViewSizeChanged(object sender, System.EventArgs e)
        {
            lock (_viewSizeChangedMapLocker)
            {
                var view = sender as View;
                var info = _viewSizeChangedMap[view];
                if (info.CardsView.GetSize(view) < 0)
                {
                    return;
                }
                CleanViewSizeChanged(view);
                SetTranslationX(view, info.Value, info.CardsView, info.IsFront, info.IsVisible);
            }
        }

        private void CleanViewSizeChanged(View view)
        {
            view.SizeChanged -= OnViewSizeChanged;
            _viewSizeChangedMap.Remove(view);
        }
    }
}
