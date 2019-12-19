### CardsView

This is the "Tinder" style card swiping view. This is also the base for the [CarouselView](CarouselView.md).

### Events

Event | Delegate type | Description
--- | --- | ---
UserInteracted | `CardsViewUserInteractedHandler` | It is raised when user interacts with any card (Start, End, Move).
ItemDisappearing | `CardsViewItemDisappearingHandler` | It is raised when any card is disappearing.
ItemBeforeAppearing | `CardsViewItemBeforeAppearingHandler` | It is raised when any card is started appearing (before animation).
ItemAppearing | `CardsViewItemAppearingHandler` | It is raised when any card is ended appearing (after animation).
ItemSwiped | `CardsViewItemSwipedHandler` | It is raised when user swipes any card (Left, Right, Up, Down).
ViewsInUseCollectionChanged | `NotifyCollectionChangedEventHandler` | It is raised when ViewsInUseCollection changed (Add, Remove).

### Properties

Below are the properties for the CardsView:

Property | Type | Default | Description
--- | --- | --- | ---
BackViewsDepth | `int` | 1 | This determines how many views should be loaded in the `SetupNextView()` and `SetupPrevView()`
DesiredMaxChildrenCount | `int` | 7 | Used to determine when to remove views from the Children stack of the control. This is used when the auto navigation animation is not processing, e.g., when `isProcessingNow` is set to false.
IsAutoInteractionRunning | `bool` | false | Determines if the auto navigation is running, e.g., This is set to true in `StartAutoNavigation()` and false in `EndAutoNavigation()`.
IsAutoNavigatingAnimationEnabled | `bool` | true | Determines if the control should animate cards, if selected item/index was chagend without user interaction (e.g. from code).
IsCyclical | `bool` | true | Determines if the control can cycle round from the last view to the first, allows for infinite swiping.
CurrentDiff | `double` | 0.0 | Shows current pan diff.
IsOnlyForwardDirection | `bool` | false | Determines if the control should always take next elements for putting them on sides (like Tinder).
IsPanSwipeEnabled | `bool` | true | Set to allow the control to detec swipes from fast pan movements.
VerticalSwipeThresholdDistance | `double` | 30.0 | **Only used in Android** Determines distance for recognizing movement as vertical swipe.
ShouldThrottlePanInteraction | `bool` | false | **Only used in Android** Set to allow throttling pan gesture interaction.
IsVerticalSwipeEnabled | `bool` | true | **Only used in Android and iOS** Determines if vertical swipes are enabled.
OppositePanDirectionDisablingThreshold | `double` | double.PositiveInfinity | Determines distance for recognizing movement as opposite direction and disable it if it's needed.
IsRightToLeftFlowDirectionEnabled | `bool` | false | Set RightToLeft flow direction for current control.
IsUserInteractionEnabled | `bool` | true | Determines if the control can be interacted with, e.g., `OnTouch` events.
IsUserInteractionInCourse | `bool` | true | Determines if the control should forbid to start new interaction with card before previous ending.
IsUserInteractionRunning | `bool` | false | Determines if the UserInteration is running, e.g., This is set to true in `OnTouchStarted()` and false in `OnTouchEnded()`.
IsViewReusingEnabled | `bool` | true | Determines wether the control should try to retrieve view from the views pool or create an own view for certain item.
IsPanInteractionEnabled | `bool` | true | Determines wether the control should handle pan gesture events or it should handle only swipes.
IsHorizontalOrientation | `bool` | true | Determines what is the orientation of the control.
IsNextItemPanInteractionEnabled | `bool` | true | Determines wether next items are available for user interaction.
IsPrevItemPanInteractionEnabled | `bool` | true | Determines wether previous items are available for user interaction.
ItemBeforeAppearingCommand | `ICommand` | null | The command that is executed when a new item is started to display.
ItemAppearingCommand | `ICommand` | null | The command that is executed when a new item is ended to display.
ItemDisappearingCommand | `ICommand` | null | The command that is executed when the item is no longer the displayed item.
ItemsCount | `int` | -1 | The count of the items in `ItemsSource`. Primarily used to work out the cyclic index.
ItemsSource | `IList` | null | Sets the items source of the control. If the source is an `ObservableCollection` the CollectionChanged events are subscribed to.
ItemSwipedCommand | `ICommand` | null | The command that is executed when the item is swiped.
ItemTemplate | `DataTemplate` | null | Sets the data Template that the `ItemsSource` will bind too.
MaxChildrenCount | `int` | 12 | Used to determine when to remove views from the Children stack on the control. This is used when the auto navigation animation is processing, e.g., when `isProcessingNow` is set to true.
MoveDistance | `double` | -1 | The distance the swipe needs to move in order for a page to move to the next.
MoveThresholdDistance | `double` | 3.0 | **Only used in Android**. The distance threshold needed to detect a swipe.
MoveSizePercentage | `double` | .325 | The percentage of the control's size (Width or Height) needed to move in order for a page to move to the next.
SelectedIndex | `int` | -1 | Sets or gets the selected index of the control and updates the `SelectedItem`.
SelectedItem | `object` | null | Sets or gets the selected item of the control and updates the `SelectedIndex`. If the item's index is not found it sets `SelectedIndex` to -1.
SlideShowDuration | `int` | 0 | This property determines the number of milliseconds to wait before disaplying the next card. 
SwipeThresholdDistance | `double` | 17.0 | This is used in conjuction with `SwipeThresholdTime` to detect if a swipe has happened. The swipe distance needs to be greater than or equal to: `SwipeThresholdDistance * timeDiff.TotalMilliseconds / SwipeThresholdTime.TotalMilliseconds`
SwipeThresholdTime | `TimeSpan` | Android: 100ms Others:60ms | This is used in conjuction with `SwipeThresholdDistance` to detect if a swipe has happened. The swipe distance needs to be greater than or equal to: `SwipeThresholdDistance * timeDiff.TotalMilliseconds / SwipeThresholdTime.TotalMilliseconds`
UserInteractedCommand | `ICommand` | null | The command that is executed when `OnTouchStarted()`, `OnTouchChanged()` and `OnTouchEnded()` is called.
UserInteractionDelay | `int` | 200 | The time in milliseconds before user interaction should start
