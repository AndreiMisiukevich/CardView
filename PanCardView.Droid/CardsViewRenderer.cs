using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Android.Content;
using PanCardView.Droid;
using PanCardView;
using Android.Views;
using Android.Runtime;
using static System.Math;
using System;

[assembly: ExportRenderer(typeof(CardsView), typeof(CardsViewRenderer))]
namespace PanCardView.Droid
{
	[Preserve(AllMembers = true)]
	public class CardsViewRenderer : VisualElementRenderer<CardsView>
	{
		private const int GestureId = -1;

		internal static int? LastDownEventHandlerHashCode { get; private set; }
		internal static bool IsMovementStarted { get; private set; }

		private bool _panStarted;
		private float _startX;
		private float _startY;
		private bool? _isSwiped;
		private readonly GestureDetector _detector;

		public CardsViewRenderer(Context context) : base(context)
		{
			var listener = new CardsGestureListener();
			_detector = new GestureDetector(listener);
			listener.Swiped += OnSwiped;
		}

		public override bool OnInterceptTouchEvent(MotionEvent ev)
		{
			if (ev.ActionMasked == MotionEventActions.Move)
			{
				if(LastDownEventHandlerHashCode.HasValue && LastDownEventHandlerHashCode != GetHashCode())
				{
					return false;
				}
				var xDist = Abs(GetTotalX(ev));
				var yDist = Abs(GetTotalY(ev));
				return IsMovementStarted = xDist > yDist;
			}

			HandleDownEvent(ev);
			HandleUpEvent(ev);
			return false;
		}

		public override bool OnTouchEvent(MotionEvent e)
		{
			_detector.OnTouchEvent(e);

			if (e.ActionMasked == MotionEventActions.Move)
			{
				var density = Context.Resources.DisplayMetrics.Density;
				var distXDp = GetTotalX(e) / density;
				var distYDp = GetTotalY(e) / density;
				var args = new PanUpdatedEventArgs(GestureStatus.Running, GestureId, distXDp, distYDp);
				Element.OnPanUpdated(args);
			}

			HandleDownEvent(e);
			HandleUpEvent(e);

			return true;
		}

		protected override void OnElementChanged(ElementChangedEventArgs<CardsView> e)
		{
			base.OnElementChanged(e);
			_panStarted = false;
		}

		private void HandleUpEvent(MotionEvent ev)
		{
			if(!_panStarted || ev.ActionMasked != MotionEventActions.Up)
			{
				return;
			}
			UpdatePan(false);
			LastDownEventHandlerHashCode = null;
			IsMovementStarted = false;
		}

		private void HandleDownEvent(MotionEvent ev)
		{
			if(ev.ActionMasked != MotionEventActions.Down)
			{
				return;
			}
			_startX = ev.GetX();
			_startY = ev.GetY();
			UpdatePan(true);
			LastDownEventHandlerHashCode = GetHashCode();
			_isSwiped = null;
		}

		private void UpdatePan(bool isStarted)
		{
			_panStarted = isStarted;
			var args = new PanUpdatedEventArgs(isStarted ? GestureStatus.Started : GestureStatus.Completed, GestureId, 0, 0);
			Element.OnPanUpdated(args, _isSwiped);
		}

		private void OnSwiped(bool isLeft)
		{
			_isSwiped = isLeft;
		}

		private float GetTotalX(MotionEvent ev) => ev.GetX() - _startX;

		private float GetTotalY(MotionEvent ev) => ev.GetY() - _startY;

	}

	public class CardsGestureListener : GestureDetector.SimpleOnGestureListener
	{
		public event Action<bool> Swiped;

		public override bool OnFling(MotionEvent e1, MotionEvent e2, float velocityX, float velocityY)
		{
			if (Abs(velocityX) > 800)
			{
				Swiped?.Invoke(velocityX < 0);
				return true;
			}
			return false;
		}
	}
}
