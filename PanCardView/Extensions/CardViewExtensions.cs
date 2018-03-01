using Xamarin.Forms;
using PanCardView.Controls;

namespace PanCardView.Extensions
{
    public static class CardViewExtensions
    {
        public static CardsView AsCardView(this BindableObject bindable)
        => bindable as CardsView;

        public static IndicatorsControl AsIndicatorsControl(this BindableObject bindable)
        => bindable as IndicatorsControl;
    }
}
