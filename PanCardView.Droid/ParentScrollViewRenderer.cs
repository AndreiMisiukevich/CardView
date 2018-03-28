using System;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Android.Content;
using PanCardView.Controls;
using PanCardView.Droid;
using Android.Views;
using static PanCardView.Droid.CardsViewRenderer;
using Android.Runtime;

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
			return 
				//!IsMovementStarted 
				LastDownEventHandlerHashCode == null
			&& base.OnInterceptTouchEvent(ev);
		}
	}
}

