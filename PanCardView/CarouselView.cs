using PanCardView.Processors;
using System;
using System.ComponentModel;

namespace PanCardView
{
    public class CarouselView : CardsView
    {
        public CarouselView() : this(new CarouselProcessor())
        {
        }

        public CarouselView(IProcessor processor) : base(processor)
        {
            IsClippedToBounds = true;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete]
        public CarouselView(ICardProcessor frontViewProcessor, ICardBackViewProcessor backViewProcessor)
            : base(frontViewProcessor ?? new BaseCarouselFrontViewProcessor(), backViewProcessor ?? new BaseCarouselBackViewProcessor())
        {
            IsClippedToBounds = true;
        }

        protected override double DefaultMoveSizePercentage => .3;

        protected override bool DefaultIsCyclical => true;

        [EditorBrowsable(EditorBrowsableState.Never)]
        public new static void Preserve()
        {
        }
    }
}
