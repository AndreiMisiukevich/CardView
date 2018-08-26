using PanCardView.Utility;
using Android.Provider;
using Android.App;
using PanCardView.Droid;

[assembly: Xamarin.Forms.Dependency(typeof(AnimationsChecker))]
namespace PanCardView.Droid
{
    public class AnimationsChecker : IAnimationsChecker
    {
        public bool AreAnimationsEnabled 
        {
            get
            {
                try
                {
                    var resolver = Application.Context.ContentResolver;
                    var scale = Settings.Global.AnimatorDurationScale;
                    return Settings.Global.GetFloat(resolver, scale, 1) > 0;
                }
                catch
                {
                    return true;
                }
            }
        }
    }
}
