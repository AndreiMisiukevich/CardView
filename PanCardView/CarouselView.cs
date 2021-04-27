using PanCardView.Processors;
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

        protected override double DefaultMoveSizePercentage => .3;

        protected override bool DefaultIsCyclical => true;

        [EditorBrowsable(EditorBrowsableState.Never)]
        public new static void Preserve()
        {
        }
    }
}
