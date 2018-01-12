// 221(c) Andrei Misiukevich
using System;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Android.Content;
using PanCardView.Droid;
using PanCardView;
using Android.Views;
using Android.Util;

[assembly: ExportRenderer(typeof(CardsView), typeof(CardsViewRenderer))]
namespace PanCardView.Droid
{
    public class CardsViewRenderer : VisualElementRenderer<CardsView>
    {
        private bool _panStarted;
        private float _startX;
        private float _startY;

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
            var totalX = ToDp(e.GetX());
            var totalY = ToDp(e.GetY());

            var currentX = totalX - _startX;
            var currentY = totalY - _startY;

            var element = Element as CardsView;

            switch (e.Action)
            {
                case MotionEventActions.Down:
                    _panStarted = true;
                    _startX = totalX;
                    _startY = totalY;
                    element.OnPanUpdated(this, new PanUpdatedEventArgs(GestureStatus.Started, e.ActionIndex, 0, 0));
                    return true;

                case MotionEventActions.Move:
                    if (_panStarted)
                    {
                        if (e.PointerCount > 1)
                        {
                            EndPan(element, currentX, currentY);
                        }
                        else
                        {
                            element.OnPanUpdated(this, new PanUpdatedEventArgs(GestureStatus.Running, e.ActionIndex, currentX, currentY));
                        }
                    }
                    return true;

                case MotionEventActions.Up:
                    if (_panStarted)
                    {
                        EndPan(element, currentX, currentY);
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
            _startX = 0;
            _startY = 0;
        }

        private float ToDp(float px)
        => px / Context.Resources.DisplayMetrics.Density;
    }
}
