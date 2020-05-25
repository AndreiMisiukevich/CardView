using System;
using System.ComponentModel;
using PanCardView.Processors;

namespace PanCardView
{
    public class CubeView : CarouselView
    {
        public CubeView() : this(new CubeProcessor())
        {
        }

        public CubeView(IProcessor processor) : base(processor)
        {
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete]
        public CubeView(ICardProcessor frontViewProcessor, ICardBackViewProcessor backViewProcessor)
            : base(frontViewProcessor ?? new BaseCubeFrontViewProcessor(), backViewProcessor ?? new BaseCubeBackViewProcessor())
        {
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public new static void Preserve()
        {
        }
    }
}
