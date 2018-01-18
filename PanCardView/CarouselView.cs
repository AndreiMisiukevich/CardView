// 11(c) Andrei Misiukevich
using System;
using PanCardView.Processors;

namespace PanCardView
{
    public class CarouselView : CardsView
    {
        public CarouselView() : this(new BaseCardFrontViewProcessor(), new BaseCarouselBackViewProcessor())
        {
        }

        public CarouselView(ICardProcessor frontViewProcessor, ICardProcessor backViewProcessor) : base(frontViewProcessor, backViewProcessor)
        {
        }
    }
}
