// 11(c) Andrei Misiukevich
using System;
using Xamarin.Forms;
namespace PanCardView
{
    internal static class CardViewExtensions
    {
        internal static CardView AsCardView(this BindableObject bindable)
        => bindable as CardView;
    }
}
