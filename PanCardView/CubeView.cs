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
        public new static void Preserve()
        {
        }
    }
}
