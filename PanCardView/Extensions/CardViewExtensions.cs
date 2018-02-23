using Xamarin.Forms;
using PanCardView.Controls;

namespace PanCardView.Extensions
{
    internal static class CardViewExtensions
    {
        internal static CardsView AsCardView(this BindableObject bindable)
        => bindable as CardsView;

        internal static IndicatorsControl AsIndicatorsControl(this BindableObject bindable)
        => bindable as IndicatorsControl;
    }
}
