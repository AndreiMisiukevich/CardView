### CardsView

This is the "Tinder" style card swiping view. This is also the base for the [CarouselView](CarouselView.md).

### Setup

**TODO**

### Properties

Below are the properties for the CardsView:

Property | Type | Default | Description
--- | --- | --- | ---
BackViewsDepth | `int` | 1 | This determines how many views should be loaded in the `SetupNextView()` and `SetupPrevView()`
DesiredMaxChildrenCount | `int` | 6 | Used to determine when to remove views from the Children stack of the control. This is used when the auto navigation animation is not processing, e.g., when `isProcessingNow` is set to false.
IsAutoInteractionRunning | `bool` | false | Determines if the auto navigation is running, e.g., This is set to true in `StartAutoNavigation()` and false in `EndAutoNavigation()`.
IsCyclical | `bool` | true | Determines if the control can cycle round from the last view to the first, allows for infinite swiping.
IsOnlyForwardDirection | `bool` | false | Set to only allow the control to move forward and not have the abilitly to go back to previous cards.
IsUserInteractionEnabled | `bool` | true | Determines if the control can be interacted with, e.g., `OnTouch` events.
IsUserInteractionInCourse | `bool` | true | Determines if the control should forbid to start new interaction with card before previous ending.
IsUserInteractionRunning | `bool` | false | Determines if the UserInteration is running, e.g., This is set to true in `OnTouchStarted()` and false in `OnTouchEnded()`.
IsViewCacheEnabled | `bool` | true | Determines wether the control should retrieve the next view from the views pool or create a new one each time.
ItemAppearingCommand | `ICommand` | null | The command that is executed when a new item is displayed.
ItemDisappearingCommand | `ICommand` | null | The command that is executed when the item is no longer the displayed item.
ItemsCount | `int` | -1 | The count of the items in `ItemsSource`. Primarily used to work out the cyclic index.
ItemsSource | `IList` | null | Sets the items source of the control. If the source is an `ObservableCollection` the CollectionChanged events are subscribed to.
ItemTappedCommand | `ICommand` | null | The command that is executed when the item is tapped.
ItemTemplate | `DataTemplate` | null | Sets the data Template that the `ItemsSource` will bind too.
MaxChildrenCount | `int` | 12 | Used to determine when to remove views from the Children stack on the control. This is used when the auto navigation animation is processing, e.g., when `isProcessingNow` is set to true.
MoveDistance | `double` | -1 | The distance the swipe needs to move in order for a page to move to the next.
MoveThresholdDistance | `double` | 3.0 | **Only used in Android**. The distance threshold needed to detect a swipe.
MoveWidthPercentage | `double` | .325 | The percentage of the control's width needed to move in order for a page to move to the next.
SelectedIndex | `int` | -1 | Sets the selected index of the control and updates the `SelectedItem`.
SelectedItem | `object` | null | Sets the selected item of the control and updates the `SelectedIndex`. If the item's index is not found it sets `SelectedIndex` to -1.
SlideShowDuration | `int` | 0 | This property determines the number of milliseconds to wait before disaplying the next card. 
SwipeThresholdDistance | `double` | 17.0 | This is used in conjuction with `SwipeThresholdTime` to detect if a swipe has happened. The swipe distance needs to be greater than or equal to: `SwipeThresholdDistance * timeDiff.TotalMilliseconds / SwipeThresholdTime.TotalMilliseconds`
SwipeThresholdTime | `TimeSpan` | Android: 100ms Others:60ms | This is used in conjuction with `SwipeThresholdDistance` to detect if a swipe has happened. The swipe distance needs to be greater than or equal to: `SwipeThresholdDistance * timeDiff.TotalMilliseconds / SwipeThresholdTime.TotalMilliseconds`
UserInteractedCommand | `ICommand` | null | The command that is executed when `OnTouchStarted()`, `OnTouchChanged()` and `OnTouchEnded()` is called.
UserInteractionDelay | `int` | 200 | The time in milliseconds before user interaction should start
