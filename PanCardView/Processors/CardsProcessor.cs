using PanCardView.Enums;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using static PanCardView.Processors.Constants;
using static System.Math;
using PanCardView.Utility;
using PanCardView.Extensions;
using System;
using System.IO;

namespace PanCardView.Processors
{
    public class CardsProcessor : IProcessor
    {
        public uint AnimationLength { get; set; } = 185;

        public Easing AnimationEasing { get; set; } = Easing.SinInOut;

        public double NoViewMaxShiftValue { get; set; } = 25;

        public double BackViewInitialScale { get; set; } = 0.8;

        public void Init(CardsView cardsView, params ProcessorItem[] items)
        {
            foreach (var item in items)
            {
                var view = item.Views.FirstOrDefault();
                ResetInitialState(view, cardsView, item.IsFront);
            }
        }

        public void Clean(CardsView cardsView, params ProcessorItem[] items)
        {
            foreach (var view in items.SelectMany(x => x.Views) ?? Enumerable.Empty<View>())
            {
                view.IsVisible = false;
                view.TranslationX = cardsView.GetSize();
            }
        }

        public void Change(CardsView cardsView, double value, params ProcessorItem[] items)
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

                    var multiplier = 1;
                    if (item.Direction == AnimationDirection.Null)
                    {
                        value = Sign(value) * Min(Abs(value / 4), NoViewMaxShiftValue);
                        multiplier = -multiplier;
                    }

                    if (view != null)
                    {
                        SetTranslationX(view, value, cardsView, true);
                        view.TranslationY = multiplier * Abs(value) / 10;
                        view.Rotation = multiplier * 0.3 * Rad * (value / cardsView.GetSize());
                    }
                    continue;
                }

                if (item.Direction == AnimationDirection.Null)
                {
                    continue;
                }

                var calcScale = BackViewInitialScale + Abs((value / cardsView.RealMoveDistance) * (1 - BackViewInitialScale));
                view.Scale = Min(calcScale, 1);
            }
        }

        public async Task Navigate(CardsView cardsView, params ProcessorItem[] items)
        {
            var animations = new List<Task>();
            ProcessorItem? endItem = null;
            foreach (var item in items)
            {
                var view = item.Views.FirstOrDefault();
                if (view == null)
                {
                    continue;
                }

                if (item.IsFront)
                {

                    view.IsVisible = true;
                    animations.Add(new AnimationWrapper(v => view.Scale = v, view.Scale, 1).Commit(view, Path.GetRandomFileName(), 16, AnimationLength, AnimationEasing));
                    continue;
                }

                animations.Add(new AnimationWrapper(v => HandleAutoAnimatingPosChanged(view, cardsView, v, item.Direction), 0, cardsView.RealMoveDistance)
                    .Commit(view, Path.GetRandomFileName(), 16, AnimationLength, AnimationEasing));
                endItem = item;
            }
            await Task.WhenAll(animations);
            if (endItem.HasValue)
            {
                await Proceed(cardsView, endItem.GetValueOrDefault());
            }
        }

        public async Task Proceed(CardsView cardsView, params ProcessorItem[] items)
        {
            var animation = new AnimationWrapper();
            Action onFinish = null;
            foreach (var item in items)
            {
                var view = item.Views.FirstOrDefault();

                if (item.IsFront)
                {
                    if (view != null)
                    {
                        view.Scale = 1;
                    }
                    continue;
                }

                if (view != null)
                {
                    animation.Add(0, 1, new AnimationWrapper(v => view.Opacity = v, view.Opacity, 0));
                    onFinish = () => view.IsVisible = false;
                }
            }
            await animation.Commit(cardsView, Path.GetRandomFileName(), 16, AnimationLength, AnimationEasing);
            onFinish?.Invoke();
        }

        public async Task Reset(CardsView cardsView, params ProcessorItem[] items)
        {
            var animation = new AnimationWrapper();
            var animLength = AnimationLength;
            Action onFinish = null;
            foreach (var item in items)
            {
                var view = item.Views.FirstOrDefault();

                if (item.IsFront)
                {
                    onFinish = () => ResetInitialState(view, cardsView, true);
                    animLength = (uint)(AnimationLength * Min(Abs(view.TranslationX / cardsView.RealMoveDistance), 1.0));

                    if (animLength == 0)
                    {
                        continue;
                    }

                    animation.Add(0, 1, new AnimationWrapper {
                        { 0, 1, new AnimationWrapper (v => SetTranslationX(view, v, cardsView, true), view.TranslationX, 0) },
                        { 0, 1, new AnimationWrapper (v => view.TranslationY = v, view.TranslationY, 0) },
                        { 0, 1, new AnimationWrapper (v => view.Rotation = v, view.Rotation, 0) }
                    });
                    continue;
                }
                if (view == null || view == cardsView.CurrentView)
                {
                    continue;
                }
                animation.Add(0, 1, new AnimationWrapper(v => view.Scale = v, view.Scale, BackViewInitialScale));
            }
            await animation.Commit(cardsView, Path.GetRandomFileName(), 16, animLength, AnimationEasing);
            onFinish?.Invoke();
        }

        protected virtual void ResetInitialState(View view, CardsView cardsView, bool isFront)
        {
            if (view != null)
            {
                view.BatchBegin();
                view.Scale = isFront ? 1 : BackViewInitialScale;
                view.Opacity = 1;
                SetTranslationX(view, 0, cardsView, isFront, isFront);
                view.Rotation = 0;
                view.TranslationY = 0;
                view.BatchCommit();
            }
        }

        protected virtual void SetTranslationX(View view, double value, CardsView cardsView, bool isFront, bool? isVisible = null)
        {
            view.TranslationX = value;
            view.IsVisible = isVisible ?? view.IsVisible;
            if (isFront)
            {
                cardsView.ProcessorDiff = value;
            }
        }

        private void HandleAutoAnimatingPosChanged(View view, CardsView cardsView, double value, AnimationDirection animationDirection)
        {
            if (animationDirection == AnimationDirection.Next)
            {
                value = -value;
            }

            view.TranslationX = value;
            view.TranslationY = Abs(value) / 10;
            view.Rotation = 0.3 * Min(value / cardsView.GetSize(), 1) * Rad;
        }
    }
}
