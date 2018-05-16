using Android.Content;
using Android.Runtime;
using Android.Views;
using PanCardView.Controls;
using PanCardView.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using static PanCardView.Droid.CardsViewRenderer;

[assembly: ExportRenderer(typeof(ParentScrollView), typeof(ParentScrollViewRenderer))]
namespace PanCardView.Droid
{
    [Preserve(AllMembers = true)]
    public class ParentScrollViewRenderer : ScrollViewRenderer
    {
        public ParentScrollViewRenderer(Context context) : base(context)
        {
        }

        public override bool OnInterceptTouchEvent(MotionEvent ev)
        {
            return !IsTouchHandled &&
                    base.OnInterceptTouchEvent(ev);
        }
    }
}