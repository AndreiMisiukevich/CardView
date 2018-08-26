using System;
using PanCardView.Utility;
using Android.Provider;
using Android.App;

namespace PanCardView.Droid
{
    public class AnimationChecker : IAnimationsChecker
    {
        public bool AnimationsAreDisabled
        {
            get => Settings.Global.GetFloat(Application.Context.ContentResolver, Settings.Global.AnimatorDurationScale, 1) <= 0;
        }
    }
}
