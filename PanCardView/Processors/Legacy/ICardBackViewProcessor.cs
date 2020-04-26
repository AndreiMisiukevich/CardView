using System.Collections.Generic;
using Xamarin.Forms;

namespace PanCardView.Processors
{
    public interface ICardBackViewProcessor : ICardProcessor
    {
        void HandleCleanView(IEnumerable<View> views, CardsView cardsView);
    }
}
