using Xamarin.Forms;
using System.Threading.Tasks;
using PanCardView.Enums;

namespace PanCardView.Processors
{
	public interface ICardProcessor
	{
		void HandleInitView(View view, CardsView cardsView, PanItemPosition panItemPosition);
		void HandleAutoNavigate(View view, CardsView cardsView, PanItemPosition panItemPosition);
		void HandlePanChanged(View view, CardsView cardsView, double xPos, PanItemPosition panItemPosition);
		Task HandlePanReset(View view, CardsView cardsView, PanItemPosition panItemPosition);
		Task HandlePanApply(View view, CardsView cardsView, PanItemPosition panItemPosition);
	}
}
