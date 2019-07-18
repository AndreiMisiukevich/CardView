using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Foundation;
using PanCardView.Enums;
using PanCardView.iOS;
using PanCardView.Utility;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: Xamarin.Forms.Dependency(typeof(CurlAnimator))]
namespace PanCardView.iOS
{
    [Preserve(AllMembers = true)]
    public class CurlAnimator : ICurlAnimator
    {
        private const double OneSecondMillisecond = 1000;

        public async Task Process(IEnumerable<View> views, CardsView cardsView, AnimationDirection animationDirection, IEnumerable<View> inactiveViews, uint duration)
        {
            UIView.BeginAnimations(nameof(CardsView));
            UIView.SetAnimationDuration(OneSecondMillisecond / duration);
            UIView.SetAnimationTransition(animationDirection == AnimationDirection.Next
                    ? UIViewAnimationTransition.CurlUp
                    : UIViewAnimationTransition.CurlDown, Platform.GetRenderer(cardsView).NativeView, true);
            UIView.CommitAnimations();
            await Task.Delay(TimeSpan.FromMilliseconds(duration));
        }
    }
}
