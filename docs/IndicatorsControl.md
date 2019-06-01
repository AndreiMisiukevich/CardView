### IndicatorsControl

This is the indicator control, usually displayed as a set of circles(dots) at the bottom of the control.

### Setup

If the IndicatorsControl is nested in a [CarouselView](CarouselView.md) or [CardView](CardView.md) then the following properties are bound to the parent:
- SelectedIndex
- ItemsCount
- IndicatorsContexts
- IsUserInteractionRunning
- IsAutoInteractionRunning

### Custom Indicator

The default indicator is a `Frame` control that is a 10px by 10px circle. This can be change by setting the `ItemTemplate` then setting the `SelectedIndicatorStyle` and `UnselectedIndicatorStyle` styles.

### Properties

Below are the properties for the IndicatorsControl:

Property | Type | Default | Description
--- | --- | --- | ---
SelectedIndex | `int` | 0 | The currently selected index this will disaplyed with the `SelectedIndicatorStyle`
ItemsCount | `int` | 0 | The number of items the indicator should display.
SelectedIndicatorStyle | `Style` | DefaultSelectedIndicatorItemStyle - Frame style that sets `Background` to White with .8 Alpha  | The style used when the indicator is selected.
UnselectedIndicatorStyle | `Style` | DefaultUnselectedIndicatorItemStyle - Frame style that sets `Background` to Transparent and `OutlineColor` to White with .8 Alpha | The style used when the indicator is not selected.
UseCardItemsAsIndicatorsBindingContexts | `bool` | true | Sets the indicators `BindingContext` to the `ItemSource` in the parent.
IsUserInteractionRunning | `bool` | true | Is used when `ToFadeDuration` is greater than 0 to show and hide the IndicatorControl.
IsAutoInteractionRunning | `bool` | true | Is used when `ToFadeDuration` is greater than 0 to show and hide the IndicatorControl.
MinimumVisibleIndicatorsCount | `int` | 1 | Minimum allowed count of indicators for showing the control.
MaximumVisibleIndicatorsCount | `int` | int.MaxValue | Maximum allowed count of indicators for showing the control.
IndicatorsContexts | `IList` | null | Binding contexts for each of the indicators, can be used to bind in the `ItemTemplate`
ItemTemplate | `DataTemplate` | [IndicatorItemView](IndicatorItemView.md) | The data template used for each of the indicators.
UseParentAsBindingContext | `bool` | true | Set the `BindingContext` of this control to the parent.
ToFadeDuration | `int` | 0 | The duration in milliseconds beforethe indicator control will fade out so it is not visible.

