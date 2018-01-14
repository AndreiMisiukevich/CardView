// 221(c) Andrei Misiukevich
using System;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Android.Content;
using PanCardView.Droid;
using PanCardView;
using Android.Views;

[assembly: ExportRenderer(typeof(CardsView), typeof(CardsViewRenderer))]
namespace PanCardView.Droid
{
    public class CardsViewRenderer : VisualElementRenderer<CardsView>
    {
        private bool _panStarted;

        public CardsViewRenderer(Context context) : base(context)
        {
        }

        public static void Init()
        {
            var now = DateTime.Now;
        }

        protected override void OnElementChanged(ElementChangedEventArgs<CardsView> e)
        {
            base.OnElementChanged(e);
            _panStarted = false;
        }

        public override bool OnTouchEvent(MotionEvent e)
        {
            var element = Element as CardsView;
            var action = e.Action;

            if(_panStarted && action == MotionEventActions.Up) // action == MotionEventActions.Move && e.PointerCount > 1
            {
                EndPan(element, e.ActionIndex);
            }
            else if(action == MotionEventActions.Down)
            {
                StartPan(element, e.ActionIndex);
            }

            return base.OnTouchEvent(e);
        }

        private void StartPan(CardsView element, int id)
        {
            _panStarted = true;
            element.OnPanUpdated(this, new PanUpdatedEventArgs(GestureStatus.Started, id, 0, 0));
        }

        private void EndPan(CardsView element, int id)
        {
            element.OnPanUpdated(this, new PanUpdatedEventArgs(GestureStatus.Completed, id, 0, 0));
            _panStarted = false;
        }
    }
}
