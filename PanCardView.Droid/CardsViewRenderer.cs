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
		private static readonly Random _randomGenerator = new Random();
		private static Guid? _lastTouchHandlerId;

		public static bool IsTouchHandled { get; private set; }

		private int _gestureId;
		private Guid _elementId;
		private bool _panStarted;
		private float _startX;
		private float _startY;
		private bool? _isSwiped;
		private readonly GestureDetector _gestureDetector;

		public CardsViewRenderer(Context context) : base(context)
		=> _gestureDetector = new GestureDetector(new CardsGestureListener(OnSwiped));

		public override bool OnInterceptTouchEvent(MotionEvent ev)
		{
			if (ev.ActionMasked == MotionEventActions.Move)
			{
				if (_lastTouchHandlerId.HasValue && _lastTouchHandlerId != _elementId)
				{
					return false;
				}

				return SetIsTouchHandled(GetTotalX(ev), GetTotalY(ev));
			}

			HandleDownUpEvents(ev);
			return false;
		}

		public override bool OnTouchEvent(MotionEvent e)
		{
			_gestureDetector.OnTouchEvent(e);

			if (e.ActionMasked == MotionEventActions.Move)
			{
				var density = Context.Resources.DisplayMetrics.Density;

				var xDelta = GetTotalX(e);
				var yDelta = GetTotalY(e);

				var distXDp = xDelta / density;
				var distYDp = yDelta / density;

				SetIsTouchHandled(xDelta, yDelta);

				UpdatePan(GestureStatus.Running, distXDp, distYDp);
			}

			HandleDownUpEvents(e);
			return true;
		}

		protected override void OnElementChanged(ElementChangedEventArgs<CardsView> e)
		{
			base.OnElementChanged(e);
			if (e.NewElement != null)
			{
				_panStarted = false;
				_elementId = Guid.NewGuid();
			}
		}

		private bool SetIsTouchHandled(float xDelta, float yDelta)
		=> IsTouchHandled = Abs(xDelta) > Abs(yDelta);

		private void HandleDownUpEvents(MotionEvent ev)
		{
			HandleDownEvent(ev);
			HandleUpCancelEvent(ev);
		}

		private void HandleUpCancelEvent(MotionEvent ev)
		{
			var action = ev.ActionMasked;
			var isUpAction = action == MotionEventActions.Up;
			var isCancelAction = action == MotionEventActions.Cancel;
			if (!_panStarted || (!isUpAction && !isCancelAction))
			{
				return;
			}

			UpdatePan(isUpAction ? GestureStatus.Completed : GestureStatus.Canceled);
			_panStarted = false;
			_lastTouchHandlerId = null;

			IsTouchHandled = false;
		}

		private void HandleDownEvent(MotionEvent ev)
		{
			if (ev.ActionMasked != MotionEventActions.Down)
			{
				return;
			}
			_gestureId = _randomGenerator.Next();
			_startX = ev.GetX();
			_startY = ev.GetY();

			UpdatePan(GestureStatus.Started);
			_panStarted = true;
			_lastTouchHandlerId = _elementId;

			_isSwiped = null;
		}

		private void UpdatePan(GestureStatus status, double totalX = 0, double totalY = 0)
		{
			var args = GetPanUpdatedEventArgs(status, totalX, totalY);
			Element.OnPanUpdated(args, _isSwiped);
		}

		private void OnSwiped(bool isLeft)
		=> _isSwiped = isLeft;

		private PanUpdatedEventArgs GetPanUpdatedEventArgs(GestureStatus status, double totalX = 0, double totalY = 0)
		=> new PanUpdatedEventArgs(status, _gestureId, totalX, totalY);

		private float GetTotalX(MotionEvent ev) => ev.GetX() - _startX;

		private float GetTotalY(MotionEvent ev) => ev.GetY() - _startY;

	}

	public class CardsGestureListener : GestureDetector.SimpleOnGestureListener
	{
		private const int SwipeVelocity = 800;

		private readonly Action<bool> _onSwiped;

		public CardsGestureListener(Action<bool> onSwiped) 
		=> _onSwiped = onSwiped;

		public override bool OnFling(MotionEvent e1, MotionEvent e2, float velocityX, float velocityY)
		{
			if (Abs(velocityX) > SwipeVelocity)
			{
				_onSwiped.Invoke(velocityX < 0);
				return true;
			}
			return false;
		}
	}

	public class FRenderer : CardsViewRenderer
	{
		public FRenderer(Context context) : base(context)
		{
		}

		public override bool OnInterceptTouchEvent(MotionEvent ev)
		{
			return true;
		}

		public override bool OnTouchEvent(MotionEvent e)
		{
			return true;
		}
	}
}