### Setup

### Custom Indicator

The default indicator is a `Frame` control that is a 10px by 10px circle. This is changed


### Properties

Below are the properties for the IndicatorsControl:

Property | Type | Default | Description
--- | --- | --- | ---
SelectedIndex | `int` | 0 | The currently selected index this will disaplyed with the `SelectedIndicatorStyle`
ItemsCount | `int` | 0 | The number of items the indicator should display.
SelectedIndicatorStyle | `Style` | DefaultSelectedIndicatorItemStyle - Frame style that sets `Background` to White with .8 Alpha  | The style used when the indicator is selected.
UnselectedIndicatorStyle | `Style` | DefaultUnselectedIndicatorItemStyle - Frame style that sets `Background` to TRansparent and `OutlineColor` to White with .8 Alpha | The style used when the indicator is not selected.
UseCardItemsAsIndicatorsBindingContexts | `bool` | true | 
IsUserInteractionRunning | `bool` | true | 
IsAutoInteractionRunning | `bool` | true | 
IndicatorsContexts | `IList` | null | 
ItemTemplate | `DataTemplate` | [IndicatorItemView](IndicatorItemView.md) | 
UseParentAsBindingContext | `bool` | true | 
ToFadeDuration | `int` | 0 |

