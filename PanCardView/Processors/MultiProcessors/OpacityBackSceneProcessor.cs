using System;
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
    public class OpacityBackSceneProcessor : BaseOpacityProcessor
    {
        public override Task HandleAutoNavigate(IEnumerable<View> views, CardsView cardsView, AnimationDirection animationDirection, IEnumerable<View> inactiveViews)
        {
            foreach (var view in views)
            {
                if (view != null)
                {
                    var alignedOpacity = ConvertRange(InitialBackOpacity, InitialFrontOpacity, 0, 1, view.Opacity);

                    var opacityAnimTimePercent = (1 - alignedOpacity) / 1;
                    var opacityAnimLength = (uint)(AnimationLength * opacityAnimTimePercent);
                    new Animation(v => view.Opacity = v, view.Opacity, InitialBackOpacity)
                        .Commit(view, $"{GetType().Name}_{nameof(HandleAutoNavigate)}", 16, opacityAnimLength, AnimEasing);
                }
            }

            return Task.FromResult(true);
        }

        public override void HandleInitView(IEnumerable<View> views, CardsView cardsView, AnimationDirection animationDirection)
        {
            views = SetupViews(views, InitialBackOpacity);
        }

        public override Task HandlePanApply(IEnumerable<View> views, CardsView cardsView, AnimationDirection animationDirection, IEnumerable<View> inactiveViews)
        {
            foreach (var view in views)
            {
                if (view != null)
                {
                    var alignedOpacity = ConvertRange(InitialBackOpacity, InitialFrontOpacity, 0, 1, view.Opacity);

                    var opacityAnimTimePercent = (1 - alignedOpacity) / 1;
                    var opacityAnimLength = (uint)(AnimationLength * opacityAnimTimePercent);
                    new Animation(v => view.Opacity = v, view.Opacity, InitialBackOpacity)
                        .Commit(view, $"{GetType().Name}_{nameof(HandlePanApply)}", 16, opacityAnimLength, AnimEasing);
                }
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

                double opacityModifier = 0;

                var xPosAbs = Abs(xPos);

                if (animationDirection == AnimationDirection.Next)
                {
                    opacityModifier = (xPosAbs / OpacityTuneVal);
                }
                else if (animationDirection == AnimationDirection.Prev)
                {
                    opacityModifier = (xPos / OpacityTuneVal);
                }
                else
                {
                    opacityModifier = InitialBackOpacity;
                }

                opacityModifier = ConvertRange(0, 1,
                                             InitialBackOpacity, InitialFrontOpacity,
                                             opacityModifier);

                views = SetupViews(views, opacityModifier.Clamp(InitialBackOpacity, InitialFrontOpacity));
            }
        }

        public override Task HandlePanReset(IEnumerable<View> views, CardsView cardsView, AnimationDirection animationDirection, IEnumerable<View> inactiveViews)
        {
            foreach (var view in views)
            {
                if (view != null)
                {
                    var alignedOpacity = ConvertRange(InitialBackOpacity, InitialFrontOpacity, 0, 1, view.Opacity);

                    var opacityAnimTimePercent = (1 - alignedOpacity) / 1;
                    var opacityAnimLength = (uint)(AnimationLength * opacityAnimTimePercent);

                    new Animation(v => view.Opacity = v, view.Opacity, InitialBackOpacity)
                        .Commit(view, $"{GetType().Name}_{nameof(HandlePanReset)}", 16, opacityAnimLength, AnimEasing);
                }
            }

            return Task.FromResult(true);
        }
    }
}
