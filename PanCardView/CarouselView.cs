﻿using PanCardView.Processors;

namespace PanCardView
{
    public class CarouselView : CardsView
    {
        public CarouselView() : this(new BaseCarouselFrontViewProcessor(), new BaseCarouselBackViewProcessor())
        {
        }

        public CarouselView(ICardProcessor frontViewProcessor, ICardBackViewProcessor backViewProcessor) : base(frontViewProcessor, backViewProcessor)
        {
            IsClippedToBounds = true;
        }

        protected override double DefaultMoveWidthPercentage => .3;

        protected override bool DefaultIsCyclical => true;
    }
}
