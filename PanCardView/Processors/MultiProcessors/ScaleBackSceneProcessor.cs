using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using iQ.Mobile.UI.Common.Extensions;
using PanCardView;
using PanCardView.Enums;
using Xamarin.Forms;
using static System.Math;

namespace PanCardView.Processors.MultiProcessors
{
    public class ScaleBackSceneProcessor : BaseScaleProcessor
    {
        public override Task HandleAutoNavigate(IEnumerable<View> views, CardsView cardsView, AnimationDirection animationDirection, IEnumerable<View> inactiveViews)
        {
            var view = views.FirstOrDefault();

            if (view == null)
            {
                return Task.FromResult(false);
            }

            var tcs = new TaskCompletionSource<bool>();

            var sourceScale = InitialFrontScale;
            var destinationScale = InitialBackScale;

            if (animationDirection != AnimationDirection.Prev)
            {
                sourceScale = InitialBackScale;
                destinationScale = InitialFrontScale;
            }

            new Animation(v => view.Scale = v, 0, destinationScale)
                .Commit(view, $"{GetType().Name}_{nameof(HandleAutoNavigate)}", 16, AnimationLength, AnimEasing, (v, t) =>
                {
                    tcs.SetResult(true);
                });

            return tcs.Task;
        }

        public override void HandleInitView(IEnumerable<View> views, CardsView cardsView, AnimationDirection animationDirection)
        {
            views = SetupDisappearingContexts(views);
            views = SetupViews(views, InitialBackScale);
		}

		public override Task HandlePanApply(IEnumerable<View> views, CardsView cardsView, AnimationDirection animationDirection, IEnumerable<View> inactiveViews)
        {
            var view = views.FirstOrDefault();

            if (view != null)
            {
                var alignedScale = ConvertRange(InitialBackScale, InitialFrontScale, 0, 1, view.Scale);

                var scaleAnimTimePercent = (1 - alignedScale) / 1;
                var scaleAnimLength = (uint)(AnimationLength * scaleAnimTimePercent);
                new Animation(v => view.Scale = v, view.Scale, InitialBackScale)
                    .Commit(view, $"{GetType().Name}_{nameof(HandlePanApply)}", 16, scaleAnimLength, AnimEasing);
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
                    scaleModifier = (xPosAbs / ScaleTuneVal);
                }
                else if (animationDirection == AnimationDirection.Prev)
                {
                    scaleModifier = (xPos / ScaleTuneVal);
                }
                else
                {
                    scaleModifier = InitialBackScale;
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
                var alignedScale = ConvertRange(InitialBackScale, InitialFrontScale, 0, 1, view.Scale);

                var scaleAnimTimePercent = (1 - alignedScale) / 1;
                var scaleAnimLength = (uint)(AnimationLength * scaleAnimTimePercent);
                new Animation(v => view.Scale = v, view.Scale, InitialBackScale)
                    .Commit(view, $"{GetType().Name}_{nameof(HandlePanReset)}", 16, scaleAnimLength, AnimEasing);
            }

            return Task.FromResult(true);
        }
    }
}