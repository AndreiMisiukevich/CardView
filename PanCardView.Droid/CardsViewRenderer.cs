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

        public override bool OnTouchEvent(MotionEvent e)
        {
            base.OnTouchEvent(e);

            if (e.Action == MotionEventActions.Up)
            {
                (Element as CardsView)?.OnPanUpdated(this, new PanUpdatedEventArgs(GestureStatus.Completed, e.ActionIndex, 0, 0));
            }

            return true;
        }
    }
}
