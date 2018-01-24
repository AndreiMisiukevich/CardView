using PanCardView.Processors;

namespace PanCardView
{
    public class CarouselView : CardsView
    {
        public CarouselView() : this(new BaseCarouselFrontViewProcessor(), new BaseCarouselBackViewProcessor())
        {
        }

        public CarouselView(ICardProcessor frontViewProcessor, ICardProcessor backViewProcessor) : base(frontViewProcessor, backViewProcessor)
        {
    		IsClippedToBounds = true;
        }
    }
}
