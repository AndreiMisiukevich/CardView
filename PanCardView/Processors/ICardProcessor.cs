using Xamarin.Forms;
using System.Threading.Tasks;
using PanCardView.Enums;

namespace PanCardView.Processors
{
	public interface ICardProcessor
	{
		void HandleInitView(View view, CardsView cardsView, AnimationDirection animationDirection);
		void HandlePanChanged(View view, CardsView cardsView, double xPos, AnimationDirection animationDirection);

		Task HandleAutoNavigate(View view, CardsView cardsView, AnimationDirection animationDirection);
		Task HandlePanReset(View view, CardsView cardsView, AnimationDirection animationDirection);
		Task HandlePanApply(View view, CardsView cardsView, AnimationDirection animationDirection);
	}
}
