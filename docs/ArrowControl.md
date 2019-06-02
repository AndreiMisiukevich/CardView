### ArrowControl LeftArrowControl RightArrowControl

This is the arrows controls, usually displayed as a circles with arrows inside (left and right sides). We use them for easy navigation

### Setup

If the LeftArrowControl/RightArrowControl is nested in a [CarouselView](CarouselView.md) or [CardView](CardView.md) then the following properties are bound to the parent:
- SelectedIndex
- ItemsCount
- IsCyclical
- UseParentAsBindingContext
- IsUserInteractionRunning
- IsAutoInteractionRunning
- IsRight

### Properties

Below are the properties for the IndicatorsControl:

Property | Type | Default | Description
--- | --- | --- | ---
SelectedIndex | `int` | 0 | The currently selected index.
ItemsCount | `int` | 0 | The number of items the indicator should display.
IsCyclical | `bool` | true | Detect is parent Cards/Carousel cyclical.
IsUserInteractionRunning | `bool` | true | Is used when `ToFadeDuration` is greater than 0 to show and hide the IndicatorControl.
IsAutoInteractionRunning | `bool` | true | Is used when `ToFadeDuration` is greater than 0 to show and hide the IndicatorControl.
UseParentAsBindingContext | `bool` | true | Set the `BindingContext` of this control to the parent.
ToFadeDuration | `int` | 0 | The duration in milliseconds beforethe indicator control will fade out so it is not visible.
IsRight | `bool` | true | The position of the control (Right by default).
ImageSource | `ImageSource` | WhiteLeftArrowImageSource / WhiteRightArrowImageSource| The image source of left / right arrow, can be set black arrows via ```PanCardView.Resources.ResourceInfo``` or any image you want.
