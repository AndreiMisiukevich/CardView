using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Android.Content;
using PanCardView.Controls;
using PanCardView.Droid;
using Android.Views;
using static PanCardView.Droid.CardsViewRenderer;
using Android.Runtime;

[assembly: ExportRenderer(typeof(ParentListView), typeof(ParentListViewRenderer))]
namespace PanCardView.Droid
{
	[Preserve(AllMembers = true)]
	public class ParentListViewRenderer : ListViewRenderer
	{
		public ParentListViewRenderer(Context context) : base(context)
		{
		}

		public override bool OnInterceptTouchEvent(MotionEvent ev)
		{
			return !IsTouchHandled &&
					base.OnInterceptTouchEvent(ev);
		}
	}
}
