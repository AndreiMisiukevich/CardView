using PanCardView.Controls;
using PanCardView.Processors;
using System;
using System.Linq;

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
			IsCyclical = true;
			MoveWidthPercentage = .3;
		}

		[Obsolete("No need use this property. Just add the IndicatorsControl as child element.")]
		public IndicatorsControl IndicatorsControl
		{
			set
			{
				var control = Children.FirstOrDefault(c => c is IndicatorsControl);
				if (control != null)
				{
					Children.Remove(control);
				}

				if (value != null)
				{
					Children.Add(value);
				}
			}
		}
	}
}