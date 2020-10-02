﻿using Android.Content;
using Android.Runtime;
using Android.Views;
using PanCardView;
using PanCardView.Droid;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using static System.Math;
using PanCardView.Enums;

[assembly: ExportRenderer(typeof(CardsView), typeof(CardsViewRenderer))]
namespace PanCardView.Droid
{
    [Preserve(AllMembers = true)]
    public class CardsViewRenderer : VisualElementRenderer<CardsView>
    {
        public static int SwipeThreshold { get; set; } = 100;
        public static int SwipeVelocityThreshold { get; set; } = 1200;

        private bool _panStarted;
        private float? _startX;
        private float? _startY;
        private GestureDetector _gestureDetector;

        public CardsViewRenderer(Context context) : base(context)
        {
        }

        public static void Preserve()
        => Preserver.Preserve();

        public override bool OnInterceptTouchEvent(MotionEvent ev)
        {
            DetectEvent(ev);

            if (Element.IsPanControlledByChild || !Element.IsPanInteractionEnabled || Element.ShouldThrottlePanInteraction)
            {
                base.OnInterceptTouchEvent(ev);
                return false;
            }

            if (ev.ActionMasked == MotionEventActions.Move)
            {
                return SetIsTouchHandled(GetTotalX(ev), GetTotalY(ev));
            }

            HandleDownUpEvents(ev);
            return false;
        }

        public override bool OnTouchEvent(MotionEvent e)
        {
            DetectEvent(e);
            if (e.ActionMasked == MotionEventActions.Move)
            {
                var xDelta = GetTotalX(e);
                var yDelta = GetTotalY(e);
                SetIsTouchHandled(xDelta, yDelta);

                if ((Abs(yDelta) <= Abs(xDelta) && Element.IsHorizontalOrientation) ||
                    (Abs(yDelta) >= Abs(xDelta) && !Element.IsHorizontalOrientation))
                {
                    UpdatePan(GestureStatus.Running, xDelta, yDelta);
                }
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
            }
        }

        private void DetectEvent(MotionEvent ev)
        {
            if (ev.PointerCount > 1)
            {
                return;
            }
            try
            {
                if (_gestureDetector == null)
                {
                    SetGestureDetector();
                }
                _gestureDetector.OnTouchEvent(ev);
            }
            catch (ObjectDisposedException)
            {
                SetGestureDetector();
                DetectEvent(ev);
            }
        }

        private bool SetIsTouchHandled(float xDelta, float yDelta)
        {
            var xDeltaAbs = Abs(xDelta);
            var yDeltaAbs = Abs(yDelta);
            var isHandled = Element.IsHorizontalOrientation
                ? (yDeltaAbs > xDeltaAbs &&
                             Element.IsVerticalSwipeEnabled &&
                             yDeltaAbs > Element.VerticalSwipeThresholdDistance) ||
                            (xDeltaAbs > yDeltaAbs &&
                             xDeltaAbs > Element.MoveThresholdDistance)
                : (xDeltaAbs > yDeltaAbs &&
                             Element.IsPanSwipeEnabled &&
                             xDeltaAbs > Element.SwipeThresholdDistance) ||
                            (yDeltaAbs > xDeltaAbs &&
                             yDeltaAbs > Element.MoveThresholdDistance);

            Element.IsUserInteractionRunning |= isHandled;
            Parent?.RequestDisallowInterceptTouchEvent(isHandled);
            return isHandled;
        }

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

            var xDelta = GetTotalX(ev);
            var yDelta = GetTotalY(ev);
            UpdatePan(isUpAction ? GestureStatus.Completed : GestureStatus.Canceled, xDelta, yDelta);
            _panStarted = false;

            Parent?.RequestDisallowInterceptTouchEvent(false);

            _startX = null;
            _startY = null;
        }

        private void HandleDownEvent(MotionEvent ev)
        {
            if (ev.ActionMasked != MotionEventActions.Down)
            {
                return;
            }
            _startX = ev.GetX();
            _startY = ev.GetY();

            UpdatePan(GestureStatus.Started);
            _panStarted = true;
        }

        private void UpdatePan(GestureStatus status, double totalX = 0, double totalY = 0)
        => Element.OnPanUpdated(GetPanUpdatedEventArgs(status, totalX, totalY));

        private void OnSwiped(ItemSwipeDirection swipeDirection) => Element.OnSwiped(swipeDirection);

        private PanUpdatedEventArgs GetPanUpdatedEventArgs(GestureStatus status, double totalX = 0, double totalY = 0)
        => new PanUpdatedEventArgs(status, 0, totalX, totalY);

        private float GetTotalX(MotionEvent ev) => (ev.GetX() - _startX.GetValueOrDefault()) / Context.Resources.DisplayMetrics.Density;

        private float GetTotalY(MotionEvent ev) => (ev.GetY() - _startY.GetValueOrDefault()) / Context.Resources.DisplayMetrics.Density;

        private void SetGestureDetector()
        => _gestureDetector = new GestureDetector(Context, new CardsGestureListener(OnSwiped));
    }

    public class CardsGestureListener : GestureDetector.SimpleOnGestureListener
    {
        private readonly Action<ItemSwipeDirection> _onSwiped;

        public CardsGestureListener(Action<ItemSwipeDirection> onSwiped)
        => _onSwiped = onSwiped;

        public override bool OnFling(MotionEvent e1, MotionEvent e2, float velocityX, float velocityY)
        {
            var diffX = (e2?.GetX() ?? 0) - (e1?.GetX() ?? 0);
            var diffY = (e2?.GetY() ?? 0) - (e1?.GetY() ?? 0);

            var absDiffX = Abs(diffX);
            var absDiffY = Abs(diffY);

            if (absDiffX > absDiffY &&
                absDiffX > CardsViewRenderer.SwipeThreshold &&
                Abs(velocityX) > CardsViewRenderer.SwipeVelocityThreshold)
            {
                _onSwiped?.Invoke(diffX < 0 ? ItemSwipeDirection.Left : ItemSwipeDirection.Right);
                return true;
            }

            if (absDiffY >= absDiffX &&
               absDiffY > CardsViewRenderer.SwipeThreshold &&
               Abs(velocityY) > CardsViewRenderer.SwipeVelocityThreshold)
            {
                _onSwiped?.Invoke(diffY < 0 ? ItemSwipeDirection.Up : ItemSwipeDirection.Down);
                return true;
            }

            return false;
        }
    }
}
