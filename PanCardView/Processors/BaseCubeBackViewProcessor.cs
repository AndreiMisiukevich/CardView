using Xamarin.Forms;
using static PanCardView.Processors.Constants;

namespace PanCardView.Processors
{
    public class BaseCubeBackViewProcessor : BaseCarouselBackViewProcessor
    {
        protected override void SetTranslationX(View view, double value, CardsView cardsView)
        {
            view.TranslationX = value;
            view.RotationY = value * Angle90 / cardsView.Width;
        }
    }
}