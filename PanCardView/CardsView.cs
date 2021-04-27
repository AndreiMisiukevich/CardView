using PanCardView.Behaviors;
using PanCardView.Enums;
using PanCardView.Extensions;
using PanCardView.Processors;
using PanCardView.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using static System.Math;
using System.Threading;
using System.Runtime.CompilerServices;
using PanCardView.EventArgs;
using PanCardView.Delegates;
using System.ComponentModel;

namespace PanCardView
{
    public class CardsView : AbsoluteLayout
    {
        public static readonly BindableProperty SelectedIndexProperty = BindableProperty.Create(nameof(SelectedIndex), typeof(int), typeof(CardsView), -1, BindingMode.TwoWay, propertyChanged: (bindable, oldValue, newValue) =>
        {
            var view = bindable.AsCardsView();
            view.SetSelectedItem();
            view.OldIndex = (int)oldValue;
            if (view.ShouldIgnoreSetCurrentView)
            {
                view.ShouldIgnoreSetCurrentView = false;
                return;
            }
            view.ForceRedrawViews();
        });

        public static readonly BindableProperty SelectedItemProperty = BindableProperty.Create(nameof(SelectedItem), typeof(object), typeof(CardsView), null, BindingMode.TwoWay, propertyChanged: (bindable, oldValue, newValue) =>
        {
            var view = bindable.AsCardsView();
            view.SelectedIndex = view.ItemsSource?.FindIndex(newValue) ?? -1;
        });

        public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable), typeof(CardsView), null, propertyChanged: (bindable, oldValue, newValue) =>
        {
            bindable.AsCardsView().SetItemsSource(oldValue as IEnumerable);
        });

        public static readonly BindableProperty ItemTemplateProperty = BindableProperty.Create(nameof(ItemTemplate), typeof(DataTemplate), typeof(CardsView), propertyChanged: (bindable, oldValue, newValue) =>
        {
            bindable.AsCardsView().ForceRedrawViews();
        });

        public static readonly BindableProperty BackViewsDepthProperty = BindableProperty.Create(nameof(BackViewsDepth), typeof(int), typeof(CardsView), defaultValueCreator: b => b.AsCardsView().DefaultBackViewsDepth, propertyChanged: (bindable, oldValue, newValue) =>
        {
            bindable.AsCardsView().ForceRedrawViews();
        });

        public static readonly BindableProperty IsRightToLeftFlowDirectionEnabledProperty = BindableProperty.Create(nameof(IsRightToLeftFlowDirectionEnabled), typeof(bool), typeof(CardsView), false, propertyChanged: (bindable, oldValue, newValue) =>
        {
            bindable.AsCardsView().ForceRedrawViews();
        });

        public static readonly BindableProperty SlideShowDurationProperty = BindableProperty.Create(nameof(SlideShowDuration), typeof(int), typeof(CardsView), 0, propertyChanged: (bindable, oldValue, newValue) =>
        {
            bindable.AsCardsView().AdjustSlideShow();
        });

        public static readonly BindableProperty IsUserInteractionRunningProperty = BindableProperty.Create(nameof(IsUserInteractionRunning), typeof(bool), typeof(CardsView), false, BindingMode.OneWayToSource, propertyChanged: (bindable, oldValue, newValue) =>
        {
            bindable.AsCardsView().AdjustSlideShow((bool)newValue);
        });

        public static readonly BindableProperty IsAutoInteractionRunningProperty = BindableProperty.Create(nameof(IsAutoInteractionRunning), typeof(bool), typeof(CardsView), false, BindingMode.OneWayToSource, propertyChanged: (bindable, oldValue, newValue) =>
        {
            bindable.AsCardsView().AdjustSlideShow((bool)newValue);
        });

        public static readonly BindableProperty IsPanInteractionEnabledProperty = BindableProperty.Create(nameof(IsPanInteractionEnabled), typeof(bool), typeof(CardsView), true, propertyChanged: (bindable, oldValue, newValue) =>
        {
            bindable.AsCardsView().SetPanGesture(!(bool)newValue);
        });

        public static readonly BindableProperty CurrentDiffProperty = BindableProperty.Create(nameof(CurrentDiff), typeof(double), typeof(CardsView), 0.0, BindingMode.OneWayToSource);

        public static readonly BindableProperty IsHorizontalOrientationProperty = BindableProperty.Create(nameof(IsHorizontalOrientation), typeof(bool), typeof(CardsView), true);

        public static readonly BindableProperty IsNextItemPanInteractionEnabledProperty = BindableProperty.Create(nameof(IsNextItemPanInteractionEnabled), typeof(bool), typeof(CardsView), true);

        public static readonly BindableProperty IsPrevItemPanInteractionEnabledProperty = BindableProperty.Create(nameof(IsPrevItemPanInteractionEnabled), typeof(bool), typeof(CardsView), true);

        public static readonly BindableProperty ItemsCountProperty = BindableProperty.Create(nameof(ItemsCount), typeof(int), typeof(CardsView), -1, BindingMode.OneWayToSource);

        public static readonly BindableProperty IsUserInteractionEnabledProperty = BindableProperty.Create(nameof(IsUserInteractionEnabled), typeof(bool), typeof(CardsView), true);

        public static readonly BindableProperty MoveDistanceProperty = BindableProperty.Create(nameof(MoveDistance), typeof(double), typeof(CardsView), -1.0);

        public static readonly BindableProperty MoveSizePercentageProperty = BindableProperty.Create(nameof(MoveSizePercentage), typeof(double), typeof(CardsView), defaultValueCreator: b => b.AsCardsView().DefaultMoveSizePercentage);

        public static readonly BindableProperty IsOnlyForwardDirectionProperty = BindableProperty.Create(nameof(IsOnlyForwardDirection), typeof(bool), typeof(CardsView), false);

        public static readonly BindableProperty IsViewReusingEnabledProperty = BindableProperty.Create(nameof(IsViewReusingEnabled), typeof(bool), typeof(CardsView), true);

        public static readonly BindableProperty UserInteractionDelayProperty = BindableProperty.Create(nameof(UserInteractionDelay), typeof(int), typeof(CardsView), 0);

        public static readonly BindableProperty IsUserInteractionInCourseProperty = BindableProperty.Create(nameof(IsUserInteractionInCourse), typeof(bool), typeof(CardsView), true);

        public static readonly BindableProperty IsCyclicalProperty = BindableProperty.Create(nameof(IsCyclical), typeof(bool), typeof(CardsView), defaultValueCreator: b => b.AsCardsView().DefaultIsCyclical);

        public static readonly BindableProperty IsAutoNavigatingAnimationEnabledProperty = BindableProperty.Create(nameof(IsAutoNavigatingAnimationEnabled), typeof(bool), typeof(CardsView), true);

        public static readonly BindableProperty IsPanSwipeEnabledProperty = BindableProperty.Create(nameof(IsPanSwipeEnabled), typeof(bool), typeof(CardsView), true);

        public static readonly BindableProperty ShouldShareViewAmongSameItemsProperty = BindableProperty.Create(nameof(ShouldShareViewAmongSameItems), typeof(bool), typeof(CardsView), false);

        public static readonly BindableProperty MaxChildrenCountProperty = BindableProperty.Create(nameof(MaxChildrenCount), typeof(int), typeof(CardsView), defaultValueCreator: b => b.AsCardsView().DefaultMaxChildrenCount);

        public static readonly BindableProperty DesiredMaxChildrenCountProperty = BindableProperty.Create(nameof(DesiredMaxChildrenCount), typeof(int), typeof(CardsView), defaultValueCreator: b => b.AsCardsView().DefaultDesiredMaxChildrenCount);

        public static readonly BindableProperty SwipeThresholdDistanceProperty = BindableProperty.Create(nameof(SwipeThresholdDistance), typeof(double), typeof(CardsView), 17.0);

        public static readonly BindableProperty MoveThresholdDistanceProperty = BindableProperty.Create(nameof(MoveThresholdDistance), typeof(double), typeof(CardsView), 7.0);

        public static readonly BindableProperty VerticalSwipeThresholdDistanceProperty = BindableProperty.Create(nameof(VerticalSwipeThresholdDistance), typeof(double), typeof(CardsView), 30.0);

        public static readonly BindableProperty ShouldThrottlePanInteractionProperty = BindableProperty.Create(nameof(ShouldThrottlePanInteraction), typeof(bool), typeof(CardsView), false);

        public static readonly BindableProperty IsVerticalSwipeEnabledProperty = BindableProperty.Create(nameof(IsVerticalSwipeEnabled), typeof(bool), typeof(CardsView), true);

        public static readonly BindableProperty OppositePanDirectionDisablingThresholdProperty = BindableProperty.Create(nameof(OppositePanDirectionDisablingThreshold), typeof(double), typeof(CardsView), double.PositiveInfinity);

        public static readonly BindableProperty SwipeThresholdTimeProperty = BindableProperty.Create(nameof(SwipeThresholdTime), typeof(TimeSpan), typeof(CardsView), TimeSpan.FromMilliseconds(Device.RuntimePlatform == Device.Android ? 100 : 60));

        public static readonly BindableProperty UserInteractedCommandProperty = BindableProperty.Create(nameof(UserInteractedCommand), typeof(ICommand), typeof(CardsView), null);

        public static readonly BindableProperty ItemDisappearingCommandProperty = BindableProperty.Create(nameof(ItemDisappearingCommand), typeof(ICommand), typeof(CardsView), null);

        public static readonly BindableProperty ItemAppearingCommandProperty = BindableProperty.Create(nameof(ItemAppearingCommand), typeof(ICommand), typeof(CardsView), null);

        public static readonly BindableProperty ItemAppearedCommandProperty = BindableProperty.Create(nameof(ItemAppearedCommand), typeof(ICommand), typeof(CardsView), null);

        public static readonly BindableProperty ItemSwipedCommandProperty = BindableProperty.Create(nameof(ItemSwipedCommand), typeof(ICommand), typeof(CardsView), null);

        internal static readonly BindableProperty ShouldAutoNavigateToNextProperty = BindableProperty.Create(nameof(ShouldAutoNavigateToNext), typeof(bool?), typeof(CardsView), null);

        internal static readonly BindableProperty ProcessorDiffProperty = BindableProperty.Create(nameof(ProcessorDiff), typeof(double), typeof(CardsView), 0.0, BindingMode.OneWayToSource);

        [Xamarin.Forms.TypeConverter(typeof(ReferenceTypeConverter))]
        public IndicatorView IndicatorView
        {
            set
            {
                if (value == null) 
                {
                    return;
                }
                
                value.SetBinding(IndicatorView.PositionProperty, new Binding
                {
                    Path = nameof(SelectedIndex),
                    Source = this
                });
                
                value.SetBinding(IndicatorView.ItemsSourceProperty, new Binding
                {
                    Path = nameof(ItemsView.ItemsSource),
                    Source = this
                });
            }
        }

        public event CardsViewUserInteractedHandler UserInteracted;
        public event CardsViewItemDisappearingHandler ItemDisappearing;
        public event CardsViewItemAppearingHandler ItemAppearing;
        public event CardsViewItemAppearedHandler ItemAppeared;
        /// <summary>
        /// Raised when user swiped a view (swipe gesture). If you want to detect card switching, use ItemAppeared / ItemAppearing
        /// </summary>
        public event CardsViewItemSwipedHandler ItemSwiped;
        public event NotifyCollectionChangedEventHandler ViewsInUseCollectionChanged
        {
            add => _viewsInUseSet.CollectionChanged += value;
            remove => _viewsInUseSet.CollectionChanged -= value;
        }
        [EditorBrowsable(EditorBrowsableState.Never)]
        public event EventHandler<bool> AccessibilityChangeRequested;

        private readonly object _childrenLocker = new object();
        private readonly object _viewsInUseLocker = new object();
        private readonly object _setCurrentViewLocker = new object();
        private readonly object _sizeChangedLocker = new object();

        private readonly Dictionary<object, HashSet<View>> _viewsPool = new Dictionary<object, HashSet<View>>();
        private readonly Dictionary<Guid, IEnumerable<View>> _viewsGestureCounter = new Dictionary<Guid, IEnumerable<View>>();
        private readonly List<TimeDiffItem> _timeDiffItems = new List<TimeDiffItem>();
        private readonly ViewsInUseSet _viewsInUseSet = new ViewsInUseSet();
        private readonly InteractionQueue _interactions = new InteractionQueue();
        private readonly PanGestureRecognizer _panGesture = new PanGestureRecognizer();
        private readonly ContextAssignedBehavior _contextAssignedBehavior = new ContextAssignedBehavior();

        private View _currentView;
        private IEnumerable<View> _prevViews = Enumerable.Empty<View>();
        private IEnumerable<View> _nextViews = Enumerable.Empty<View>();
        private IEnumerable<View> _currentBackViews = Enumerable.Empty<View>();
        private IEnumerable<View> _currentInactiveBackViews = Enumerable.Empty<View>();

        private AnimationDirection _currentBackAnimationDirection;

        private Optional<CardsView> _parentCardsViewOption;
        private Optional<CardsView> _parentCardsViewTouchHandlerOption;

        private int _viewsChildrenCount;
        private bool _isPanStarted;
        private bool _isOppositePanDirectionIssueResolved;
        private bool _isViewInited;
        private bool _hasRenderer;
        private Size _parentSize;
        private DateTime _lastPanTime;
        private Task _animationTask;
        private CancellationTokenSource _slideShowTokenSource;
        private CancellationTokenSource _hardSetTokenSource;

        public CardsView() : this(new CardsProcessor())
        {
        }

        public CardsView(IProcessor processor)
        {
            Processor = processor;
            SetPanGesture();
        }

        private bool ShouldIgnoreSetCurrentView { get; set; }

        private bool ShouldSetIndexAfterPan { get; set; }

        internal double RealMoveDistance
        {
            get
            {
                var dist = MoveDistance;
                return dist > 0
                        ? dist
                        : Size * MoveSizePercentage;
            }
        }

        protected virtual int DefaultBackViewsDepth => 1;

        protected virtual double DefaultMoveSizePercentage => 0.325;

        protected virtual bool DefaultIsCyclical => false;

        protected virtual int DefaultMaxChildrenCount => 12;

        protected virtual int DefaultDesiredMaxChildrenCount => 7;

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool IsPanControllingByChild { get; set; }

        public View CurrentView
        {
            get => _currentView;
            private set
            {
                var oldValue = _currentView;
                _currentView = value;

                if (value != oldValue)
                {
                    AccessibilityChangeRequested?.Invoke(oldValue, false);
                    AccessibilityChangeRequested?.Invoke(value, true);
                }
            }
        }

        public IReadOnlyList<View> ViewsInUseCollection => _viewsInUseSet.Views;

        public double Size => IsHorizontalOrientation ? Width : Height;

        public IEnumerable<View> PrevViews
        {
            get => _prevViews;
            private set => _prevViews = value ?? Enumerable.Empty<View>();
        }

        public IEnumerable<View> NextViews
        {
            get => _nextViews;
            private set => _nextViews = value ?? Enumerable.Empty<View>();
        }

        public IEnumerable<View> CurrentBackViews
        {
            get => _currentBackViews;
            private set => _currentBackViews = value ?? Enumerable.Empty<View>();
        }

        public IEnumerable<View> CurrentInactiveBackViews
        {
            get => _currentInactiveBackViews;
            private set => _currentInactiveBackViews = value ?? Enumerable.Empty<View>();
        }

        public int OldIndex { get; private set; } = -1;

        public IProcessor Processor { get; }

        public int SelectedIndex
        {
            get => (int)GetValue(SelectedIndexProperty);
            set => SetValue(SelectedIndexProperty, value);
        }

        public object SelectedItem
        {
            get => GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        public IEnumerable ItemsSource
        {
            get => GetValue(ItemsSourceProperty) as IEnumerable;
            set => SetValue(ItemsSourceProperty, value);
        }

        public DataTemplate ItemTemplate
        {
            get => GetValue(ItemTemplateProperty) as DataTemplate;
            set => SetValue(ItemTemplateProperty, value);
        }

        public int BackViewsDepth
        {
            get => (int)GetValue(BackViewsDepthProperty);
            set => SetValue(BackViewsDepthProperty, value);
        }

        public bool IsRightToLeftFlowDirectionEnabled
        {
            get => (bool)GetValue(IsRightToLeftFlowDirectionEnabledProperty);
            set => SetValue(IsRightToLeftFlowDirectionEnabledProperty, value);
        }

        public int SlideShowDuration
        {
            get => (int)GetValue(SlideShowDurationProperty);
            set => SetValue(SlideShowDurationProperty, value);
        }

        public bool IsUserInteractionRunning
        {
            get => (bool)GetValue(IsUserInteractionRunningProperty);
            set => SetValue(IsUserInteractionRunningProperty, value);
        }

        public bool IsAutoInteractionRunning
        {
            get => (bool)GetValue(IsAutoInteractionRunningProperty);
            set => SetValue(IsAutoInteractionRunningProperty, value);
        }

        public bool IsPanInteractionEnabled
        {
            get => (bool)GetValue(IsPanInteractionEnabledProperty);
            set => SetValue(IsPanInteractionEnabledProperty, value);
        }

        public double CurrentDiff
        {
            get => (double)GetValue(CurrentDiffProperty);
            set => SetValue(CurrentDiffProperty, value);
        }

        public bool IsHorizontalOrientation
        {
            get => (bool)GetValue(IsHorizontalOrientationProperty);
            set => SetValue(IsHorizontalOrientationProperty, value);
        }

        public bool IsNextItemPanInteractionEnabled
        {
            get => (bool)GetValue(IsNextItemPanInteractionEnabledProperty);
            set => SetValue(IsNextItemPanInteractionEnabledProperty, value);
        }

        public bool IsPrevItemPanInteractionEnabled
        {
            get => (bool)GetValue(IsPrevItemPanInteractionEnabledProperty);
            set => SetValue(IsPrevItemPanInteractionEnabledProperty, value);
        }

        public int ItemsCount
        {
            get => (int)GetValue(ItemsCountProperty);
            private set => SetValue(ItemsCountProperty, value);
        }

        public bool IsUserInteractionEnabled
        {
            get => (bool)GetValue(IsUserInteractionEnabledProperty);
            set => SetValue(IsUserInteractionEnabledProperty, value);
        }

        public double MoveDistance
        {
            get => (double)GetValue(MoveDistanceProperty);
            set => SetValue(MoveDistanceProperty, value);
        }

        public double MoveSizePercentage
        {
            get => (double)GetValue(MoveSizePercentageProperty);
            set => SetValue(MoveSizePercentageProperty, value);
        }

        public bool IsOnlyForwardDirection
        {
            get => (bool)GetValue(IsOnlyForwardDirectionProperty);
            set => SetValue(IsOnlyForwardDirectionProperty, value);
        }

        public bool IsViewReusingEnabled
        {
            get => (bool)GetValue(IsViewReusingEnabledProperty);
            set => SetValue(IsViewReusingEnabledProperty, value);
        }

        public int UserInteractionDelay
        {
            get => (int)GetValue(UserInteractionDelayProperty);
            set => SetValue(UserInteractionDelayProperty, value);
        }

        public bool IsUserInteractionInCourse
        {
            get => (bool)GetValue(IsUserInteractionInCourseProperty);
            set => SetValue(IsUserInteractionInCourseProperty, value);
        }

        public bool IsCyclical
        {
            get => (bool)GetValue(IsCyclicalProperty);
            set => SetValue(IsCyclicalProperty, value);
        }

        public bool IsAutoNavigatingAnimationEnabled
        {
            get => (bool)GetValue(IsAutoNavigatingAnimationEnabledProperty);
            set => SetValue(IsAutoNavigatingAnimationEnabledProperty, value);
        }

        public bool ShouldShareViewAmongSameItems
        {
            get => (bool)GetValue(ShouldShareViewAmongSameItemsProperty);
            set => SetValue(ShouldShareViewAmongSameItemsProperty, value);
        }

        public bool IsPanSwipeEnabled
        {
            get => (bool)GetValue(IsPanSwipeEnabledProperty);
            set => SetValue(IsPanSwipeEnabledProperty, value);
        }

        public int MaxChildrenCount
        {
            get => (int)GetValue(MaxChildrenCountProperty);
            set => SetValue(MaxChildrenCountProperty, value);
        }

        public int DesiredMaxChildrenCount
        {
            get => (int)GetValue(DesiredMaxChildrenCountProperty);
            set => SetValue(DesiredMaxChildrenCountProperty, value);
        }

        public double SwipeThresholdDistance
        {
            get => (double)GetValue(SwipeThresholdDistanceProperty);
            set => SetValue(SwipeThresholdDistanceProperty, value);
        }

        /// <summary>
        /// Only for Android and iOS
        /// </summary>
        /// <value>Move threshold distance.</value>
        public double MoveThresholdDistance
        {
            get => (double)GetValue(MoveThresholdDistanceProperty);
            set => SetValue(MoveThresholdDistanceProperty, value);
        }

        /// <summary>
        /// Only for Android
        /// </summary>
        /// <value>Vertical swipe threshold distance.</value>
        public double VerticalSwipeThresholdDistance
        {
            get => (double)GetValue(VerticalSwipeThresholdDistanceProperty);
            set => SetValue(VerticalSwipeThresholdDistanceProperty, value);
        }

        /// <summary>
        /// Only for Android
        /// </summary>
        /// <value>Should throttle pan interaction.</value>
        public bool ShouldThrottlePanInteraction
        {
            get => (bool)GetValue(ShouldThrottlePanInteractionProperty);
            set => SetValue(ShouldThrottlePanInteractionProperty, value);
        }

        /// <summary>
        /// Only for Android and iOS
        /// </summary>
        /// <value>Is vertical swipe enabled.</value>
        public bool IsVerticalSwipeEnabled
        {
            get => (bool)GetValue(IsVerticalSwipeEnabledProperty);
            set => SetValue(IsVerticalSwipeEnabledProperty, value);
        }

        public double OppositePanDirectionDisablingThreshold
        {
            get => (double)GetValue(OppositePanDirectionDisablingThresholdProperty);
            set => SetValue(OppositePanDirectionDisablingThresholdProperty, value);
        }

        public TimeSpan SwipeThresholdTime
        {
            get => (TimeSpan)GetValue(SwipeThresholdTimeProperty);
            set => SetValue(SwipeThresholdTimeProperty, value);
        }

        public ICommand UserInteractedCommand
        {
            get => GetValue(UserInteractedCommandProperty) as ICommand;
            set => SetValue(UserInteractedCommandProperty, value);
        }

        public ICommand ItemDisappearingCommand
        {
            get => GetValue(ItemDisappearingCommandProperty) as ICommand;
            set => SetValue(ItemDisappearingCommandProperty, value);
        }

        public ICommand ItemAppearedCommand
        {
            get => GetValue(ItemAppearedCommandProperty) as ICommand;
            set => SetValue(ItemAppearedCommandProperty, value);
        }

        public ICommand ItemAppearingCommand
        {
            get => GetValue(ItemAppearingCommandProperty) as ICommand;
            set => SetValue(ItemAppearingCommandProperty, value);
        }

        public ICommand ItemSwipedCommand
        {
            get => GetValue(ItemSwipedCommandProperty) as ICommand;
            set => SetValue(ItemSwipedCommandProperty, value);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool? ShouldAutoNavigateToNext
        {
            get => GetValue(ShouldAutoNavigateToNextProperty) as bool?;
            set => SetValue(ShouldAutoNavigateToNextProperty, value);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public double ProcessorDiff
        {
            get => (double)GetValue(ProcessorDiffProperty);
            set => SetValue(ProcessorDiffProperty, value);
        }

        public object this[int index] => ItemsSource?.FindValue(index);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void Preserve()
        {
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void OnPanUpdated(object sender, PanUpdatedEventArgs e)
        => OnPanUpdated(e);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void OnPanUpdated(PanUpdatedEventArgs e)
        {
            var statusType = e.StatusType;

            if (IsPanControllingByChild || ShouldParentHandleTouch(statusType))
            {
                return;
            }

            var diff = e.TotalX;
            var oppositeDirectionDiff = e.TotalY;
            if (!IsHorizontalOrientation)
            {
                diff = e.TotalY;
                oppositeDirectionDiff = e.TotalX;
            }

            if (ItemsCount <= 0 || !IsPanInteractionEnabled)
            {
                switch(statusType)
                {
                    case GestureStatus.Started:
                        SetParentCardsViewOption();
                        SetupBackViews();
                        ResetActiveInactiveBackViews(diff);
                        SetParentTouchHandlerIfNeeded(statusType);
                        break;
                    case GestureStatus.Canceled:
                    case GestureStatus.Completed:
                        ClearParentCardsViewOption();
                        break;
                }
                return;
            }

            SetParentCardsViewOption();
            switch (statusType)
            {
                case GestureStatus.Started:
                    OnTouchStarted();
                    return;
                case GestureStatus.Running:
                    OnTouchChanged(diff, oppositeDirectionDiff);
                    SetParentTouchHandlerIfNeeded(statusType);
                    return;
                case GestureStatus.Canceled:
                case GestureStatus.Completed:
                    if (Device.RuntimePlatform == Device.Android)
                    {
                        OnTouchChanged(diff, oppositeDirectionDiff, true);
                        SetParentTouchHandlerIfNeeded(statusType);
                    }
                    OnTouchEnded();
                    ClearParentCardsViewOption();
                    return;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public async void OnSwiped(ItemSwipeDirection swipeDirection)
        {
            await Task.Delay(1);
            if (!IsUserInteractionEnabled || _isPanStarted || !CheckInteractionDelay())
            {
                return;
            }
            _lastPanTime = DateTime.UtcNow;

            var oldIndex = SelectedIndex;
            if ((int)swipeDirection < 2)
            {
                //https://github.com/AndreiMisiukevich/CardView/issues/285
                if (!IsUserInteractionRunning && !IsAutoInteractionRunning)
                {
                    SetupBackViews();
                }

                var isLeftSwiped = swipeDirection == ItemSwipeDirection.Left;
                var haveItems = (isLeftSwiped && NextViews.Any()) || (!isLeftSwiped && PrevViews.Any());
                var isAndroid = Device.RuntimePlatform == Device.Android;

                var parentCardsViewOption = new Optional<CardsView>(FindParentElement<CardsView>());
                if (!haveItems && parentCardsViewOption?.Value != null)
                {
                    if (!isAndroid || !IsPanSwipeEnabled)
                    {
                        parentCardsViewOption.Value.OnSwiped(swipeDirection);
                    }
                    return;
                }

                if (IsPanSwipeEnabled && isAndroid && haveItems)
                {
                    return;
                }

                if ((!IsPanSwipeEnabled || !isAndroid) && haveItems)
                {
                    if (IsRightToLeftFlowDirectionEnabled)
                    {
                        isLeftSwiped = !isLeftSwiped;
                    }

                    SetSelectedIndexWithShouldAutoNavigateToNext(isLeftSwiped);
                }
            }

            FireItemSwiped(swipeDirection, oldIndex);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual async void ForceRedrawViews()
        {
            var isHardSet = CheckIsHardSetCurrentView() ||
                //https://github.com/AndreiMisiukevich/CardView/issues/313
                (BackViewsDepth > 1 && (IsAutoInteractionRunning || IsUserInteractionRunning));

            if (isHardSet)
            {
                await (_animationTask = HardSetAsync());
                return;
            }

            if (!_hasRenderer || Parent == null)
            {
                return;
            }

            var tryAutoNavigateTask = TryAutoNavigate();
            _animationTask = tryAutoNavigateTask;

            if (await tryAutoNavigateTask)
            {
                return;
            }

            SetAllViews(false);
        }

        protected virtual void SetAllViews(bool shouldCleanUnprocessingChildren)
        {
            lock (_setCurrentViewLocker)
            {
                var oldAllViews = GetNextPrevCurrentViews();
                if (ItemsSource != null)
                {
                    CurrentView = InitViews(true, AnimationDirection.Current, Enumerable.Empty<View>(), SelectedIndex).FirstOrDefault();

                    if (CurrentView == null && SelectedIndex >= 0)
                    {
                        ShouldIgnoreSetCurrentView = true;
                        SelectedIndex = -1;
                    }
                    else if (SelectedIndex != OldIndex)
                    {
                        var isNextSelected = SelectedIndex > OldIndex;
                        FireItemDisappearing(InteractionType.User, isNextSelected, OldIndex);
                        FireItemAppearing(InteractionType.User, isNextSelected, SelectedIndex);
                        FireItemAppeared(InteractionType.User, isNextSelected, SelectedIndex);
                    }

                    SetupBackViews();
                }

                if (shouldCleanUnprocessingChildren)
                {
                    CleanUnprocessingChildren();
                }
                else
                {
                    Processor?.Clean(this, new ProcessorItem { Views = oldAllViews.Where(view => !CheckIsProcessingView(view)) });
                }
            }
        }

        //https://github.com/AndreiMisiukevich/CardView/issues/286
        protected virtual async Task HardSetAsync()
        {
            DisposeCancellationTokenSource(ref _hardSetTokenSource);
            _hardSetTokenSource = new CancellationTokenSource();
            var token = _hardSetTokenSource.Token;
            ViewExtensions.CancelAnimations(this);

            await Task.Delay(5);
            if (token.IsCancellationRequested)
            {
                return;
            }

            var opacity = Opacity;
            var scale = Scale;
            var time = 150u;

            try
            {
                await Task.WhenAll(
                    this.FadeTo(0, time, Easing.CubicIn),
                    this.ScaleTo(.75, time, Easing.CubicIn));
                token.ThrowIfCancellationRequested();
                SetAllViews(true);
                await Task.Delay(10);
                token.ThrowIfCancellationRequested();
                await Task.WhenAll(
                    this.FadeTo(opacity, time, Easing.CubicOut),
                    this.ScaleTo(scale, time, Easing.CubicOut));
                token.ThrowIfCancellationRequested();
            }
            catch
            {
                Opacity = opacity;
                Scale = scale;
            }
        }

        protected virtual async void OnSizeChanged()
        {
            if (CurrentView != null && ItemTemplate != null)
            {
                var currentViewPair = _viewsPool.FirstOrDefault(p => p.Value.Contains(CurrentView));
                if (!currentViewPair.Equals(default(KeyValuePair<object, List<View>>)))
                {
                    currentViewPair.Value.Clear();
                    currentViewPair.Value.Add(CurrentView);
                    _viewsPool.Clear();
                    _viewsPool.Add(currentViewPair.Key, currentViewPair.Value);
                }
            }
            await Task.Delay(1);// Workaround for https://github.com/AndreiMisiukevich/CardView/issues/194
            ForceRedrawViews();
            RemoveUnprocessingChildren();
            LayoutChildren(X, Y, Width, Height);
            ForceLayout();
        }

        protected virtual void SetupBackViews(int? index = null)
        {
            var realIndex = index ?? SelectedIndex;

            var bookedView = SetupNextView(realIndex, Enumerable.Repeat(CurrentView, 1));
            SetupPrevView(realIndex, bookedView);

            if (IsRightToLeftFlowDirectionEnabled)
            {
                var nextViews = NextViews;
                NextViews = PrevViews;
                PrevViews = nextViews;
            }
        }

        protected virtual void SetupLayout(params View[] views)
        {
            foreach (var view in views.Where(v => v != null))
            {
                SetLayoutBounds(view, new Rectangle(0, 0, 1, 1));
                SetLayoutFlags(view, AbsoluteLayoutFlags.All);
            }
        }

        protected virtual void SetSelectedItem()
        {
            var index = SelectedIndex;
            if (IsCyclical)
            {
                index = index.ToCyclicalIndex(ItemsCount);
            }

            if (!CheckIndexValid(index))
            {
                SelectedItem = null;
                return;
            }

            SelectedItem = this[index];
        }

        protected virtual async void AdjustSlideShow(bool isForceStop = false)
        {
            DisposeCancellationTokenSource(ref _slideShowTokenSource);
            if (isForceStop)
            {
                return;
            }

            if (SlideShowDuration > 0)
            {
                _slideShowTokenSource = new CancellationTokenSource();
                var token = _slideShowTokenSource.Token;
                while (true)
                {
                    await Task.Delay(SlideShowDuration);
                    if (token.IsCancellationRequested)
                    {
                        return;
                    }
                    if (ItemsCount > 0)
                    {
                        SetSelectedIndexWithShouldAutoNavigateToNext(true);
                    }
                }
            }
        }

        protected virtual async Task<bool> TryAutoNavigate()
        {
            if (CurrentView == null || !IsAutoNavigatingAnimationEnabled)
            {
                return false;
            }

            var context = GetContext(SelectedIndex, true);

            if (Equals(GetItem(CurrentView), context))
            {
                return false;
            }

            var animationDirection = GetAutoNavigateAnimationDirection();
            var realDirection = animationDirection;
            if (IsRightToLeftFlowDirectionEnabled)
            {
                realDirection = (AnimationDirection)Sign(-(int)realDirection);
            }

            var oldView = CurrentView;
            SetupBackViews(OldIndex);
            ResetActiveInactiveBackViews(realDirection);
            var newView = InitViews(false, realDirection, Enumerable.Empty<View>(), SelectedIndex).FirstOrDefault();
            SwapViews(realDirection);
            CurrentView = newView;

            var views = CurrentBackViews
                .Union(CurrentInactiveBackViews)
                .Union(Enumerable.Repeat(CurrentView, 1))
                .Where(x => x != null);

            var animationId = Guid.NewGuid();
            StartAutoNavigation(views, animationId, animationDirection);
            PerformUWPFrontViewProcessorHandlePanChanged(Size * Sign((int)realDirection), realDirection);
            await Task.Delay(5);
            _currentBackAnimationDirection = realDirection;

            await (Processor?.Navigate(this, GetAnimationProcessorItems()) ?? Task.FromResult(true));
            PerformUWPFrontViewProcessorHandlePanChanged(0, realDirection);
            EndAutoNavigation(views, animationId, animationDirection);
            return true;
        }

        protected virtual void OnObservableCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ItemsCount = ItemsSource?.Count() ?? -1;

            ShouldSetIndexAfterPan = IsUserInteractionRunning;
            if (!ShouldSetIndexAfterPan)
            {
                SetNewIndex();
            }
        }

        protected virtual bool CheckIsProtectedView(View view) => view.Behaviors.Any(b => b is ProtectedControlBehavior);

        protected virtual bool CheckIsHardSetCurrentView() => false;

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            lock (_sizeChangedLocker)
            {
                var parent = FindParentElement<Page>();
                if (parent == null)
                {
                    return;
                }

                var parentWidth = parent.Width;
                var parentHeight = parent.Height;
                var isValidSize = width > 0 && height > 0;
                var isValidParentSize = parentWidth > 0 && parentHeight > 0;

                if (!_isViewInited && isValidParentSize && isValidSize)
                {
                    _isViewInited = true;
                    StoreParentSize(parentWidth, parentHeight);
                    var prevAnimationDirection = AnimationDirection.Prev;
                    var nextAnimationDirection = AnimationDirection.Next;
                    if (IsRightToLeftFlowDirectionEnabled)
                    {
                        prevAnimationDirection = AnimationDirection.Next;
                        nextAnimationDirection = AnimationDirection.Prev;
                    }
                    Processor?.Init(this,
                        new ProcessorItem { IsFront = true, Views = Enumerable.Repeat(CurrentView, 1) },
                        new ProcessorItem { Views = PrevViews, Direction = prevAnimationDirection },
                        new ProcessorItem { Views = NextViews, Direction = nextAnimationDirection });
                }
                if (_isViewInited &&
                    isValidParentSize &&
                    (Abs(_parentSize.Width - parentWidth) > double.Epsilon ||
                    Abs(_parentSize.Height - parentHeight) > double.Epsilon))
                {
                    StoreParentSize(parentWidth, parentHeight);
                    OnSizeChanged();
                }
            }
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
            switch (propertyName)
            {
                case "Renderer":
                    _hasRenderer = !_hasRenderer;
                    if (_hasRenderer)
                    {
                        ForceRedrawViews();
                        return;
                    }
                    AdjustSlideShow(true);
                    Processor?.Clean(this, new ProcessorItem { Views = GetNextPrevCurrentViews() });
                    return;
            }
        }

        protected override void OnParentSet()
        {
            base.OnParentSet();
            ForceRedrawViews();
        }

        private IEnumerable<View> GetNextPrevCurrentViews()
        => NextViews.Union(PrevViews).Union(Enumerable.Repeat(CurrentView, 1)).Where(view => view != null);

        private IEnumerable<View> SetupNextView(int index, IEnumerable<View> bookedViews)
        {
            var indices = new int[BackViewsDepth];
            for (int i = 0; i < indices.Length; ++i)
            {
                indices[i] = index + 1 + i;
            }

            NextViews = IsNextItemPanInteractionEnabled
                ? InitViews(false, AnimationDirection.Next, bookedViews, indices)
                : Enumerable.Empty<View>();
            return bookedViews.Union(NextViews);
        }

        private IEnumerable<View> SetupPrevView(int index, IEnumerable<View> bookedViews)
        {
            var isForwardOnly = IsOnlyForwardDirection;

            var indices = new int[BackViewsDepth];
            for (int i = 0; i < indices.Length; ++i)
            {
                var incValue = i + 1;
                if (!isForwardOnly)
                {
                    incValue = -incValue;
                }
                indices[i] = index + incValue;
            }

            PrevViews = (IsPrevItemPanInteractionEnabled && !isForwardOnly) || (IsNextItemPanInteractionEnabled && isForwardOnly)
                ? InitViews(false, AnimationDirection.Prev, bookedViews, indices)
                : Enumerable.Empty<View>();
            return bookedViews.Union(PrevViews);
        }

        private void StoreParentSize(double width, double height)
        => _parentSize = new Size(width, height);

        private void SetPanGesture(bool isForceRemoving = false)
        {
            if (Device.RuntimePlatform != Device.Android)
            {
                _panGesture.PanUpdated -= OnPanUpdated;
                GestureRecognizers.Remove(_panGesture);
                if (isForceRemoving)
                {
                    return;
                }
                _panGesture.PanUpdated += OnPanUpdated;
                GestureRecognizers.Add(_panGesture);
            }

            if (Device.RuntimePlatform == Device.GTK)
            {
                var lastTapTime = DateTime.MinValue;
                const int delay = 200;
                CancellationTokenSource tapCts = null;

                GestureRecognizers.Clear();
                GestureRecognizers.Add(new TapGestureRecognizer
                {
                    Command = new Command(async () =>
                    {
                        var now = DateTime.UtcNow;
                        if (Abs((now - lastTapTime).TotalMilliseconds) < delay)
                        {
                            DisposeCancellationTokenSource(ref tapCts);
                            lastTapTime = DateTime.MinValue;
                            SelectedIndex = (SelectedIndex.ToCyclicalIndex(ItemsCount) - 1).ToCyclicalIndex(ItemsCount);
                            return;
                        }
                        lastTapTime = now;
                        tapCts = new CancellationTokenSource();
                        var token = tapCts.Token;
                        await Task.Delay(delay);
                        if (!token.IsCancellationRequested)
                        {
                            SelectedIndex = (SelectedIndex.ToCyclicalIndex(ItemsCount) + 1).ToCyclicalIndex(ItemsCount);
                            return;
                        }
                    })
                });
            }
        }

        private void StartAutoNavigation(IEnumerable<View> views, Guid animationId, AnimationDirection animationDirection)
        {
            if (views != null)
            {
                _interactions.Add(animationId, InteractionType.Auto, InteractionState.Removing);
                IsAutoInteractionRunning = true;
                lock (_viewsInUseLocker)
                {
                    _viewsInUseSet.AddRange(views);
                }
                FireItemDisappearing(InteractionType.Auto, animationDirection != AnimationDirection.Prev, OldIndex);
                FireItemAppearing(InteractionType.Auto, animationDirection != AnimationDirection.Prev, SelectedIndex);
            }
        }

        private void EndAutoNavigation(IEnumerable<View> views, Guid animationId, AnimationDirection animationDirection)
        {
            var isProcessingNow = !_interactions.CheckLastId(animationId);

            if (views != null)
            {
                lock (_viewsInUseLocker)
                {
                    _viewsInUseSet.RemoveRange(views);
                    if (isProcessingNow && BackViewsDepth <= 1)
                    {
                        foreach (var view in views.Where(v => !_viewsInUseSet.Contains(v) && v != CurrentView))
                        {
                            CleanView(view);
                        }
                    }
                }
            }
            IsAutoInteractionRunning = false;
            CompleteInteraction(isProcessingNow, true);
            FireItemAppeared(InteractionType.Auto, animationDirection != AnimationDirection.Prev, SelectedIndex);
            _interactions.Remove(animationId);
        }

        private AnimationDirection GetAutoNavigateAnimationDirection()
        {
            if (!IsCyclical)
            {
                return SelectedIndex < OldIndex && SelectedIndex != -1
                       ? AnimationDirection.Prev
                       : AnimationDirection.Next;
            }

            if (ShouldAutoNavigateToNext.HasValue)
            {
                return ShouldAutoNavigateToNext.GetValueOrDefault()
                    ? AnimationDirection.Next
                    : AnimationDirection.Prev;
            }

            var recIndex = SelectedIndex.ToCyclicalIndex(ItemsCount);
            var oldRecIndex = OldIndex.ToCyclicalIndex(ItemsCount);

            var deltaIndex = recIndex - oldRecIndex;

            var aniamationDirection = (AnimationDirection)Sign(deltaIndex);
            if (aniamationDirection == AnimationDirection.Current)
            {
                aniamationDirection = AnimationDirection.Next;
            }

            var cyclicalDeltaIndex = ItemsCount - Max(recIndex, oldRecIndex) + Min(recIndex, oldRecIndex);

            if (cyclicalDeltaIndex < Abs(deltaIndex))
            {
                aniamationDirection = (AnimationDirection)(-(int)aniamationDirection);
            }

            return aniamationDirection;
        }

        private void OnTouchStarted()
        {
            if (_isPanStarted)
            {
                return;
            }

            if (!CheckInteractionDelay())
            {
                return;
            }
            _isOppositePanDirectionIssueResolved = false;

            var gestureId = Guid.NewGuid();
            _interactions.Add(gestureId, InteractionType.User);

            FireUserInteracted(UserInteractionStatus.Started, CurrentDiff, SelectedIndex);
            if (Device.RuntimePlatform != Device.Android)
            {
                IsUserInteractionRunning = true;
            }
            _isPanStarted = true;

            SetupBackViews();
            AddRangeViewsInUse(gestureId);

            _timeDiffItems.Add(new TimeDiffItem
            {
                Time = DateTime.UtcNow,
                Diff = 0
            });
        }

        private void OnTouchChanged(double diff, double oppositeDirectionDiff, bool isTouchCompleted = false)
        {
            if (!_isPanStarted || Abs(CurrentDiff - diff) <= double.Epsilon)
            {
                return;
            }

            var absDiff = Abs(diff);
            var absOppositeDirectionDiff = Abs(oppositeDirectionDiff);
            if (!_isOppositePanDirectionIssueResolved && Max(absDiff, absOppositeDirectionDiff) > OppositePanDirectionDisablingThreshold)
            {
                absOppositeDirectionDiff *= 2.5;
                if (absOppositeDirectionDiff >= absDiff)
                {
                    diff = 0;
                    _isPanStarted = false;
                }
                _isOppositePanDirectionIssueResolved = true;
            }

            var interactionItem = _interactions.GetFirstItem(InteractionType.User, InteractionState.Regular);
            if (interactionItem == null)
            {
                return;
            }

            interactionItem.IsInvolved = true;
            ResetActiveInactiveBackViews(diff);
            var prevDiff = CurrentDiff;
            CurrentDiff = diff;
            SetupDiffItems(diff);

            if (isTouchCompleted)
            {
                diff = prevDiff;
            }

            FireUserInteracted(UserInteractionStatus.Running, diff, SelectedIndex);
            try
            {
                BatchBegin();
                Processor?.Change(this, diff, GetAnimationProcessorItems());
            }
            finally
            {
                BatchCommit();
            }
        }

        private async void OnTouchEnded()
        {
            if (!_isPanStarted)
            {
                return;
            }

            var interactionItem = _interactions.GetFirstItem(InteractionType.User, InteractionState.Regular);
            if (interactionItem == null)
            {
                return;
            }
            _lastPanTime = DateTime.UtcNow;
            interactionItem.State = InteractionState.Removing;
            if (interactionItem.Id == Guid.Empty)
            {
                return;
            }

            var gestureId = interactionItem.Id;

            _isPanStarted = false;
            var absDiff = Abs(CurrentDiff);

            var oldIndex = SelectedIndex;
            var index = oldIndex;
            var diff = CurrentDiff;

            CleanDiffItems();

            bool? isNextSelected = null;

            if (IsEnabled && _currentBackAnimationDirection != AnimationDirection.Null)
            {
                if (absDiff > RealMoveDistance || CheckPanSwipe())
                {
                    isNextSelected = diff < 0;
                    FireItemSwiped(isNextSelected.Value ? ItemSwipeDirection.Left : ItemSwipeDirection.Right, oldIndex);
                }
            }

            _timeDiffItems.Clear();

            Task endingTask;
            if (isNextSelected.HasValue)
            {
                index = GetNewIndexFromDiff();
                if (index < 0)
                {
                    return;
                }

                SwapViews(isNextSelected.GetValueOrDefault());
                ShouldIgnoreSetCurrentView = true;

                SelectedIndex = index;

                endingTask = Processor?.Proceed(this, GetAnimationProcessorItems()) ?? Task.FromResult(true);
            }
            else
            {
                endingTask = interactionItem.IsInvolved
                    ? Processor?.Reset(this, GetAnimationProcessorItems()) ?? Task.FromResult(true)
                    : Task.FromResult(true);
            }

            FireUserInteracted(UserInteractionStatus.Ending, diff, oldIndex);
            if (isNextSelected.HasValue)
            {
                FireItemDisappearing(InteractionType.User, isNextSelected.GetValueOrDefault(), oldIndex);
                FireItemAppearing(InteractionType.User, isNextSelected.GetValueOrDefault(), index);
            }

            CurrentDiff = 0;
            await (_animationTask = endingTask);

            FireUserInteracted(UserInteractionStatus.Ended, diff, oldIndex);
            if (isNextSelected.HasValue)
            {
                FireItemAppeared(InteractionType.User, isNextSelected.GetValueOrDefault(), index);
            }

            var isProcessingNow = !_interactions.CheckLastId(gestureId);
            RemoveRangeViewsInUse(gestureId, isProcessingNow);

            if (!isProcessingNow)
            {
                IsUserInteractionRunning = false;
                if (ShouldSetIndexAfterPan)
                {
                    ShouldSetIndexAfterPan = false;
                    SetNewIndex();
                }
            }

            CompleteInteraction(isProcessingNow, isNextSelected.HasValue);

            _interactions.Remove(gestureId);
        }

        private bool ShouldParentHandleTouch(GestureStatus satusType)
        {
            var result = _parentCardsViewTouchHandlerOption?.Value != null;
            switch(satusType)
            {
                case GestureStatus.Canceled:
                case GestureStatus.Completed:
                    ClearParentCardsViewOption();
                    break;
            }
            return result;
        }

        private void SetParentTouchHandlerIfNeeded(GestureStatus statusType)
        {
            if (CurrentDiff == 0 || _parentCardsViewTouchHandlerOption != null)
            {
                return;
            }

            _parentCardsViewTouchHandlerOption = CurrentBackViews.Any()
                ? new Optional<CardsView>(null)
                : _parentCardsViewOption;

            if (_parentCardsViewTouchHandlerOption.Value == null)
            {
                return;
            }

            _parentCardsViewOption.Value.IsPanControllingByChild = false;

            if (statusType == GestureStatus.Running)
            {
                OnTouchChanged(0, 0, true);
            }

            switch(statusType)
            {
                case GestureStatus.Started:
                case GestureStatus.Running:
                    OnTouchEnded();
                    break;
            }
        }

        private void SetParentCardsViewOption()
        {
            if (_parentCardsViewOption != null)
            {
                return;
            }

            _parentCardsViewOption = new Optional<CardsView>(FindParentElement<CardsView>());

            if (_parentCardsViewOption.Value == null)
            {
                return;
            }

            _parentCardsViewOption.Value.IsPanControllingByChild = true;
        }

        private void ClearParentCardsViewOption()
        {
            if (_parentCardsViewOption?.Value != null)
            {
                _parentCardsViewOption.Value.IsPanControllingByChild = false;
            }

            IsPanControllingByChild = false;
            _parentCardsViewTouchHandlerOption = null;
            _parentCardsViewOption = null;
        }

        private bool CheckInteractionDelay()
            => CurrentView != null &&
            IsUserInteractionEnabled &&
            Abs((DateTime.UtcNow - _lastPanTime).TotalMilliseconds) >= UserInteractionDelay &&
            (!IsUserInteractionInCourse || (_animationTask?.IsCompleted ?? true));

        private bool CheckPanSwipe()
        {
            if (!IsPanSwipeEnabled || _timeDiffItems.Count < 2)
            {
                return false;
            }

            var lastItem = _timeDiffItems.LastOrDefault();
            var firstItem = _timeDiffItems.FirstOrDefault();

            var distDiff = lastItem.Diff - firstItem.Diff;

            if (Sign(distDiff) != Sign(lastItem.Diff))
            {
                return false;
            }

            var absDistDiff = Abs(distDiff);
            var timeDiff = lastItem.Time - firstItem.Time;

            var acceptValue = SwipeThresholdDistance * timeDiff.TotalMilliseconds / SwipeThresholdTime.TotalMilliseconds;

            return absDistDiff >= acceptValue;
        }

        private void SetupDiffItems(double diff)
        {
            var timeNow = DateTime.UtcNow;

            if (_timeDiffItems.Count >= 25)
            {
                CleanDiffItems();
            }

            _timeDiffItems.Add(new TimeDiffItem
            {
                Time = timeNow,
                Diff = diff
            });
        }

        private void CleanDiffItems()
        {
            var time = _timeDiffItems.LastOrDefault().Time;

            for (var i = _timeDiffItems.Count - 1; i >= 0; --i)
            {
                if (time - _timeDiffItems[i].Time > SwipeThresholdTime)
                {
                    _timeDiffItems.RemoveAt(i);
                }
            }
        }

        private int GetNewIndexFromDiff()
        {
            var indexDelta = -Sign(CurrentDiff) * (IsRightToLeftFlowDirectionEnabled ? -1 : 1);
            if (IsOnlyForwardDirection)
            {
                indexDelta = Abs(indexDelta);
            }
            var newIndex = SelectedIndex + indexDelta;

            if (!CheckIndexValid(newIndex))
            {
                if (!IsCyclical)
                {
                    return -1;
                }

                newIndex = newIndex.ToCyclicalIndex(ItemsCount);
            }

            return newIndex;
        }

        private void SetSelectedIndexWithShouldAutoNavigateToNext(bool isNext)
        {
            try
            {
                ShouldAutoNavigateToNext = isNext;
                SelectedIndex = (SelectedIndex + (isNext ? 1 : -1)).ToCyclicalIndex(ItemsCount);
            }
            finally
            {
                ShouldAutoNavigateToNext = null;
            }
        }

        private void ResetActiveInactiveBackViews(AnimationDirection animationDirection)
        {
            var activeViews = NextViews;
            var inactiveViews = PrevViews;

            if (animationDirection == AnimationDirection.Prev)
            {
                activeViews = PrevViews;
                inactiveViews = NextViews;
            }

            CurrentBackViews = activeViews;
            _currentBackAnimationDirection = CurrentBackViews.Any()
                    ? animationDirection
                    : AnimationDirection.Null;


            CurrentInactiveBackViews = !inactiveViews.SequenceEqual(activeViews)
                ? inactiveViews
                : null;
        }

        private void ResetActiveInactiveBackViews(double diff)
        => ResetActiveInactiveBackViews(diff > 0 ? AnimationDirection.Prev : AnimationDirection.Next);

        private void SwapViews(bool isNext)
        {
            var view = CurrentView;
            CurrentView = CurrentBackViews.FirstOrDefault();
            CurrentBackViews = Enumerable.Repeat(view, 1).Union(CurrentBackViews.Except(Enumerable.Repeat(CurrentView, 1)));

            if (isNext)
            {
                NextViews = PrevViews;
                PrevViews = CurrentBackViews;
                return;
            }
            PrevViews = NextViews;
            NextViews = CurrentBackViews;
        }

        private void SwapViews(AnimationDirection animationDirection)
        => SwapViews(animationDirection == AnimationDirection.Next);

        private IEnumerable<View> InitViews(bool isFront, AnimationDirection animationDirection, IEnumerable<View> bookedViews, params int[] indices)
        {
            var views = new View[indices.Length];

            try
            {
                BatchBegin();
                for (int i = 0; i < indices.Length; ++i)
                {
                    var view = PrepareView(bookedViews, indices[i]);
                    views[i] = view;
                    if (view != null)
                    {
                        bookedViews = bookedViews.Union(Enumerable.Repeat(view, 1));
                    }
                }
            }
            finally
            {
                BatchCommit();
            }

            if (views.All(x => x == null))
            {
                return Enumerable.Empty<View>();
            }

            Processor?.Init(this, new ProcessorItem { IsFront = isFront, Views = views, Direction = animationDirection });

            SetupLayout(views);

            AddChildren(animationDirection != AnimationDirection.Current
                ? CurrentView
                : null, views);

            return views;
        }

        private View PrepareView(IEnumerable<View> bookedViews, int index)
        {
            var context = GetContext(index, !bookedViews.Any());

            if (context == null)
            {
                return null;
            }

            var template = ItemTemplate;
            while (template is DataTemplateSelector selector)
            {
                template = selector.SelectTemplate(context, this);
            }

            var view = template != null
                ? CreateRetrieveView(context, template, bookedViews)
                : context as View;

            if (view != null && view != context)
            {
                if (view.BindingContext != context)
                {
                    view.BindingContext = null;
                    view.BindingContext = context;
                }
                view.Behaviors.Remove(_contextAssignedBehavior);
                view.Behaviors.Add(_contextAssignedBehavior);
            }

            return view;
        }

        private View CreateRetrieveView(object context, DataTemplate template, IEnumerable<View> bookedViews)
        {
            if (!_viewsPool.TryGetValue(template, out HashSet<View> viewsCollection))
            {
                viewsCollection = new HashSet<View>();
                _viewsPool.Add(template, viewsCollection);
            }

            var notUsingViews = viewsCollection.Where(v => !_viewsInUseSet.Contains(v) && !bookedViews.Contains(v));
            var view = notUsingViews.FirstOrDefault(v => Equals(v.BindingContext, context) || v == context);
            if (IsViewReusingEnabled)
            {
                view = view ?? notUsingViews.FirstOrDefault(v => v.BindingContext == null)
                            ?? notUsingViews.FirstOrDefault(v => !CheckIsProcessingView(v));
            }

            if (ShouldShareViewAmongSameItems)
            {
                view = bookedViews.FirstOrDefault(v => Equals(v.BindingContext, context)) ?? view;
            }

            if (view == null)
            {
                view = template.CreateView();
                viewsCollection.Add(view);
            }

            // https://github.com/AndreiMisiukevich/CardView/issues/282
            if (IsCyclical && BackViewsDepth > 1 && ItemsCount <= BackViewsDepth * 2)
            {
                var duplicatedViews = notUsingViews
                    .Except(Enumerable.Repeat(view, 1))
                    .Where(v => Equals(GetItem(v), GetItem(view)));
                Processor?.Clean(this, new ProcessorItem { Views = duplicatedViews });
            }

            return view;
        }

        private object GetContext(int index, bool isCurrent)
        {
            if (ItemsCount <= 0)
            {
                return null;
            }

            if (!CheckIndexValid(index))
            {
                if (!IsCyclical || (!isCurrent && ItemsCount <= 1))
                {
                    return null;
                }

                index = index.ToCyclicalIndex(ItemsCount);
            }

            if (index < 0 || ItemsSource == null)
            {
                return null;
            }

            return this[index];
        }

        private void RemoveRangeViewsPool(View[] views)
        {
            foreach (var view in views)
            {
                foreach (var viewsCollection in _viewsPool.Values)
                {
                    viewsCollection.Remove(view);
                }
            }
        }

        private bool CheckContextAssigned(View view)
        => view?.Behaviors.Contains(_contextAssignedBehavior) ?? false;

        private object GetItem(View view)
        => CheckContextAssigned(view)
            ? view.BindingContext
            : view;

        private object GetItem(int index)
        {
            if (IsCyclical)
            {
                index = index.ToCyclicalIndex(ItemsCount);
            }
            return index >= 0 && index < ItemsCount
                ? this[index]
                    : null;
        }

        private void SendChildToBackIfNeeded(View view, View topView)
        {
            if (view == null || topView == null)
            {
                return;
            }

            var currentIndex = Children.IndexOf(topView);
            var backIndex = Children.IndexOf(view);

            if (currentIndex < backIndex)
            {
                ExecutePreventException(() => LowerChild(view));
            }
        }

        private void CleanUnprocessingChildren()
        {
            InvokeOnMainThreadIfNeeded(() =>
            {
                lock (_childrenLocker)
                {
                    var views = Children.Where(c => !CheckIsProtectedView(c) && !CheckIsProcessingView(c)).ToArray();
                    foreach (var view in views)
                    {
                        CleanView(view);
                    }
                }
            });
        }

        private void CleanView(View view)
        {
            if (CheckContextAssigned(view) && IsViewReusingEnabled)
            {
                view.BindingContext = null;
            }
            view.Behaviors.Remove(_contextAssignedBehavior);
            Processor?.Clean(this, new ProcessorItem { Views = Enumerable.Repeat(view, 1) });
        }

        private void SetItemsSource(IEnumerable oldCollection)
        {
            if (oldCollection is INotifyCollectionChanged oldObservableCollection)
            {
                oldObservableCollection.CollectionChanged -= OnObservableCollectionChanged;
            }

            if (ItemsSource is INotifyCollectionChanged observableCollection)
            {
                observableCollection.CollectionChanged += OnObservableCollectionChanged;
            }

            OnObservableCollectionChanged(oldCollection, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        private void SetNewIndex()
        {
            if (ItemsCount <= 0)
            {
                SelectedIndex = -1;
                return;
            }

            var index = -1;

            if (CurrentView != null)
            {
                var currentItem = GetItem(CurrentView);
                for (var i = 0; i < ItemsCount; ++i)
                {
                    if (Equals(this[i], currentItem))
                    {
                        index = i;
                        break;
                    }
                }
            }

            var isCurrentContextPresented = index >= 0;
            if (!isCurrentContextPresented)
            {
                index = SelectedIndex < 0
                    ? 0
                    : SelectedIndex >= ItemsCount
                        ? ItemsCount - 1
                        : SelectedIndex;
            }

            if (SelectedIndex == index)
            {
                if (!isCurrentContextPresented || BackViewsDepth > 1)
                {
                    OldIndex = index;
                    ForceRedrawViews();
                }
                SetSelectedItem();
                return;
            }

            SelectedIndex = index;
        }

        private void CompleteInteraction(bool isProcessingNow, bool isNewItemSelected)
        {
            if (!isProcessingNow && isNewItemSelected && BackViewsDepth > 1)
            {
                SetupBackViews();
            }
            RemoveRedundantChildren(isProcessingNow);
        }

        private void AddChildren(View topView = null, params View[] views)
        {
            InvokeOnMainThreadIfNeeded(() =>
            {
                lock (_childrenLocker)
                {
                    foreach (var view in views)
                    {
                        if (view == null)
                        {
                            continue;
                        }

                        if (Children.Contains(view))
                        {
                            SendChildToBackIfNeeded(view, topView);
                            continue;
                        }

                        ++_viewsChildrenCount;
                        var index = topView != null
                            ? Children.IndexOf(topView)
                            : 0;
                        
                        ExecutePreventException(() =>
                        {
                            Children.Insert(index, view);
                            AccessibilityChangeRequested?.Invoke(view, view == CurrentView);
                        });
                    }
                }
            });
        }

        private void RemoveRedundantChildren(bool isProcessingNow)
        {
            var maxChildrenCount = isProcessingNow
                ? MaxChildrenCount
                : DesiredMaxChildrenCount;

            InvokeOnMainThreadIfNeeded(() =>
            {
                lock (_childrenLocker)
                {
                    if (_viewsChildrenCount <= maxChildrenCount)
                    {
                        return;
                    }

                    var views = Children.Where(c => !CheckIsProtectedView(c) && !CheckIsProcessingView(c) && !_viewsInUseSet.Contains(c));
                    if (IsViewReusingEnabled)
                    {
                        views = views.Take(_viewsChildrenCount - DesiredMaxChildrenCount);
                    }
                    RemoveChildren(views.ToArray());
                }
            });
        }

        private void RemoveUnprocessingChildren()
        {
            InvokeOnMainThreadIfNeeded(() =>
            {
                lock (_childrenLocker)
                {
                    var views = Children.Where(c => !CheckIsProtectedView(c) && !CheckIsProcessingView(c)).ToArray();
                    RemoveChildren(views);
                }
            });
        }

        private void RemoveChildren(View[] views)
        {
            _viewsChildrenCount -= views.Length;
            foreach (var view in views)
            {
                ExecutePreventException(() => Children.Remove(view));
                CleanView(view);
            }

            if (!IsViewReusingEnabled)
            {
                RemoveRangeViewsPool(views);
            }
        }

        private ProcessorItem[] GetAnimationProcessorItems()
            => new ProcessorItem[]
            {
                new ProcessorItem
                {
                    IsFront = true,
                    Views = Enumerable.Repeat(CurrentView, 1),
                    Direction = _currentBackAnimationDirection,
                    InactiveViews = Enumerable.Empty<View>()
                },
                new ProcessorItem
                {
                    Views = CurrentBackViews,
                    Direction = _currentBackAnimationDirection,
                    InactiveViews = CurrentInactiveBackViews
                }
            };

        private void InvokeOnMainThreadIfNeeded(Action action)
        {
            if (!Device.IsInvokeRequired)
            {
                action.Invoke();
                return;
            }
            Device.BeginInvokeOnMainThread(action);
        }

        private void ExecutePreventException(Action action)
        {
            try
            {
                action?.Invoke();
            }
            catch
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    try
                    {
                        action?.Invoke();
                    }
                    catch
                    {
#if NETSTANDARD2_0
                        Console.WriteLine("CardsView: Unhandled Exception occurred during the interaction with Children collection.");
#endif
                    }
                });
            }
        }

        //https://github.com/AndreiMisiukevich/CardView/issues/335
        private void PerformUWPFrontViewProcessorHandlePanChanged(double value, AnimationDirection direction)
        {
            if (Device.RuntimePlatform != Device.UWP)
            {
                return;
            }
            Processor?.Change(this, value,
                new ProcessorItem
                {
                    IsFront = true,
                    Views = Enumerable.Repeat(CurrentView, 1),
                    Direction = direction,
                    InactiveViews = Enumerable.Empty<View>()
                });
        }

        private TElement FindParentElement<TElement>() where TElement : VisualElement
        {
            var parent = Parent;
            while (parent != null && !(parent is TElement))
            {
                parent = parent.Parent;
            }
            return parent as TElement;
        }

        private bool CheckIsProcessingView(View view)
        => view == CurrentView || NextViews.Contains(view) || PrevViews.Contains(view);

        private bool CheckIndexValid(int index)
        => index >= 0 && index < ItemsCount;

        private void AddRangeViewsInUse(Guid gestureId)
        {
            lock (_viewsInUseLocker)
            {
                var views = GetNextPrevCurrentViews();
                _viewsGestureCounter[gestureId] = views;
                _viewsInUseSet.AddRange(views);
            }
        }

        private void RemoveRangeViewsInUse(Guid gestureId, bool isProcessingNow)
        {
            lock (_viewsInUseLocker)
            {
                if (!_viewsGestureCounter.ContainsKey(gestureId))
                {
                    return;
                }

                var views = _viewsGestureCounter[gestureId];
                _viewsInUseSet.RemoveRange(views);
                if (isProcessingNow)
                {
                    foreach (var view in views.Where(v => v != null && !_viewsInUseSet.Contains(v)))
                    {
                        CleanView(view);
                    }
                }
                _viewsGestureCounter.Remove(gestureId);
            }
        }

        private void DisposeCancellationTokenSource(ref CancellationTokenSource tokenSource)
        {
            tokenSource?.Cancel();
            tokenSource?.Dispose();
            tokenSource = null;
        }

        private void FireUserInteracted(UserInteractionStatus status, double diff, int index)
        {
            var item = GetItem(index);
            var args = new UserInteractedEventArgs(status, diff, index, item);
            UserInteractedCommand?.Execute(args);
            UserInteracted?.Invoke(this, args);
        }

        private void FireItemDisappearing(InteractionType type, bool isNextSelected, int index)
        {
            var item = GetItem(index);
            var args = new ItemDisappearingEventArgs(type, isNextSelected, index, item);
            ItemDisappearingCommand?.Execute(args);
            ItemDisappearing?.Invoke(this, args);
        }

        private void FireItemAppearing(InteractionType type, bool isNextSelected, int index)
        {
            var item = GetItem(index);
            var args = new ItemAppearingEventArgs(type, isNextSelected, index, item);
            ItemAppearingCommand?.Execute(args);
            ItemAppearing?.Invoke(this, args);
        }

        private void FireItemAppeared(InteractionType type, bool isNextSelected, int index)
        {
            var item = GetItem(index);
            var args = new ItemAppearedEventArgs(type, isNextSelected, index, item);
            ItemAppearedCommand?.Execute(args);
            ItemAppeared?.Invoke(this, args);
        }

        private void FireItemSwiped(ItemSwipeDirection swipeDirection, int index)
        {
            var item = GetItem(index);
            var args = new ItemSwipedEventArgs(swipeDirection, index, item);
            ItemSwipedCommand?.Execute(args);
            ItemSwiped?.Invoke(this, args);
        }
    }
}
