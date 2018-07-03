using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using iQ.Mobile.UI.Common.Extensions;
using iQ.Mobile.UI.Common.Interfaces;
using PanCardView;
using PanCardView.Enums;
using Xamarin.Forms;
using static System.Math;

namespace PanCardView.Processors.MultiProcessors
{
    public class ScaleFrontSceneProcessor : BaseScaleProcessor
    {
        public override Task HandleAutoNavigate(IEnumerable<View> views, CardsView cardsView, AnimationDirection animationDirection, IEnumerable<View> inactiveViews)
        {
            var view = views.FirstOrDefault();
            if (view == null)
            {
                return Task.FromResult(false);
            }

            var tcs = new TaskCompletionSource<bool>();
            view.IsVisible = true;
            new Animation(v => view.TranslationX = v, view.Scale, InitialFrontScale)
                .Commit(view, $"{GetType().Name}_{nameof(HandleAutoNavigate)}", 16, AnimationLength, AnimEasing, (d, b) => tcs.SetResult(true));
            return tcs.Task;
        }

        public override void HandleInitView(IEnumerable<View> views, CardsView cardsView, AnimationDirection animationDirection)
        {
            views = SetupViews(views, InitialFrontScale);
        }

        public override Task HandlePanApply(IEnumerable<View> views, CardsView cardsView, AnimationDirection animationDirection, IEnumerable<View> inactiveViews)
        {
            var view = views.FirstOrDefault();
            if (view != null)
            {
                var tcs = new TaskCompletionSource<bool>();

                var alignedScale = ConvertRange(InitialBackScale, InitialFrontScale, 0, 1, view.Scale);

                var scaleAnimTimePercent = (1 - alignedScale) / 1;
                var scaleAnimLength = (uint)(AnimationLength * scaleAnimTimePercent);

                new Animation(v =>
                {
                    view.Scale = v;
                }, view.Scale, InitialFrontScale)
                    .Commit(view, $"{GetType().Name}_{nameof(HandlePanApply)}", 16, scaleAnimLength, AnimEasing, (v, t) => tcs.SetResult(true));

                return tcs.Task;
            }

            return Task.FromResult(true);
        }

        public override void HandlePanChanged(IEnumerable<View> views, CardsView cardsView, double xPos, AnimationDirection animationDirection, IEnumerable<View> inactiveViews)
        {
            var viewArr = views.ToArray();

            for (var i = 0; i < viewArr.Length; i++)
            {
                var view = viewArr[i];

                if (view == null)
                    continue;

                double scaleModifier = 0;

                var xPosAbs = Abs(xPos);

                if (animationDirection == AnimationDirection.Next)
                {
                    scaleModifier = 1 - (xPosAbs / ScaleTuneVal);
                }
                else if (animationDirection == AnimationDirection.Prev)
                {
                    scaleModifier = 1 - (xPos / ScaleTuneVal);
                }
                else
                {
                    scaleModifier = InitialFrontScale;
                }

                scaleModifier = ConvertRange(0, 1,
                                             InitialBackScale, InitialFrontScale,
                                             scaleModifier);

                views = SetupViews(views, scaleModifier.Clamp(InitialBackScale, InitialFrontScale));
            }
        }

        public override Task HandlePanReset(IEnumerable<View> views, CardsView cardsView, AnimationDirection animationDirection, IEnumerable<View> inactiveViews)
        {
            var view = views.FirstOrDefault();
            if (view != null)
            {
                var tcs = new TaskCompletionSource<bool>();

                var alignedScale = ConvertRange(InitialBackScale, InitialFrontScale, 0, 1, view.Scale);

                var scaleAnimTimePercent = (1 - alignedScale) / 1;
                var scaleAnimLength = (uint)(AnimationLength * scaleAnimTimePercent);

                new Animation(v =>
                {
                    view.Scale = v;
                }, view.Scale, InitialFrontScale)
                    .Commit(view, $"{GetType().Name}_{nameof(HandlePanReset)}", 16, scaleAnimLength, AnimEasing, (v, t) => tcs.SetResult(true));

                return tcs.Task;
            }

            return Task.FromResult(true);
        }
	}
}
