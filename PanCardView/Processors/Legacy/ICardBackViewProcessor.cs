using System;
using System.Collections.Generic;
using System.ComponentModel;
using Xamarin.Forms;

namespace PanCardView.Processors
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete]
    public interface ICardBackViewProcessor : ICardProcessor
    {
        void HandleCleanView(IEnumerable<View> views, CardsView cardsView);
    }
}
