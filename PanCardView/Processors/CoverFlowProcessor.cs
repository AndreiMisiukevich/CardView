using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using PanCardView.Enums;
using PanCardView.Extensions;
using PanCardView.Utility;
using Xamarin.Forms;
using static System.Math;

namespace PanCardView.Processors
{
    public class CoverFlowProcessor : CarouselProcessor
    {
        public CoverFlowProcessor()
        {
            NoViewMaxShiftValue = 0;
        }

        public override void Init(CardsView cardsView, params ProcessorItem[] items)
        {
            var step = GetStep(cardsView);
            foreach (var item in items)
            {
                if (item.IsFront)
                {
                    SetTranslationX(item.Views.FirstOrDefault(), 0, cardsView, true, true);
                    continue;
                }
                var index = 0;
                foreach (var view in item.Views ?? Enumerable.Empty<View>())
                {
                    ++index;
                    if (view == null)
                    {
                        continue;
                    }
                    SetTranslationX(view, Sign((int)item.Direction)
                        * (cardsView.IsRightToLeftFlowDirectionEnabled ? -1 : 1)
                        * step
                        * index, cardsView, false, true);
                }
            }
        }

        public override void Clean(CardsView cardsView, params ProcessorItem[] items)
        {
            foreach (var view in items.SelectMany(x => x.Views) ?? Enumerable.Empty<View>())
            {
                SetTranslationX(view, cardsView.GetSize(), cardsView, false, false, true);
            }
        }

        public override void Change(CardsView cardsView, double value, params ProcessorItem[] items)
        {
            var step = GetStep(cardsView);
            foreach (var item in items)
            {
                var view = item.Views.FirstOrDefault();

                if (Abs(value) > step || (item.Direction == AnimationDirection.Prev && value < 0) || (item.Direction == AnimationDirection.Next && value > 0))
                {
                    return;
                }

                if (item.IsFront)
                {
                    if (item.Direction == AnimationDirection.Null)
                    {
                        value = Sign(value) * Min(Abs(value / 4), NoViewMaxShiftValue);
                    }

                    if (view != null)
                    {
                        SetTranslationX(view, value, cardsView, true);
                    }
                    continue;
                }

                if (item.Direction == AnimationDirection.Null || view == null)
                {
                    Init(cardsView,
                        new ProcessorItem
                        {
                            Views = item.Views,
                            Direction = (AnimationDirection)Sign(GetTranslationX(view, cardsView)),
                        },
                        new ProcessorItem
                        {
                            Views = item.InactiveViews,
                            Direction = (AnimationDirection)Sign(GetTranslationX(item.InactiveViews?.FirstOrDefault(), cardsView))
                        });
                    continue;
                }

                var otherViews = item.Views.Union(item.InactiveViews ?? Enumerable.Empty<View>()).Except(Enumerable.Repeat(view, 1));
                ProceedPositionChanged(Sign((int)item.Direction) * step + value, view, otherViews, cardsView);
            }
        }

        public override Task Navigate(CardsView cardsView, params ProcessorItem[] items)
        {
            var step = GetStep(cardsView);
            var animation = new AnimationWrapper();
            foreach (var item in items)
            {
                var view = item.Views.FirstOrDefault();
                if (view == null)
                {
                    continue;
                }

                if (item.IsFront)
                {
                    animation.Add(0, 1, new AnimationWrapper(v => SetTranslationX(view, v, cardsView, true), GetTranslationX(view, cardsView), 0));
                    continue;
                }

                var otherViews = item.Views.Union(item.InactiveViews ?? Enumerable.Empty<View>()).Except(Enumerable.Repeat(view, 1));
                animation.Add(0, 1, new AnimationWrapper(v => ProceedPositionChanged(v, view, otherViews, cardsView), 0, -Sign((int)item.Direction) * step));

            }
            return animation.Commit(cardsView, Path.GetRandomFileName(), 16, AnimationLength, AnimationEasing);
        }

        public override Task Proceed(CardsView cardsView, params ProcessorItem[] items)
        {
            var step = GetStep(cardsView);
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
                    var animTimePercent = 1 - (step - Abs(GetTranslationX(view, cardsView))) / step;
                    animLength = Max((uint)(AnimationLength * animTimePercent), 1);
                    animation.Add(0, 1, new AnimationWrapper(v => SetTranslationX(view, v, cardsView, true), GetTranslationX(view, cardsView), 0));
                    continue;
                }

                var otherViews = item.Views.Union(item.InactiveViews ?? Enumerable.Empty<View>()).Except(Enumerable.Repeat(view, 1));
                animation.Add(0, 1, new AnimationWrapper(v => ProceedPositionChanged(v, view, otherViews, cardsView), GetTranslationX(view, cardsView), -Sign((int)item.Direction) * step));
            }
            return animation.Commit(cardsView, Path.GetRandomFileName(), 16, animLength, AnimationEasing);
        }

        public override Task Reset(CardsView cardsView, params ProcessorItem[] items)
        {
            var step = GetStep(cardsView);
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
                    var animTimePercent = 1 - (step - Abs(GetTranslationX(view, cardsView))) / step;
                    animLength = Max((uint)(AnimationLength * animTimePercent) * 3 / 2, 1);
                    animation.Add(0, 1, new AnimationWrapper(v => SetTranslationX(view, v, cardsView, true), GetTranslationX(view, cardsView), 0));
                    continue;
                }

                if (view == cardsView.CurrentView)
                {
                    continue;
                }
                var otherViews = item.Views.Union(item.InactiveViews ?? Enumerable.Empty<View>()).Except(Enumerable.Repeat(view, 1));
                animation.Add(0, 1, new AnimationWrapper(v => ProceedPositionChanged(v, view, otherViews, cardsView), GetTranslationX(view, cardsView), Sign((int)item.Direction) * step));
            }
            return animation.Commit(cardsView, Path.GetRandomFileName(), 16, animLength, AnimationEasing);
        }

        private double GetStep(CardsView cardsView)
        {
            var coverFlowView = cardsView.AsCoverFlowView();
            return cardsView.GetSize() * (1 - coverFlowView.PositionShiftPercentage) - coverFlowView.PositionShiftValue;
        }

        private void ProceedPositionChanged(double value, View checkView, IEnumerable<View> views, CardsView cardsView)
        {
            var diff = GetTranslationX(checkView, cardsView) - value;
            SetTranslationX(checkView, value, cardsView, false);

            foreach (var view in views ?? Enumerable.Empty<View>())
            {
                if (view == null)
                {
                    continue;
                }
                SetTranslationX(view, GetTranslationX(view, cardsView) - diff, cardsView, false);
            }
        }
    }
}
