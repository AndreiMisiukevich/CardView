using PanCardView.Behaviors;
using PanCardView.Controls;
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
            view.SetCurrentView();
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
            bindable.AsCardsView().SetCurrentView();
        });

        public static readonly BindableProperty BackViewsDepthProperty = BindableProperty.Create(nameof(BackViewsDepth), typeof(int), typeof(CardsView), defaultValueCreator: b => b.AsCardsView().DefaultBackViewsDepth, propertyChanged: (bindable, oldValue, newValue) =>
        {
            bindable.AsCardsView().SetCurrentView();
        });

        public static readonly BindableProperty IsRightToLeftFlowDirectionEnabledProperty = BindableProperty.Create(nameof(IsRightToLeftFlowDirectionEnabled), typeof(bool), typeof(CardsView), false, propertyChanged: (bindable, oldValue, newValue) =>
        {
            bindable.AsCardsView().SetCurrentView();
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

        public static readonly BindableProperty ItemsCountProperty = BindableProperty.Create(nameof(ItemsCount), typeof(int), typeof(CardsView), -1, BindingMode.OneWayToSource);

        public static readonly BindableProperty IsUserInteractionEnabledProperty = BindableProperty.Create(nameof(IsUserInteractionEnabled), typeof(bool), typeof(CardsView), true);

        public static readonly BindableProperty MoveDistanceProperty = BindableProperty.Create(nameof(MoveDistance), typeof(double), typeof(CardsView), -1.0);

        public static readonly BindableProperty MoveWidthPercentageProperty = BindableProperty.Create(nameof(MoveWidthPercentage), typeof(double), typeof(CardsView), defaultValueCreator: b => b.AsCardsView().DefaultMoveWidthPercentage);

        public static readonly BindableProperty IsOnlyForwardDirectionProperty = BindableProperty.Create(nameof(IsOnlyForwardDirection), typeof(bool), typeof(CardsView), false);

        public static readonly BindableProperty IsViewCacheEnabledProperty = BindableProperty.Create(nameof(IsViewCacheEnabled), typeof(bool), typeof(CardsView), true);

        public static readonly BindableProperty UserInteractionDelayProperty = BindableProperty.Create(nameof(UserInteractionDelay), typeof(int), typeof(CardsView), 200);

        public static readonly BindableProperty IsUserInteractionInCourseProperty = BindableProperty.Create(nameof(IsUserInteractionInCourse), typeof(bool), typeof(CardsView), true);

        public static readonly BindableProperty IsCyclicalProperty = BindableProperty.Create(nameof(IsCyclical), typeof(bool), typeof(CardsView), defaultValueCreator: b => b.AsCardsView().DefaultIsCyclical);

        public static readonly BindableProperty IsAutoNavigatingAimationEnabledProperty = BindableProperty.Create(nameof(IsAutoNavigatingAimationEnabled), typeof(bool), typeof(CardsView), true);

        public static readonly BindableProperty IsPanSwipeEnabledProperty = BindableProperty.Create(nameof(IsPanSwipeEnabled), typeof(bool), typeof(CardsView), true);

        public static readonly BindableProperty AreAnimationsEnabledProperty = BindableProperty.Create(nameof(AreAnimationsEnabled), typeof(bool), typeof(CardsView), true);

        public static readonly BindableProperty MaxChildrenCountProperty = BindableProperty.Create(nameof(MaxChildrenCount), typeof(int), typeof(CardsView), defaultValueCreator: b => b.AsCardsView().DefaultMaxChildrenCount);

        public static readonly BindableProperty DesiredMaxChildrenCountProperty = BindableProperty.Create(nameof(DesiredMaxChildrenCount), typeof(int), typeof(CardsView), defaultValueCreator: b => b.AsCardsView().DefaultDesiredMaxChildrenCount);

        public static readonly BindableProperty SwipeThresholdDistanceProperty = BindableProperty.Create(nameof(SwipeThresholdDistance), typeof(double), typeof(CardsView), 17.0);

        public static readonly BindableProperty MoveThresholdDistanceProperty = BindableProperty.Create(nameof(MoveThresholdDistance), typeof(double), typeof(CardsView), 7.0);

        public static readonly BindableProperty VerticalSwipeThresholdDistanceProperty = BindableProperty.Create(nameof(VerticalSwipeThresholdDistance), typeof(double), typeof(CardsView), 30.0);

        public static readonly BindableProperty SwipeThresholdTimeProperty = BindableProperty.Create(nameof(SwipeThresholdTime), typeof(TimeSpan), typeof(CardsView), TimeSpan.FromMilliseconds(Device.RuntimePlatform == Device.Android ? 100 : 60));

        public static readonly BindableProperty UserInteractedCommandProperty = BindableProperty.Create(nameof(UserInteractedCommand), typeof(ICommand), typeof(CardsView), null);

        public static readonly BindableProperty ItemDisappearingCommandProperty = BindableProperty.Create(nameof(ItemDisappearingCommand), typeof(ICommand), typeof(CardsView), null);

        public static readonly BindableProperty ItemAppearingCommandProperty = BindableProperty.Create(nameof(ItemAppearingCommand), typeof(ICommand), typeof(CardsView), null);

        public static readonly BindableProperty ItemSwipedCommandProperty = BindableProperty.Create(nameof(ItemSwipedCommand), typeof(ICommand), typeof(CardsView), null);

        public event CardsViewUserInteractedHandler UserInteracted;
        public event CardsViewItemDisappearingHandler ItemDisappearing;
        public event CardsViewItemAppearingHandler ItemAppearing;
        public event CardsViewItemSwipedHandler ItemSwiped;

        private readonly object _childLocker = new object();
        private readonly object _viewsInUseLocker = new object();
        private readonly object _setCurrentViewLocker = new object();
        private readonly object _sizeChangedLocker = new object();

        private readonly Dictionary<object, List<View>> _viewsPool = new Dictionary<object, List<View>>();
        private readonly Dictionary<Guid, IEnumerable<View>> _viewsGestureCounter = new Dictionary<Guid, IEnumerable<View>>();
        private readonly List<TimeDiffItem> _timeDiffItems = new List<TimeDiffItem>();
        private readonly ViewsInUseSet _viewsInUse = new ViewsInUseSet();
        private readonly InteractionQueue _interactions = new InteractionQueue();
        private readonly PanGestureRecognizer _panGesture = new PanGestureRecognizer();
        private readonly ContextAssignedBehavior _contextAssignedBehavior = new ContextAssignedBehavior();

        private IEnumerable<View> _prevViews = Enumerable.Empty<View>();
        private IEnumerable<View> _nextViews = Enumerable.Empty<View>();
        private IEnumerable<View> _currentBackViews = Enumerable.Empty<View>();
        private IEnumerable<View> _currentInactiveBackViews = Enumerable.Empty<View>();

        private AnimationDirection _currentBackAnimationDirection;

        private int _viewsChildrenCount;
        private int _inCoursePanDelay;
        private bool _isPanEndRequested = true;
        private bool _shouldSkipTouch;
        private bool _isViewsInited;
        private bool _hasRenderer;
        private bool? _shouldScrollParent;
        private Size _parentSize;
        private DateTime _lastPanTime;
        private CancellationTokenSource _slideshowTokenSource;

        public CardsView() : this(new BaseCardFrontViewProcessor(), new BaseCardBackViewProcessor())
        {
        }

        public CardsView(ICardProcessor frontViewProcessor, ICardBackViewProcessor backViewProcessor)
        {
            FrontViewProcessor = frontViewProcessor;
            BackViewProcessor = backViewProcessor;
            SetPanGesture();
        }

        private bool ShouldIgnoreSetCurrentView { get; set; }

        private bool ShouldSetIndexAfterPan { get; set; }

        protected virtual int DefaultBackViewsDepth => 1;

        protected virtual double DefaultMoveWidthPercentage => 0.325;

        protected virtual bool DefaultIsCyclical => false;

        protected virtual int DefaultMaxChildrenCount => 12;

        protected virtual int DefaultDesiredMaxChildrenCount => 7;

        public View CurrentView { get; private set; }

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

        public double CurrentDiff { get; private set; }

        public int OldIndex { get; private set; } = -1;

        public ICardProcessor FrontViewProcessor { get; }

        public ICardBackViewProcessor BackViewProcessor { get; }

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

        /// <summary>
        /// Only for iOS and Android
        /// </summary>
        /// <value>Pan interaction is enabled</value>
        public bool IsPanInteractionEnabled
        {
            get => (bool)GetValue(IsPanInteractionEnabledProperty);
            set => SetValue(IsPanInteractionEnabledProperty, value);
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
            get
            {
                var dist = (double)GetValue(MoveDistanceProperty);
                return dist > 0
                        ? dist
                        : Width * MoveWidthPercentage;
            }
            set => SetValue(MoveDistanceProperty, value);
        }

        public double MoveWidthPercentage
        {
            get => (double)GetValue(MoveWidthPercentageProperty);
            set => SetValue(MoveWidthPercentageProperty, value);
        }

        public bool IsOnlyForwardDirection
        {
            get => (bool)GetValue(IsOnlyForwardDirectionProperty);
            set => SetValue(IsOnlyForwardDirectionProperty, value);
        }

        public bool IsViewCacheEnabled
        {
            get => (bool)GetValue(IsViewCacheEnabledProperty);
            set => SetValue(IsViewCacheEnabledProperty, value);
        }

        public int UserInteractionDelay
        {
            get => IsUserInteractionInCourse ? _inCoursePanDelay : (int)GetValue(UserInteractionDelayProperty);
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

        public bool IsAutoNavigatingAimationEnabled
        {
            get => (bool)GetValue(IsAutoNavigatingAimationEnabledProperty);
            set => SetValue(IsAutoNavigatingAimationEnabledProperty, value);
        }

        public bool AreAnimationsEnabled
        {
            get => (bool)GetValue(AreAnimationsEnabledProperty);
            set => SetValue(AreAnimationsEnabledProperty, value);
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
        /// Only for Android
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
        /// <value>Move threshold distance.</value>
        public double VerticalSwipeThresholdDistance
        {
            get => (double)GetValue(VerticalSwipeThresholdDistanceProperty);
            set => SetValue(VerticalSwipeThresholdDistanceProperty, value);
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

        public object this[int index] => ItemsSource?.FindValue(index);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void Preserve()
        {
        }

        public void OnPanUpdated(object sender, PanUpdatedEventArgs e)
        => OnPanUpdated(e);

        public void OnPanUpdated(PanUpdatedEventArgs e)
        {
            if (ItemsCount <= 0)
            {
                return;
            }

            switch (e.StatusType)
            {
                case GestureStatus.Started:
                    OnTouchStarted();
                    return;
                case GestureStatus.Running:
                    HandleParentScroll(e);
                    OnTouchChanged(e.TotalX);
                    return;
                case GestureStatus.Canceled:
                case GestureStatus.Completed:
                    if (Device.RuntimePlatform == Device.Android)
                    {
                        OnTouchChanged(e.TotalX);
                    }
                    OnTouchEnded();
                    return;
            }
        }

        public void OnSwiped(ItemSwipeDirection swipeDirection)
        {
            if (!IsUserInteractionEnabled || !_isPanEndRequested || !CheckInteractionDelay())
            {
                return;
            }
            _lastPanTime = DateTime.UtcNow;

            var oldIndex = SelectedIndex;
            if ((int)swipeDirection < 2)
            {
                var isLeftSwiped = swipeDirection == ItemSwipeDirection.Left;
                var haveItems = (isLeftSwiped && NextViews.Any()) || (!isLeftSwiped && PrevViews.Any());
                var isAndroid = Device.RuntimePlatform == Device.Android;

                if (IsPanSwipeEnabled && haveItems && isAndroid)
                {
                    return;
                }

                if ((!IsPanSwipeEnabled || !isAndroid) && haveItems)
                {
                    if (IsRightToLeftFlowDirectionEnabled)
                    {
                        isLeftSwiped = !isLeftSwiped;
                    }
                    SelectedIndex = (SelectedIndex + (isLeftSwiped ? 1 : -1)).ToCyclingIndex(ItemsCount);
                }
            }

            FireItemSwiped(swipeDirection, oldIndex);
        }

        protected internal virtual async void SetCurrentView()
        {
            var isHardSet = CheckIsHardSetCurrentView();

            if (!isHardSet && (!_hasRenderer || Parent == null || await TryAutoNavigate()))
            {
                return;
            }

            lock (_setCurrentViewLocker)
            {
                if (ItemsSource != null)
                {
                    var oldView = CurrentView;
                    CurrentView = InitViews(FrontViewProcessor, AnimationDirection.Current, Enumerable.Empty<View>(), SelectedIndex).FirstOrDefault();
                    var newView = CurrentView;

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
                    }

                    SetupBackViews();
                }
            }

            if (isHardSet)
            {
                CleanUnprocessingChildren();
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
            SetCurrentView();
            RemoveUnprocessingChildren();
            LayoutChildren(X, Y, Width, Height);
            ForceLayout();
        }

        protected virtual void SetupBackViews(int? index = null)
        {
            var realIndex = index ?? SelectedIndex;

            SetupNextView(realIndex);
            SetupPrevView(realIndex);

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
                index = index.ToCyclingIndex(ItemsCount);
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
            _slideshowTokenSource?.Cancel();
            if (isForceStop)
            {
                return;
            }

            if (SlideShowDuration > 0)
            {
                _slideshowTokenSource = new CancellationTokenSource();
                var token = _slideshowTokenSource.Token;
                while (true)
                {
                    await Task.Delay(SlideShowDuration);
                    if (token.IsCancellationRequested)
                    {
                        return;
                    }
                    if (ItemsCount > 0)
                    {
                        SelectedIndex = (SelectedIndex.ToCyclingIndex(ItemsCount) + 1).ToCyclingIndex(ItemsCount);
                    }
                }
            }
        }

        protected virtual async Task<bool> TryAutoNavigate()
        {
            if (CurrentView == null || !IsAutoNavigatingAimationEnabled)
            {
                return false;
            }

            var context = GetContext(SelectedIndex, true);

            if (GetItem(CurrentView) == context)
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
            var newView = InitViews(BackViewProcessor, realDirection, Enumerable.Empty<View>(), SelectedIndex).FirstOrDefault();
            SwapViews(realDirection);
            CurrentView = newView;


            var views = CurrentBackViews
                .Union(CurrentInactiveBackViews)
                .Union(Enumerable.Repeat(CurrentView, 1))
                .Where(x => x != null);

            var animationId = Guid.NewGuid();
            StartAutoNavigation(views, animationId, animationDirection);
            await Task.Delay(5);
            var autoNavigationTask = Task.WhenAll(
                BackViewProcessor.HandleAutoNavigate(CurrentBackViews, this, realDirection, CurrentInactiveBackViews),
                FrontViewProcessor.HandleAutoNavigate(Enumerable.Repeat(CurrentView, 1), this, realDirection, Enumerable.Empty<View>()));

            await autoNavigationTask;

            EndAutoNavigation(views, animationId, animationDirection);

            return true;
        }

        protected virtual void OnObservableCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ItemsCount = ItemsSource?.Count() ?? -1;

            ShouldSetIndexAfterPan = IsUserInteractionRunning;
            if (!IsUserInteractionRunning)
            {
                SetNewIndex();
            }
        }

        protected virtual bool CheckIsProtectedView(View view) => view.Behaviors.Any(b => b is ProtectedControlBehavior);

        protected virtual bool CheckIsCacheEnabled(DataTemplate template) => IsViewCacheEnabled;

        protected virtual bool CheckIsHardSetCurrentView() => false;

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            lock (_sizeChangedLocker)
            {
                var parent = FindParentPage();
                if (parent == null)
                {
                    return;
                }

                var parentWidth = parent.Width;
                var parentHeight = parent.Height;
                var isValidSize = width > 0 && height > 0;
                var isValidParentSize = parentWidth > 0 && parentHeight > 0;

                if (!_isViewsInited && isValidParentSize && isValidSize)
                {
                    _isViewsInited = true;
                    StoreParentSize(parentWidth, parentHeight);
                    var prevAnimationDirection = AnimationDirection.Prev;
                    var nextAnimationDirection = AnimationDirection.Next;
                    if (IsRightToLeftFlowDirectionEnabled)
                    {
                        prevAnimationDirection = AnimationDirection.Next;
                        nextAnimationDirection = AnimationDirection.Prev;
                    }
                    FrontViewProcessor.HandleInitView(Enumerable.Repeat(CurrentView, 1), this, AnimationDirection.Current);
                    BackViewProcessor.HandleInitView(PrevViews, this, prevAnimationDirection);
                    BackViewProcessor.HandleInitView(NextViews, this, nextAnimationDirection);
                }
                if (_isViewsInited &&
                    isValidParentSize &&
                    Abs(_parentSize.Width - parentWidth) > double.Epsilon &&
                    Abs(_parentSize.Height - parentHeight) > double.Epsilon)
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
                        SetCurrentView();
                        return;
                    }
                    AdjustSlideShow(true);
                    return;
            }
        }

        protected override void OnParentSet()
        {
            base.OnParentSet();
            SetCurrentView();
        }

        private void SetupNextView(int index)
        {
            var indeces = new int[BackViewsDepth];
            for (int i = 0; i < indeces.Length; ++i)
            {
                indeces[i] = index + 1 + i;
            }

            NextViews = InitViews(BackViewProcessor, AnimationDirection.Next, Enumerable.Repeat(CurrentView, 1), indeces);
        }

        private void SetupPrevView(int index)
        {
            var isForwardOnly = IsOnlyForwardDirection;

            var indeces = new int[BackViewsDepth];
            for (int i = 0; i < indeces.Length; ++i)
            {
                var incValue = i + 1;
                if (!isForwardOnly)
                {
                    incValue = -incValue;
                }
                indeces[i] = index + incValue;
            }

            PrevViews = InitViews(BackViewProcessor, AnimationDirection.Prev, Enumerable.Repeat(CurrentView, 1).Union(NextViews), indeces);
        }

        private void StoreParentSize(double width, double height)
        => _parentSize = new Size(width, height);

        private void SetPanGesture(bool _isForceRemove = false)
        {
            if (Device.RuntimePlatform != Device.Android)
            {
                _panGesture.PanUpdated -= OnPanUpdated;
                GestureRecognizers.Remove(_panGesture);
                if (_isForceRemove)
                {
                    return;
                }
                _panGesture.PanUpdated += OnPanUpdated;
                GestureRecognizers.Add(_panGesture);
            }

            if (Device.RuntimePlatform == Device.GTK || Device.RuntimePlatform == Device.Tizen)
            {
                var lastTapTime = DateTime.MinValue;
                const int delay = 200;
                CancellationTokenSource tapCts = null;

                GestureRecognizers.Add(new TapGestureRecognizer
                {
                    Command = new Command(async () =>
                    {
                        var now = DateTime.UtcNow;
                        if (Abs((now - lastTapTime).TotalMilliseconds) < delay)
                        {
                            tapCts?.Cancel();
                            lastTapTime = DateTime.MinValue;
                            SelectedIndex = (SelectedIndex.ToCyclingIndex(ItemsCount) - 1).ToCyclingIndex(ItemsCount);
                            return;
                        }
                        lastTapTime = now;
                        tapCts = new CancellationTokenSource();
                        var token = tapCts.Token;
                        await Task.Delay(delay);
                        if (!token.IsCancellationRequested)
                        {
                            SelectedIndex = (SelectedIndex.ToCyclingIndex(ItemsCount) + 1).ToCyclingIndex(ItemsCount);
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
                if (IsUserInteractionInCourse)
                {
                    _inCoursePanDelay = int.MaxValue;
                }
                lock (_viewsInUseLocker)
                {
                    foreach (var view in views)
                    {
                        _viewsInUse.Add(view);
                    }
                }
                FireItemDisappearing(InteractionType.Auto, animationDirection != AnimationDirection.Prev, OldIndex);
            }
        }

        private void EndAutoNavigation(IEnumerable<View> views, Guid animationId, AnimationDirection animationDirection)
        {
            var isProcessingNow = !_interactions.CheckLastId(animationId);

            _inCoursePanDelay = 0;
            if (views != null)
            {
                lock (_viewsInUseLocker)
                {
                    var depth = BackViewsDepth;
                    foreach (var view in views)
                    {
                        _viewsInUse.Remove(view);
                        if (isProcessingNow &&
                            !_viewsInUse.Contains(view) &&
                            depth <= 1 &&
                            view != CurrentView)
                        {
                            CleanView(view);
                        }
                    }
                }
            }

            IsAutoInteractionRunning = false;
            RemoveRedundantChildren(isProcessingNow);
            FireItemAppearing(InteractionType.Auto, animationDirection != AnimationDirection.Prev, SelectedIndex);
            _interactions.Remove(animationId);
        }

        private AnimationDirection GetAutoNavigateAnimationDirection()
        {
            if (!IsCyclical)
            {
                return SelectedIndex < OldIndex
                       ? AnimationDirection.Prev
                       : AnimationDirection.Next;
            }

            var recIndex = SelectedIndex.ToCyclingIndex(ItemsCount);
            var oldRecIndex = OldIndex.ToCyclingIndex(ItemsCount);

            var deltaIndex = recIndex - oldRecIndex;

            var aniamationDirection = (AnimationDirection)Sign(deltaIndex);

            var cyclingDeltaIndex = ItemsCount - Max(recIndex, oldRecIndex) + Min(recIndex, oldRecIndex);

            if (cyclingDeltaIndex < Abs(deltaIndex))
            {
                aniamationDirection = (AnimationDirection)(-(int)aniamationDirection);
            }

            return aniamationDirection;
        }

        private void OnTouchStarted()
        {
            _shouldScrollParent = null;
            if (!_isPanEndRequested)
            {
                return;
            }

            if (!CheckInteractionDelay())
            {
                _shouldSkipTouch = true;
                return;
            }
            _shouldSkipTouch = false;

            if (IsUserInteractionInCourse)
            {
                _inCoursePanDelay = int.MaxValue;
            }

            var gestureId = Guid.NewGuid();
            _interactions.Add(gestureId, InteractionType.User);

            FireUserInteracted(UserInteractionStatus.Started, CurrentDiff, SelectedIndex);
            if (Device.RuntimePlatform != Device.Android)
            {
                IsUserInteractionRunning = true;
            }
            _isPanEndRequested = false;

            SetupBackViews();
            AddRangeViewsInUse(gestureId);

            _timeDiffItems.Add(new TimeDiffItem
            {
                Time = DateTime.UtcNow,
                Diff = 0
            });
        }

        private void OnTouchChanged(double diff)
        {
            if (_shouldSkipTouch || (_shouldScrollParent ?? false) || Abs(CurrentDiff - diff) <= double.Epsilon)
            {
                return;
            }

            var interactionItem = _interactions.GetFirstItem(InteractionType.User, InteractionState.Regular);
            interactionItem.WasTouchChanged = true;

            ResetActiveInactiveBackViews(diff);

            CurrentDiff = diff;

            SetupDiffItems(diff);

            FireUserInteracted(UserInteractionStatus.Running, CurrentDiff, SelectedIndex);

            FrontViewProcessor.HandlePanChanged(Enumerable.Repeat(CurrentView, 1), this, diff, _currentBackAnimationDirection, Enumerable.Empty<View>());
            BackViewProcessor.HandlePanChanged(CurrentBackViews, this, diff, _currentBackAnimationDirection, CurrentInactiveBackViews);
        }

        private async void OnTouchEnded()
        {
            if (_isPanEndRequested || _shouldSkipTouch)
            {
                return;
            }

            _lastPanTime = DateTime.UtcNow;
            var interactionItem = _interactions.GetFirstItem(InteractionType.User, InteractionState.Regular);
            interactionItem.State = InteractionState.Removing;
            if (interactionItem.Id == default(Guid))
            {
                return;
            }

            var gestureId = interactionItem.Id;

            _isPanEndRequested = true;
            var absDiff = Abs(CurrentDiff);

            var oldIndex = SelectedIndex;
            var index = oldIndex;
            var diff = CurrentDiff;

            CleanDiffItems();

            bool? isNextSelected = null;

            if (IsEnabled && _currentBackAnimationDirection != AnimationDirection.Null)
            {
                var checkSwipe = CheckPanSwipe();
                if (checkSwipe.HasValue)
                {
                    if (checkSwipe.Value || absDiff > MoveDistance)
                    {
                        isNextSelected = diff < 0;
                        FireItemSwiped(isNextSelected.Value ? ItemSwipeDirection.Left : ItemSwipeDirection.Right, oldIndex);
                    }
                }
            }

            _timeDiffItems.Clear();

            Task endingTask = null;
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

                endingTask = Task.WhenAll(
                    FrontViewProcessor.HandlePanApply(Enumerable.Repeat(CurrentView, 1), this, _currentBackAnimationDirection, Enumerable.Empty<View>()),
                    BackViewProcessor.HandlePanApply(CurrentBackViews, this, _currentBackAnimationDirection, CurrentInactiveBackViews)
                );
            }
            else
            {
                endingTask = interactionItem.WasTouchChanged
                    ? Task.WhenAll(
                        FrontViewProcessor.HandlePanReset(Enumerable.Repeat(CurrentView, 1), this, _currentBackAnimationDirection, Enumerable.Empty<View>()),
                        BackViewProcessor.HandlePanReset(CurrentBackViews, this, _currentBackAnimationDirection, CurrentInactiveBackViews))
                    : Task.FromResult(true);
            }

            FireUserInteracted(UserInteractionStatus.Ending, diff, oldIndex);
            if (isNextSelected.HasValue)
            {
                FireItemDisappearing(InteractionType.User, isNextSelected.GetValueOrDefault(), oldIndex);
            }
            CurrentDiff = 0;

            await endingTask;

            FireUserInteracted(UserInteractionStatus.Ended, diff, oldIndex);
            if (isNextSelected.HasValue)
            {
                FireItemAppearing(InteractionType.User, isNextSelected.GetValueOrDefault(), index);
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

            RemoveRedundantChildren(isProcessingNow);

            _inCoursePanDelay = 0;

            _interactions.Remove(gestureId);
        }

        private bool CheckInteractionDelay()
            => IsUserInteractionEnabled && Abs((DateTime.UtcNow - _lastPanTime).TotalMilliseconds) >= UserInteractionDelay && CurrentView != null;

        private bool? CheckPanSwipe()
        {
            if (!IsPanSwipeEnabled)
            {
                return null;
            }

            if (_timeDiffItems.Count < 2)
            {
                return false;
            }

            var lastItem = _timeDiffItems.Last();
            var firstItem = _timeDiffItems.First();

            var distDiff = lastItem.Diff - firstItem.Diff;

            if (Sign(distDiff) != Sign(lastItem.Diff))
            {
                return null;
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

                newIndex = newIndex.ToCyclingIndex(ItemsCount);
            }

            return newIndex;
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

        private IEnumerable<View> InitViews(ICardProcessor processor, AnimationDirection animationDirection, IEnumerable<View> bookedViews, params int[] indeces)
        {
            var views = new View[indeces.Length];

            for (int i = 0; i < indeces.Length; ++i)
            {
                var view = PrepareView(bookedViews, indeces[i]);
                views[i] = view;
                if (view != null)
                {
                    bookedViews = bookedViews.Union(Enumerable.Repeat(view, 1));
                }
            }

            if (views.All(x => x == null))
            {
                return Enumerable.Empty<View>();
            }

            processor.HandleInitView(views, this, animationDirection);

            SetupLayout(views);

            if (animationDirection == AnimationDirection.Current)
            {
                AddBackChild(views);
            }
            else
            {
                AddChild(CurrentView, views);
            }
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
            if (!CheckIsCacheEnabled(template))
            {
                return template.CreateView();
            }

            List<View> viewsList;
            if (!_viewsPool.TryGetValue(template, out viewsList))
            {
                viewsList = new List<View>();
                _viewsPool.Add(template, viewsList);
            }

            var notUsingViews = viewsList.Where(v => !_viewsInUse.Contains(v) && !bookedViews.Contains(v));
            var view = notUsingViews.FirstOrDefault(v => v.BindingContext == context || v == context)
                                    ?? notUsingViews.FirstOrDefault(v => v.BindingContext == null)
                                    ?? notUsingViews.FirstOrDefault(v => !CheckIsProcessingView(v));

            if (view == null)
            {
                view = template.CreateView();
                viewsList.Add(view);
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

                index = index.ToCyclingIndex(ItemsCount);
            }

            if (index < 0 || ItemsSource == null)
            {
                return null;
            }

            return this[index];
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
                index = index.ToCyclingIndex(ItemsCount);
            }
            return index >= 0 && index < ItemsCount
                ? this[index]
                    : null;
        }

        private void SendChildrenToBackIfNeeded(View view, View topView)
        {
            lock (_childLocker)
            {
                if (view == null || topView == null)
                {
                    return;
                }

                var currentIndex = Children.IndexOf(topView);
                var backIndex = Children.IndexOf(view);

                if (currentIndex < backIndex)
                {
                    ExecutePreventInvalidOperationException(() => LowerChild(view));
                }
            }
        }

        private void CleanUnprocessingChildren()
        {
            lock (_childLocker)
            {
                var views = Children.Where(c => !CheckIsProtectedView(c) && !CheckIsProcessingView(c)).ToArray();
                foreach (var view in views)
                {
                    CleanView(view);
                }
            }
        }

        private void CleanView(View view)
        {
            if (CheckContextAssigned(view))
            {
                view.Behaviors.Remove(_contextAssignedBehavior);
                view.BindingContext = null;
            }
            BackViewProcessor.HandleCleanView(Enumerable.Repeat(view, 1), this);
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
            var isCurrentContextPresent = false;

            if (CurrentView != null)
            {
                var currentItem = GetItem(CurrentView);
                for (var i = 0; i < ItemsCount; ++i)
                {
                    if (this[i] == currentItem)
                    {
                        index = i;
                        isCurrentContextPresent = true;
                        break;
                    }
                }

                if (index < 0)
                {
                    index = SelectedIndex - 1;
                }
            }

            if (index < 0)
            {
                index = 0;
            }

            if (SelectedIndex == index)
            {
                if (!isCurrentContextPresent || BackViewsDepth > 1)
                {
                    OldIndex = index;
                    SetCurrentView();
                }
                return;
            }

            SelectedIndex = index;
        }

        private void AddBackChild(params View[] views)
        {
            lock (_childLocker)
            {
                foreach (var view in views)
                {
                    if (view == null || Children.Contains(view))
                    {
                        continue;
                    }

                    ++_viewsChildrenCount;

                    ExecutePreventInvalidOperationException(() => Children.Insert(0, view));
                }
            }
        }

        private void AddChild(View topView, params View[] views)
        {
            lock (_childLocker)
            {
                foreach (var view in views)
                {
                    if (view == null)
                    {
                        continue;
                    }

                    if (Children.Contains(view))
                    {
                        SendChildrenToBackIfNeeded(view, topView);
                        continue;
                    }

                    ++_viewsChildrenCount;
                    var index = Children.IndexOf(topView);

                    ExecutePreventInvalidOperationException(() => Children.Insert(index, view));
                }
            }
        }

        private void RemoveRedundantChildren(bool isProcessingNow)
        {
            var maxChildrenCount = isProcessingNow
                ? MaxChildrenCount
                : DesiredMaxChildrenCount;

            if (_viewsChildrenCount <= maxChildrenCount)
            {
                return;
            }

            lock (_childLocker)
            {
                var views = Children.Where(c => !CheckIsProtectedView(c) && !CheckIsProcessingView(c) && !_viewsInUse.Contains(c)).Take(_viewsChildrenCount - DesiredMaxChildrenCount).ToArray();
                RemoveChildren(views);
            }
        }

        private void RemoveUnprocessingChildren()
        {
            lock (_childLocker)
            {
                var views = Children.Where(c => !CheckIsProtectedView(c) && !CheckIsProcessingView(c)).ToArray();
                RemoveChildren(views);
            }
        }

        private void RemoveChildren(View[] views)
        {
            _viewsChildrenCount -= views.Length;
            foreach (var view in views)
            {
                ExecutePreventInvalidOperationException(() => Children.Remove(view));
                CleanView(view);
            }
        }

        private void ExecutePreventInvalidOperationException(Action action)
        {
            try
            {
                action?.Invoke();
            }
            catch (InvalidOperationException)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    try
                    {
                        action?.Invoke();
                    }
                    catch (InvalidOperationException)
                    {
                        System.Diagnostics.Debug.WriteLine("CardsView: Couldn't handle InvalidOperationException");
                    }
                });
            }
        }

        private Page FindParentPage()
        {
            var parent = Parent;
            while (parent != null && !(parent is Page))
            {
                parent = parent.Parent;
            }
            return parent as Page;
        }

        private bool CheckIsProcessingView(View view)
        => view == CurrentView || NextViews.Contains(view) || PrevViews.Contains(view);

        private bool CheckIndexValid(int index)
        => index >= 0 && index < ItemsCount;

        private void AddRangeViewsInUse(Guid gestureId)
        {
            lock (_viewsInUseLocker)
            {
                var views = NextViews.Union(PrevViews).Union(Enumerable.Repeat(CurrentView, 1));

                _viewsGestureCounter[gestureId] = views;

                foreach (var view in views.Where(v => v != null))
                {
                    _viewsInUse.Add(view);
                }
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

                foreach (var view in _viewsGestureCounter[gestureId])
                {
                    _viewsInUse.Remove(view);
                    if (isProcessingNow && !_viewsInUse.Contains(view))
                    {
                        CleanView(view);
                    }
                }
                _viewsGestureCounter.Remove(gestureId);
            }
        }

        private void HandleParentScroll(PanUpdatedEventArgs e)
        {
            if (Device.RuntimePlatform == Device.iOS)
            {
                var y = e.TotalY;
                var absY = Abs(y);
                var absX = Abs(e.TotalX);

                var isFirst = false;
                if (!_shouldScrollParent.HasValue)
                {
                    _shouldScrollParent = absY > absX;
                    isFirst = true;
                }

                if (_shouldScrollParent.Value)
                {
                    var parent = Parent;
                    while (parent != null)
                    {
                        if (parent is IOrdinateHandlerParentView scrollableView)
                        {
                            scrollableView.HandleOrdinateValue(y, isFirst);
                            break;
                        }
                        parent = parent.Parent;
                    }
                }
            }
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

        private void FireItemSwiped(ItemSwipeDirection swipeDirection, int index)
        {
            var item = GetItem(index);
            var args = new ItemSwipedEventArgs(swipeDirection, index, item);
            ItemSwipedCommand?.Execute(args);
            ItemSwiped?.Invoke(this, args);
        }
    }
}