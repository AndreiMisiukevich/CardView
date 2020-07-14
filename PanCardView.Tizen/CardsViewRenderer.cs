using PanCardView;
using PanCardView.Enums;
using PanCardView.Tizen;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.Tizen;
using ElmSharp;
using Point = Xamarin.Forms.Point;
using Layout = Xamarin.Forms.Layout;

[assembly: ExportRenderer(typeof(CardsView), typeof(CardsViewRenderer))]
namespace PanCardView.Tizen
{
    [Preserve(AllMembers = true)]
    public class CardsViewRenderer : LayoutRenderer
    {
        GestureLayer _gestureLayer;
        CardsView CardsViewElement => Element as CardsView;

        public static void Preserve() => Preserver.Preserve();

        public CardsViewRenderer()
        {
            RegisterPropertyHandler(CardsView.IsVerticalSwipeEnabledProperty, UpdateIsVerticalSwipeEnabled);
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Layout> e)
        {
            base.OnElementChanged(e);
            Initialize();
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _gestureLayer?.Unrealize();
                _gestureLayer = null;
            }
            base.Dispose(disposing);
        }

        private void Initialize()
        {
            _gestureLayer?.Unrealize();
            _gestureLayer = new GestureLayer(NativeView);
            _gestureLayer.Attach(NativeView);

            _gestureLayer.SetMomentumCallback(GestureLayer.GestureState.Move, OnMoved);
            _gestureLayer.SetMomentumCallback(GestureLayer.GestureState.End, OnEnd);
            _gestureLayer.SetMomentumCallback(GestureLayer.GestureState.Abort, OnEnd);
        }

        private void OnMoved(GestureLayer.MomentumData moment)
        {
        }

        private void OnEnd(GestureLayer.MomentumData moment)
        {
            var direction = SwipeDirectionHelper.GetSwipeDirection(new Point(moment.X1, moment.Y1), new Point(moment.X2, moment.Y2));

            var swipeDirection = direction == SwipeDirection.Left
                ? ItemSwipeDirection.Left
                                : direction == SwipeDirection.Right
                                    ? ItemSwipeDirection.Right
                                    : direction == SwipeDirection.Up
                                        ? ItemSwipeDirection.Up
                                        : ItemSwipeDirection.Down;
            CardsViewElement?.OnSwiped(swipeDirection);
        }

        private void UpdateIsVerticalSwipeEnabled()
        {
            Initialize();
        }
    }
}