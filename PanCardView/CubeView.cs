using System.ComponentModel;
using PanCardView.Processors;

namespace PanCardView
{
    public class CubeView : CarouselView
    {
        public CubeView() : this(new BaseCubeFrontViewProcessor(), new BaseCubeBackViewProcessor())
        {
        }

        public CubeView(ICardProcessor frontViewProcessor, ICardBackViewProcessor backViewProcessor) : base(frontViewProcessor, backViewProcessor)
        {
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public new static void Preserve()
        {
        }
    }
}
