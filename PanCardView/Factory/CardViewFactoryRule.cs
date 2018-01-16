// 21(c) Andrei Misiukevich
using System;
using Xamarin.Forms;

namespace PanCardView.Factory
{
    public sealed class CardViewFactoryRule
    {
        public Func<View> Creator { get; set; }
    }
}
