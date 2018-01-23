using System;
using Xamarin.Forms;

namespace PanCardView.Factory
{
    public sealed class CardViewFactoryRule
    {
        public CardViewFactoryRule()
        {
        }

        public CardViewFactoryRule(Func<View> creator)
        {
            Creator = creator;
        }

        public Func<View> Creator { get; set; }
    }
}
