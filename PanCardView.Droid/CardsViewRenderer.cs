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
            var totalX = e.GetX();
            var totalY = e.GetY();
            var element = Element as CardsView;

            switch (e.Action)
            {
                case MotionEventActions.Down:
                    _panStarted = true;
                    element.OnPanUpdated(this, new PanUpdatedEventArgs(GestureStatus.Started, e.ActionIndex, totalX, totalY));
                    return true;

                case MotionEventActions.Move:
                    if (e.PointerCount > 1)
                    {
                        var x1 = e.GetX(1);
                        var y1 = e.GetY(1);

                        if (_panStarted)
                        {
                            EndPan(element, totalX, totalY);
                        }
                    }
                    else if (_panStarted)
                    {
                        element.OnPanUpdated(this, new PanUpdatedEventArgs(GestureStatus.Running, e.ActionIndex, totalX, totalY));
                    }

                    return true;

                case MotionEventActions.Up:
                    if (_panStarted)
                    {
                        EndPan(element, totalX, totalY);
                    }
                    return true;

                default:
                    return base.OnTouchEvent(e);
            }
        }

        private void EndPan(CardsView element, float x, float y)
        {
            element.OnPanUpdated(this, new PanUpdatedEventArgs(GestureStatus.Completed, 0, x, y));
            _panStarted = false;
        }
    }
}
