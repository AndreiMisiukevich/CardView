### CoverFlowView

The CoverFlowView is based on the [CarouselView](CarouselView.md) but has the following set by default:

	DefaultBackViewsDepth = 2;
	DefaultMaxChildrenCount = 17;
	MoveWidthPercDefaultDesiredMaxChildrenCountentage = 12;

### Properties

The CarouselView is a subclass of [CardsView](CardsView.md) so shares alot of the same properties. Below are the CarouselView specific properties:

Property | Type | Default | Description
--- | --- | --- | ---
PositionShiftPercentage | `double` | 0.0 | Percentage of shift to current (center) view (Has values from 0 to 1, where 0 is full CoverFlowView.Width and 1 is the same position with current view.
PositionShiftValue | `double` | 0.0 | Absolute shift to current (center) view.
