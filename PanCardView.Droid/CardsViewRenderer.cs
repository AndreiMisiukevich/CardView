using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Android.Content;
using PanCardView.Droid;
using PanCardView;
using Android.Views;
using Android.Runtime;

[assembly: ExportRenderer(typeof(CardsView), typeof(CardsViewRenderer))]
namespace PanCardView.Droid
{
    [Preserve(AllMembers = true)]
    public class CardsViewRenderer : VisualElementRenderer<CardsView>
    {
        private bool _panStarted;

        public CardsViewRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<CardsView> e)
        {
            base.OnElementChanged(e);
            _panStarted = false;
        }

        public override bool OnTouchEvent(MotionEvent e)
        {
            var action = e.Action;

            if(_panStarted && action == MotionEventActions.Up)
            {
                UpdatePan(false);
            }
            else if(action == MotionEventActions.Down)
            {
                UpdatePan(true);
            }

            return base.OnTouchEvent(e);
        }

        private void UpdatePan(bool isStarted)
        {
            _panStarted = isStarted;
            var args = new PanUpdatedEventArgs(isStarted ? GestureStatus.Started : GestureStatus.Completed, -1, 0, 0);
            (Element as CardsView).OnPanUpdated(this, args);
        }
    }
}
