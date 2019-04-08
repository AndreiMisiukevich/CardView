using System.Diagnostics;
using Foundation;
using PanCardView;
using PanCardView.iOS;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using PanCardView.Enums;
using CoreGraphics;

[assembly: ExportRenderer(typeof(CardsViewWrapper), typeof(CardsViewWrapperRenderer))]
namespace PanCardView.iOS
{
    [Preserve(AllMembers = true)]
    public class CardsViewWrapperRenderer : ViewRenderer
    {       
        private readonly UISwipeGestureRecognizer _leftSwipeGesture;
        private readonly UISwipeGestureRecognizer _rightSwipeGesture;
        private readonly UISwipeGestureRecognizer _upSwipeGesture;
        private readonly UISwipeGestureRecognizer _downSwipeGesture;

        public static void Preserve()
        => Preserver.Preserve();

        public CardsViewWrapperRenderer()
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

        protected override void OnElementChanged(ElementChangedEventArgs<View> e)
        {
            base.OnElementChanged(e);
            if (e.NewElement != null)
            {
                SetNativeControl(new CardsViewWrapperNativeView());

                SetSwipeGestures();
            }
        }

        protected virtual void ResetSwipeGestureRecognizer(UISwipeGestureRecognizer swipeGestureRecognizer)
        {
            RemoveGestureRecognizer(swipeGestureRecognizer);
            AddGestureRecognizer(swipeGestureRecognizer);
        }

        protected void SetSwipeGestures()
        {
            ResetSwipeGestureRecognizer(_leftSwipeGesture);
            ResetSwipeGestureRecognizer(_rightSwipeGesture);
            ResetSwipeGestureRecognizer(_upSwipeGesture);
            ResetSwipeGestureRecognizer(_downSwipeGesture);
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
            
            if (Element != null && Element is CardsViewWrapper)
            {
                var control = Element as CardsViewWrapper;
                control.Child?.OnSwiped(swipeDirection);
            }
        }
    }

    [Register("CardsViewWrapperNativeView")]
    public class CardsViewWrapperNativeView : UIView
    {
        public override bool PointInside(CGPoint point, UIEvent uievent)
        {
            return false;
        }
    }
}
