using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Android.Content;
using PanCardView.Droid;
using PanCardView;
using Android.Views;
using Android.Runtime;
using static System.Math;

[assembly: ExportRenderer(typeof(CardsView), typeof(CardsViewRenderer))]
namespace PanCardView.Droid
{
    [Preserve(AllMembers = true)]
    public class CardsViewRenderer : VisualElementRenderer<CardsView>
    {
        private bool _panStarted;
		private float _startX;
		private float _startY;

        public CardsViewRenderer(Context context) : base(context)
        {
        }

		public override bool OnInterceptTouchEvent(MotionEvent ev)
		{
			var action = ev.Action;

			if (ev.Action == MotionEventActions.Move)
			{
				var xDist = Abs(GetTotalX(ev));
				var yDist = Abs(GetTotalY(ev));
				return xDist > yDist;
			}

			if (action == MotionEventActions.Down)
			{
				_startX = ev.GetX();
				_startY = ev.GetY();
				UpdatePan(true);
			}

			if (_panStarted && action == MotionEventActions.Up)
			{
				UpdatePan(false);
			}

			return false;
		}

		public override bool OnTouchEvent(MotionEvent e)
		{
			var action = e.Action;

			if (action == MotionEventActions.Move)
			{
				var density = Context.Resources.DisplayMetrics.Density;
				var distXDp = GetTotalX(e) / density;
				var distYDp = GetTotalY(e) / density;
				var args = new PanUpdatedEventArgs(GestureStatus.Running, -1, distXDp, distYDp);
				Element.OnPanUpdated(this, args);
			}

			if (_panStarted && action == MotionEventActions.Up)
			{
				UpdatePan(false);
			}

			return true;
		}

		protected override void OnElementChanged(ElementChangedEventArgs<CardsView> e)
		{
			base.OnElementChanged(e);
			_panStarted = false;
		}

		private void UpdatePan(bool isStarted)
        {
            _panStarted = isStarted;
            var args = new PanUpdatedEventArgs(isStarted ? GestureStatus.Started : GestureStatus.Completed, -1, 0, 0);
			Element.OnPanUpdated(this, args);
        }

		private float GetTotalX(MotionEvent ev) => ev.GetX() - _startX;

		private float GetTotalY(MotionEvent ev) => ev.GetY() - _startY;

    }
}
