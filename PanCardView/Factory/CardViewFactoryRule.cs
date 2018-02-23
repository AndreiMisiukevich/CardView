using System;
using Xamarin.Forms;

namespace PanCardView.Factory
{
    [Obsolete("This class is obsolete and will be removed soon. Use DataTemplate instead")]
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
