using System.ComponentModel;
using Foundation;
using PanCardView;
using PanCardView.iOS;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(CardsView), typeof(CardsViewRenderer))]
namespace PanCardView.iOS
{
    [Preserve(AllMembers = true)]
    public class CardsViewRenderer : VisualElementRenderer<CardsView>
    {
        private readonly UISwipeGestureRecognizer _leftSwipeGesture;
        private readonly UISwipeGestureRecognizer _rightSwipeGesture;

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
        }

        protected override void OnElementChanged(ElementChangedEventArgs<CardsView> e)
        {
            base.OnElementChanged(e);
            if(e.NewElement != null)
            {
                SetSwipeGestures(Element.IsPanInteractionEnabled);
            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
            if(e.PropertyName == nameof(CardsView.IsPanInteractionEnabled))
            {
                SetSwipeGestures(Element.IsPanInteractionEnabled);
            }
        }

        private void SetSwipeGestures(bool isForceRemove = false)
        {
            RemoveGestureRecognizer(_leftSwipeGesture);
            RemoveGestureRecognizer(_rightSwipeGesture);
            if(isForceRemove)
            {
                return;
            }
            AddGestureRecognizer(_leftSwipeGesture);
            AddGestureRecognizer(_rightSwipeGesture);
        }

        private void OnSwiped(UISwipeGestureRecognizer gesture)
        => Element.OnSwiped(gesture.Direction == UISwipeGestureRecognizerDirection.Left);
    }
}