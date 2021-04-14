using Foundation;
using PanCardView;
using PanCardView.iOS;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using PanCardView.Enums;
using System.ComponentModel;
using static System.Math;

[assembly: ExportRenderer(typeof(CardsView), typeof(CardsViewRenderer))]
namespace PanCardView.iOS
{
    [Preserve(AllMembers = true)]
    public class CardsViewRenderer : VisualElementRenderer<CardsView>
    {
        private UISwipeGestureRecognizer _leftSwipeGesture;
        private UISwipeGestureRecognizer _rightSwipeGesture;
        private UISwipeGestureRecognizer _upSwipeGesture;
        private UISwipeGestureRecognizer _downSwipeGesture;

        public static void Preserve()
            => Preserver.Preserve();

        public CardsViewRenderer()
        {
            _leftSwipeGesture = new UISwipeGestureRecognizer(OnSwiped)
            {
                Direction = UISwipeGestureRecognizerDirection.Left
            };
            _rightSwipeGesture = new UISwipeGestureRecognizer(OnSwiped)
            {
                Direction = UISwipeGestureRecognizerDirection.Right
            };
            _upSwipeGesture = new UISwipeGestureRecognizer(OnSwiped)
            {
                Direction = UISwipeGestureRecognizerDirection.Up
            };
            _downSwipeGesture = new UISwipeGestureRecognizer(OnSwiped)
            {
                Direction = UISwipeGestureRecognizerDirection.Down
            };
        }

        public override void AddGestureRecognizer(UIGestureRecognizer gestureRecognizer)
        {
            base.AddGestureRecognizer(gestureRecognizer);

            if (gestureRecognizer is UIPanGestureRecognizer panGestureRecognizer)
            {
                gestureRecognizer.ShouldBeRequiredToFailBy = ShouldBeRequiredToFailBy;
                gestureRecognizer.ShouldRecognizeSimultaneously = ShouldRecognizeSimultaneously;
            }
        }

        protected override void OnElementChanged(ElementChangedEventArgs<CardsView> e)
        {
            base.OnElementChanged(e);
            if (e.NewElement != null)
            {
                SetSwipeGestures();
            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
            if (e.PropertyName == CardsView.IsVerticalSwipeEnabledProperty.PropertyName)
            {
                SetSwipeGestures();
                return;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _leftSwipeGesture?.Dispose();
                _rightSwipeGesture?.Dispose();
                _upSwipeGesture?.Dispose();
                _downSwipeGesture?.Dispose();
                _leftSwipeGesture = null;
                _rightSwipeGesture = null;
                _upSwipeGesture = null;
                _downSwipeGesture = null;
            }
            base.Dispose(disposing);
        }

        protected virtual void ResetSwipeGestureRecognizer(UISwipeGestureRecognizer swipeGestureRecognizer, bool isForceRemove = false)
        {
            RemoveGestureRecognizer(swipeGestureRecognizer);
            if (isForceRemove)
            {
                return;
            }
            AddGestureRecognizer(swipeGestureRecognizer);
        }

        protected void SetSwipeGestures()
        {
            ResetSwipeGestureRecognizer(_leftSwipeGesture);
            ResetSwipeGestureRecognizer(_rightSwipeGesture);

            var shouldRemoveVerticalSwipes = !(Element?.IsVerticalSwipeEnabled ?? false);
            ResetSwipeGestureRecognizer(_upSwipeGesture, shouldRemoveVerticalSwipes);
            ResetSwipeGestureRecognizer(_downSwipeGesture, shouldRemoveVerticalSwipes);
        }

        private void OnSwiped(UISwipeGestureRecognizer gesture)
        {
            var swipeDirection = gesture.Direction == UISwipeGestureRecognizerDirection.Left
                ? ItemSwipeDirection.Left
                : gesture.Direction == UISwipeGestureRecognizerDirection.Right
                    ? ItemSwipeDirection.Right
                    : gesture.Direction == UISwipeGestureRecognizerDirection.Up
                        ? ItemSwipeDirection.Up
                        : ItemSwipeDirection.Down;

            Element?.OnSwiped(swipeDirection);
        }

        private bool ShouldBeRequiredToFailBy(UIGestureRecognizer gestureRecognizer, UIGestureRecognizer otherGestureRecognizer)
            => IsPanGestureHandled() && otherGestureRecognizer.View != this;

        private bool ShouldRecognizeSimultaneously(UIGestureRecognizer gestureRecognizer, UIGestureRecognizer otherGestureRecognizer)
        {
            if (!(gestureRecognizer is UIPanGestureRecognizer panGesture))
            {
                return true;
            }

            var parent = Element?.Parent;
            while (parent != null)
            {
                if (parent is FlyoutPage && (Element?.IsHorizontalOrientation ?? false))
                {
                    var velocity = panGesture.VelocityInView(this);
                    return Abs(velocity.Y) > Abs(velocity.X);
                }
                parent = parent.Parent;
            }
            return !IsPanGestureHandled();
        }

        private bool IsPanGestureHandled()
            => Abs(Element?.CurrentDiff ?? 0) >= Element?.MoveThresholdDistance;
    }
}