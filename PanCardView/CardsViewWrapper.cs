using Xamarin.Forms;

namespace PanCardView
{
    public class CardsViewWrapper : ContentView
    {
        public CardsView Child { get; set; }
        public CardsViewWrapper()
        {
            if (Device.RuntimePlatform != Device.iOS)
            {
                InputTransparent = true;
            }
        }

        protected override void OnChildAdded(Element child)
        {
            if (child is CardsView)
            {
                Child = child as CardsView;
            }
            base.OnChildAdded(child);
        }
    }

}
