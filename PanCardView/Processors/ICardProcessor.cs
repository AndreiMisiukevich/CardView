using Xamarin.Forms;
using System.Threading.Tasks;
using PanCardView.Enums;

namespace PanCardView.Processors
{
    public interface ICardProcessor
    {
        void InitView(View view, CardsView cardsView, PanItemPosition panItemPosition);
        void AutoNavigate(View view, CardsView cardsView, PanItemPosition panItemPosition);
        void HandlePanChanged(View view, CardsView cardsView, double xPos, PanItemPosition panItemPosition);
        Task HandlePanReset(View view, CardsView cardsView, PanItemPosition panItemPosition);
        Task HandlePanApply(View view, CardsView cardsView, PanItemPosition panItemPosition);
    }
}
