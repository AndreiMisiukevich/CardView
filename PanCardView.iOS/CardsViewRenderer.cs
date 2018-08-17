using Foundation;
using PanCardView;
using PanCardView.iOS;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using PanCardView.Enums;

[assembly: ExportRenderer(typeof(CardsView), typeof(CardsViewRenderer))]
namespace PanCardView.iOS
{
    [Preserve(AllMembers = true)]
    public class CardsViewRenderer : VisualElementRenderer<CardsView>
    {
        private readonly UISwipeGestureRecognizer _leftSwipeGesture;
        private readonly UISwipeGestureRecognizer _rightSwipeGesture;
        private readonly UISwipeGestureRecognizer _upSwipeGesture;
        private readonly UISwipeGestureRecognizer _downSwipeGesture;

        public static void Preserve()
        {
        }

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

        protected override void OnElementChanged(ElementChangedEventArgs<CardsView> e)
        {
            base.OnElementChanged(e);
            if(e.NewElement != null)
            {
                SetSwipeGestures();
            }
        }

        private void SetSwipeGestures()
        {
            RemoveGestureRecognizer(_leftSwipeGesture);
            RemoveGestureRecognizer(_rightSwipeGesture);
            RemoveGestureRecognizer(_upSwipeGesture);
            RemoveGestureRecognizer(_downSwipeGesture);

            AddGestureRecognizer(_leftSwipeGesture);
            AddGestureRecognizer(_rightSwipeGesture);
            AddGestureRecognizer(_upSwipeGesture);
            AddGestureRecognizer(_downSwipeGesture);
        }

        private void OnSwiped(UISwipeGestureRecognizer gesture)
        {
            var swipeDirection = gesture.Direction == UISwipeGestureRecognizerDirection.Left
                                ? SwipeDirection.Left
                                : gesture.Direction == UISwipeGestureRecognizerDirection.Right
                                    ? SwipeDirection.Right
                                    : gesture.Direction == UISwipeGestureRecognizerDirection.Up
                                        ? SwipeDirection.Up
                                        : SwipeDirection.Down;
            
            Element.OnSwiped(swipeDirection);
        }
    }
}