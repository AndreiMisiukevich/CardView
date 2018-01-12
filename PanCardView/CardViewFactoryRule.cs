// 21(c) Andrei Misiukevich
using System;
using Xamarin.Forms;
namespace PanCardView
{
    public sealed class CardViewFactoryRule
    {
        public Func<CardItemView> Creator { get; set; }
    }
}
