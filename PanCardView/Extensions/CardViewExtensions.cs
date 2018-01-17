using Xamarin.Forms;

namespace PanCardView.Extensions
{
    internal static class CardViewExtensions
    {
        internal static CardsView AsCardView(this BindableObject bindable)
        => bindable as CardsView;
    }
}
