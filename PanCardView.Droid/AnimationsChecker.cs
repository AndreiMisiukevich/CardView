using PanCardView.Utility;
using Android.Provider;
using Android.App;
using PanCardView.Droid;
using Android.Runtime;

[assembly: Xamarin.Forms.Dependency(typeof(AnimationsChecker))]
namespace PanCardView.Droid
{
    [Preserve(AllMembers = true)]
    public class AnimationsChecker : IAnimationsChecker
    {
        public bool AreAnimationsEnabled
        {
            get
            {
                var resolver = Application.Context.ContentResolver;
                try
                {
                    var scale = Settings.Global.AnimatorDurationScale;
                    return Settings.Global.GetFloat(resolver, scale, 1) > 0;
                }
                catch
                {
                    try
                    {
#pragma warning disable
                        var scale = Settings.System.AnimatorDurationScale;
#pragma warning restore
                        return Settings.System.GetFloat(resolver, scale, 1) > 0;
                    }
                    catch
                    {
                        return true;
                    }
                }
            }
        }
    }
}
