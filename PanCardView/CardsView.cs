using System;
using Xamarin.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Windows.Input;
using PanCardView.Extensions;
using PanCardView.Factory;
using PanCardView.Processors;
using System.Collections;
using PanCardView.Enums;

namespace PanCardView
{
    public delegate void CardsViewPanStartEndHandler(CardsView view, int index, double diff);
    public delegate void CardsViewPanChangedHandler(CardsView view, double diff);
    public delegate void CardsViewPositionChangedHandler(CardsView view, bool isNextSelected);

    public class CardsView : AbsoluteLayout
    {
        public event CardsViewPanStartEndHandler PanStarted;
        public event CardsViewPanStartEndHandler PanEnding;
        public event CardsViewPanStartEndHandler PanEnded;
        public event CardsViewPanChangedHandler PanChanged;
        public event CardsViewPositionChangedHandler PositionChanging;
        public event CardsViewPositionChangedHandler PositionChanged;

        public static readonly BindableProperty CurrentIndexProperty = BindableProperty.Create(nameof(CurrentIndex), typeof(int), typeof(CardsView), 0, BindingMode.TwoWay, propertyChanged: (bindable, oldValue, newValue) => {
            var view = bindable.AsCardView();
            view.OldIndex = (int)oldValue;
            if(view.ShouldIgnoreSetCurrentView)
            {
                view.ShouldIgnoreSetCurrentView = false;
                return;
            }
            view.SetCurrentView();
        });

        public static readonly BindableProperty ItemsProperty = BindableProperty.Create(nameof(Items), typeof(IList), typeof(CardsView), null, propertyChanged: (bindable, oldValue, newValue) => {
            bindable.AsCardView().SetItemsCount();
            bindable.AsCardView().SetCurrentView();
        });

        [Obsolete("This property is obsolete and will be removed soon. Use DataTemplateProperty instead")]
        public static readonly BindableProperty ItemViewFactoryProperty = BindableProperty.Create(nameof(ItemViewFactory), typeof(CardViewItemFactory), typeof(CardsView), null, propertyChanged: (bindable, oldValue, newValue) => {
            bindable.AsCardView().SetCurrentView();
        });

        public static readonly BindableProperty DataTemplateProperty = BindableProperty.Create(nameof(DataTemplate), typeof(DataTemplate), typeof(CardsView), null, propertyChanged: (bindable, oldValue, newValue) => {
            bindable.AsCardView().SetCurrentView();
        });

        public static readonly BindableProperty CurrentContextProperty = BindableProperty.Create(nameof(CurrentContext), typeof(object), typeof(CardsView), null, BindingMode.OneWay, propertyChanged: (bindable, oldValue, newValue) => {
            bindable.AsCardView().SetCurrentView(true);
        });

        public static readonly BindableProperty NextContextProperty = BindableProperty.Create(nameof(NextContext), typeof(object), typeof(CardsView), null, BindingMode.OneWay);

        public static readonly BindableProperty PrevContextProperty = BindableProperty.Create(nameof(PrevContext), typeof(object), typeof(CardsView), null, BindingMode.OneWay);

        public static readonly BindableProperty IsPanEnabledProperty = BindableProperty.Create(nameof(IsPanEnabled), typeof(bool), typeof(CardsView), true);

        public static readonly BindableProperty MoveDistanceProperty = BindableProperty.Create(nameof(MoveDistance), typeof(double), typeof(CardsView), -1.0);

        public static readonly BindableProperty MoveWidthPercentageProperty = BindableProperty.Create(nameof(MoveWidthPercentage), typeof(double), typeof(CardsView), .325);

        public static readonly BindableProperty IsOnlyForwardDirectionProperty = BindableProperty.Create(nameof(IsOnlyForwardDirection), typeof(bool), typeof(CardsView), false);

        public static readonly BindableProperty PanDelayProperty = BindableProperty.Create(nameof(PanDelay), typeof(int), typeof(CardsView), 200);

        public static readonly BindableProperty IsPanInCourseProperty = BindableProperty.Create(nameof(IsPanInCourse), typeof(bool), typeof(CardsView), true);

        public static readonly BindableProperty IsRecycledProperty = BindableProperty.Create(nameof(IsRecycled), typeof(bool), typeof(CardsView), false);

        public static readonly BindableProperty IsAutoNavigatingProperty = BindableProperty.Create(nameof(IsAutoNavigating), typeof(bool), typeof(CardsView), false, BindingMode.OneWayToSource);

        public static readonly BindableProperty MaxChildrenCountProperty = BindableProperty.Create(nameof(MaxChildrenCount), typeof(int), typeof(CardsView), 12);

        public static readonly BindableProperty DesiredMaxChildrenCountProperty = BindableProperty.Create(nameof(DesiredMaxChildrenCount), typeof(int), typeof(CardsView), 6);

        public static readonly BindableProperty SwipeThresholdDistanceProperty = BindableProperty.Create(nameof(SwipeThresholdDistance), typeof(double), typeof(CardsView), 17.0);

        public static readonly BindableProperty SwipeThresholdTimeProperty = BindableProperty.Create(nameof(SwipeThresholdTime), typeof(TimeSpan), typeof(CardsView), TimeSpan.FromMilliseconds(60));

        public static readonly BindableProperty PanStartedCommandProperty = BindableProperty.Create(nameof(PanStartedCommand), typeof(ICommand), typeof(CardsView), null);

        public static readonly BindableProperty PanEndingCommandProperty = BindableProperty.Create(nameof(PanEndingCommand), typeof(ICommand), typeof(CardsView), null);

        public static readonly BindableProperty PanEndedCommandProperty = BindableProperty.Create(nameof(PanEndedCommand), typeof(ICommand), typeof(CardsView), null);

        public static readonly BindableProperty PanChangedCommandProperty = BindableProperty.Create(nameof(PanChangedCommand), typeof(ICommand), typeof(CardsView), null);

        public static readonly BindableProperty PositionChangingCommandProperty = BindableProperty.Create(nameof(PositionChangingCommand), typeof(ICommand), typeof(CardsView), null);

        public static readonly BindableProperty PositionChangedCommandProperty = BindableProperty.Create(nameof(PositionChangedCommand), typeof(ICommand), typeof(CardsView), null);

        private readonly Dictionary<object, List<View>> _viewsPool = new Dictionary<object, List<View>>();
        private readonly HashSet<View> _viewsInUse = new HashSet<View>();
        private readonly Dictionary<Guid, View[]> _viewsGestureCounter = new Dictionary<Guid, View[]>();
        private readonly List<TimeDiffItem> _timeDiffItems = new List<TimeDiffItem>();
        private readonly object _childLocker = new object();
        private readonly object _viewsInUseLocker = new object();

        private View _currentView;
        private View _nextView;
        private View _prevView;
        private View _currentBackView;
        private PanItemPosition _currentBackPanItemPosition;

        private INotifyCollectionChanged _currentObservableCollection;

        private int _itemsCount = -1;
        private int _viewsChildrenCount;
        private int _inCoursePanDelay;
        private bool _isPanRunning;
        private bool _isPanEndRequested = true;
        private bool _shouldSkipTouch;
        private Guid _gestureId;
        private DateTime _lastPanTime;

        public CardsView() : this(null, null)
        {
        }

        public CardsView(ICardProcessor frontViewProcessor, ICardProcessor backViewProcessor)
        {
            FrontViewProcessor = frontViewProcessor ?? new BaseCardFrontViewProcessor();
            BackViewProcessor = backViewProcessor ?? new BaseCardBackViewProcessor();
            var panGesture = new PanGestureRecognizer();
            panGesture.PanUpdated += OnPanUpdated;
            GestureRecognizers.Add(panGesture);
        }

        public double CurrentDiff { get; private set; }

        public int OldIndex { get; private set; } = -1;

        public ICardProcessor FrontViewProcessor { get; }

        public ICardProcessor BackViewProcessor { get; }

        private bool ShouldIgnoreSetCurrentView { get; set; }

        private bool ShouldSetIndexAfterPan { get; set; }

        public int CurrentIndex 
        {
            get => (int)GetValue(CurrentIndexProperty);
            set => SetValue(CurrentIndexProperty, value);
        }

        public IList Items 
        {
            get => GetValue(ItemsProperty) as IList;
            set => SetValue(ItemsProperty, value);
        }

        [Obsolete("This property is obsolete and will be deleted soon. Use DataTemplate instead")]
        public CardViewItemFactory ItemViewFactory 
        {
            get => GetValue(ItemViewFactoryProperty) as CardViewItemFactory;
            set => SetValue(ItemViewFactoryProperty, value);
        }

        public DataTemplate DataTemplate
        {
            get => GetValue(DataTemplateProperty) as DataTemplate;
            set => SetValue(DataTemplateProperty, value);
        }

        public object CurrentContext
        {
            get => GetValue(CurrentContextProperty);
            set => SetValue(CurrentContextProperty, value);
        }

        public object NextContext
        {
            get => GetValue(NextContextProperty);
            set => SetValue(NextContextProperty, value);
        }

        public object PrevContext
        {
            get => GetValue(PrevContextProperty);
            set => SetValue(PrevContextProperty, value);
        }

        public bool IsPanEnabled
        {
            get => (bool)GetValue(IsPanEnabledProperty);
            set => SetValue(IsPanEnabledProperty, value);
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

        public int PanDelay
        {
            get => IsPanInCourse ? _inCoursePanDelay : (int)GetValue(PanDelayProperty);
            set => SetValue(PanDelayProperty, value);
        }

        public bool IsPanInCourse
        {
            get => (bool)GetValue(IsPanInCourseProperty);
            set => SetValue(IsPanInCourseProperty, value);
        }

        public bool IsRecycled
        {
            get => (bool)GetValue(IsRecycledProperty);
            set => SetValue(IsRecycledProperty, value);
        }

        public bool IsAutoNavigating
        {
            get => (bool)GetValue(IsAutoNavigatingProperty);
            set => SetValue(IsAutoNavigatingProperty, value);
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

        public TimeSpan SwipeThresholdTime
        {
            get => (TimeSpan)GetValue(SwipeThresholdTimeProperty);
            set => SetValue(SwipeThresholdTimeProperty, value);
        }

        public bool IsOnlyForwardDirection
        {
            get => (bool)GetValue(IsOnlyForwardDirectionProperty);
            set => SetValue(IsOnlyForwardDirectionProperty, value);
        }

        public ICommand PanStartedCommand
        {
            get => GetValue(PanStartedCommandProperty) as ICommand;
            set => SetValue(PanStartedCommandProperty, value);
        }

        public ICommand PanEndingCommand
        {
            get => GetValue(PanEndingCommandProperty) as ICommand;
            set => SetValue(PanEndingCommandProperty, value);
        }

        public ICommand PanEndedCommand
        {
            get => GetValue(PanEndedCommandProperty) as ICommand;
            set => SetValue(PanEndedCommandProperty, value);
        }

        public ICommand PanChangedCommand
        {
            get => GetValue(PanChangedCommandProperty) as ICommand;
            set => SetValue(PanChangedCommandProperty, value);
        }

        public ICommand PositionChangingCommand
        {
            get => GetValue(PositionChangingCommandProperty) as ICommand;
            set => SetValue(PositionChangingCommandProperty, value);
        }

        public ICommand PositionChangedCommand
        {
            get => GetValue(PositionChangedCommandProperty) as ICommand;
            set => SetValue(PositionChangedCommandProperty, value);
        }

        public void OnPanUpdated(object sender, PanUpdatedEventArgs e)
        {
            if (_itemsCount < 0 && CurrentContext == null)
            {
                return;
            }

            switch (e.StatusType)
            {
                case GestureStatus.Started:
                    if (Device.RuntimePlatform != Device.Android || e.GestureId == -1)
                    {
                        OnTouchStarted();
                        (_currentView as ICardItem)?.HandleTouchStarted();
                    }
                    return;
                case GestureStatus.Running:
                    var handled = (_currentView as ICardItem)?.HandeTouchChanged(e.TotalX, e.TotalY) ?? false;
                    if (!handled)
                    {
                        OnTouchChanged(e.TotalX);
                    }
                    return;
                case GestureStatus.Canceled:
                case GestureStatus.Completed:
                    if (Device.RuntimePlatform != Device.Android || e.GestureId == -1)
                    {
                        OnTouchEnded();
                        (_currentView as ICardItem)?.HandleTouchEnded();
                    }
                    return;
            }
        }

        public void AutoNavigatingStarted(View view)
        {
            if (view != null)
            {
                IsAutoNavigating = true;
                if(IsPanInCourse)
                {
                    _inCoursePanDelay = int.MaxValue;
                }
                lock (_viewsInUseLocker)
                {
                    _viewsInUse.Add(view);
                }
            }
        }

        public void AutoNavigatingEnded(View view)
        {
            _inCoursePanDelay = 0;
            if(view != null)
            {
                lock (_viewsInUseLocker)
                {
                    _viewsInUse.Remove(view);
                    view.IsVisible = false;
                    ClearBindingContext(view);
                }
            }
            IsAutoNavigating = false;
        }

        protected virtual void SetupBackViews(bool? isOnStart = null)
        {
            SetupNextView();
            SetupPrevView();
        }

        protected virtual void SetupLayout(View view)
        {
            SetLayoutBounds(view, new Rectangle(0, 0, 1, 1));
            SetLayoutFlags(view, AbsoluteLayoutFlags.All);
        }

        protected virtual void SetCurrentView(bool canResetContext = false)
        {
            if(TryAutoNavigate())
            {
                return;
            }

            if (TryResetContext(canResetContext, _currentView, CurrentContext))
            {
                return;
            }

            if (Items != null || CurrentContext != null)
            {
                _currentView = GetView(CurrentIndex, PanItemPosition.Current);
                if(_currentView == null && CurrentIndex >= 0)
                {
                    ShouldIgnoreSetCurrentView = true;
                    CurrentIndex = -1;
                }
            }

            SetupBackViews(null);
        }

        protected virtual bool TryAutoNavigate()
        {
            if (_currentView == null)
            {
                return false;
            }

            var context = GetContext(CurrentIndex, PanItemPosition.Current);

            if(_currentView.BindingContext == context)
            {
                return false;
            }

            var autoNavigatePanPosition = GetAutoNavigatePanPosition();

            var oldView = _currentView;
            var view = PrepareView(CurrentIndex, PanItemPosition.Current, out context);
            if(view == null)
            {
                return false;
            }

            _currentView = view;

            BackViewProcessor.InitView(_currentView, this, autoNavigatePanPosition);

            _currentView.BindingContext = context;
            SetupLayout(_currentView);

            AddChild(_currentView, oldView);

            BackViewProcessor.AutoNavigate(oldView, this, autoNavigatePanPosition);
            FrontViewProcessor.AutoNavigate(_currentView, this, autoNavigatePanPosition);

            SetupBackViews(null);

            return true;
        }

        protected virtual void SetupNextView(bool canResetContext = false)
        {
            if (TryResetContext(canResetContext, _nextView, NextContext))
            {
                return;
            }

            var nextIndex = CurrentIndex + 1;
            _nextView = GetView(nextIndex, PanItemPosition.Next);
        }

        protected virtual void SetupPrevView(bool canResetContext = false)
        {
            if (TryResetContext(canResetContext, _prevView, PrevContext))
            {
                return;
            }

            var prevIndex = IsOnlyForwardDirection
                ? CurrentIndex + 1
                : CurrentIndex - 1;
            _prevView = GetView(prevIndex, PanItemPosition.Prev);
        }

        protected virtual bool TryResetContext(bool canResetContext, View view, object context)
        {
            if (canResetContext && view != null)
            {
                view.BindingContext = context;
                return true;
            }
            return false;
        }

        private PanItemPosition GetAutoNavigatePanPosition()
        {
            if(CurrentContext != null)
            {
                return CurrentContext == _prevView?.BindingContext
                       ? PanItemPosition.Prev
                       : PanItemPosition.Next;
            }

            if(!IsRecycled)
            {
                return CurrentIndex < Items.IndexOf(_currentView.BindingContext)
                       ? PanItemPosition.Prev
                       : PanItemPosition.Next;
            }

            var recIndex = GetRecycledIndex(CurrentIndex);
            var oldRecIndex = GetRecycledIndex(OldIndex);

            var deltaIndex = recIndex - oldRecIndex;
            if(Math.Abs(deltaIndex) == 1)
            {
                return deltaIndex > 0
                    ? PanItemPosition.Next
                     : PanItemPosition.Prev;
            }

            return deltaIndex > 0
                    ? PanItemPosition.Prev
                     : PanItemPosition.Next;
        }

        private void OnTouchStarted()
        {
            if(!_isPanEndRequested)
            {
                return;
            }

            var deltaTime = DateTime.Now - _lastPanTime;
            if(!IsPanEnabled || deltaTime.TotalMilliseconds < PanDelay)
            {
                _shouldSkipTouch = true;
                return;
            }
            _shouldSkipTouch = false;

            if(IsPanInCourse)
            {
                _inCoursePanDelay = int.MaxValue;
            }

            _gestureId = Guid.NewGuid();
            FirePanStarted();
            _isPanRunning = true;
            _isPanEndRequested = false;

            SetupBackViews(true);
            AddRangeViewsInUse();

            _timeDiffItems.Add(new TimeDiffItem
            {
                Time = DateTime.Now,
                Diff = 0
            });
        }

        private void OnTouchChanged(double diff)
        {
            if(_shouldSkipTouch)
            {
                return;
            }

            if (!TrySetSelectedBackView(diff))
            {
                return;
            }

            _currentBackView.IsVisible = true;
            CurrentDiff = diff;

            SetupDiffItems(diff);

            FirePanChanged();

            FrontViewProcessor.HandlePanChanged(_currentView, this, diff, _currentBackPanItemPosition);
            BackViewProcessor.HandlePanChanged(_currentBackView, this, diff, _currentBackPanItemPosition);
        }

        private async void OnTouchEnded()
        {
            if (_isPanEndRequested || _shouldSkipTouch)
            {
                return;
            }

            _lastPanTime = DateTime.Now;
            var gestureId = _gestureId;

            _isPanEndRequested = true;
            var absDiff = Math.Abs(CurrentDiff);

            var index = CurrentIndex;
            var diff = CurrentDiff;

            CleanDiffItems();

            bool? isNextSelected = null;

            if(IsEnabled)
            {
                var checkSwipe = CheckSwipe();
                if(checkSwipe.HasValue && (checkSwipe.Value || absDiff > MoveDistance))
                {
                    isNextSelected = diff < 0;
                }
            }

            _timeDiffItems.Clear();

            if (isNextSelected.HasValue)
            {
                index = GetNewIndexFromDiff();
                if(index < 0)
                {
                    return;
                }

                SwapViews(isNextSelected.GetValueOrDefault());
                ShouldIgnoreSetCurrentView = true;

                CurrentIndex = index;

                FirePanEnding(isNextSelected, index, diff);

                await Task.WhenAll(
                    FrontViewProcessor.HandlePanApply(_currentView, this, _currentBackPanItemPosition),
                    BackViewProcessor.HandlePanApply(_currentBackView, this, _currentBackPanItemPosition)
                );
            }
            else
            {
                FirePanEnding(isNextSelected, index, diff);
                await Task.WhenAll(
                    FrontViewProcessor.HandlePanReset(_currentView, this, _currentBackPanItemPosition),
                    BackViewProcessor.HandlePanReset(_currentBackView, this, _currentBackPanItemPosition)
                );
            }

            FirePanEnded(isNextSelected, index, diff);

            RemoveRangeViewsInUse(gestureId);
            var isProcessingNow = gestureId != _gestureId;
            if (!isProcessingNow)
            {
                _isPanRunning = false;
                if (ShouldSetIndexAfterPan)
                {
                    ShouldSetIndexAfterPan = false;
                    SetNewIndex();
                }
                if(CurrentContext == null)
                {
                    SetupBackViews(false);
                }
            }

            var maxChildrenCount = isProcessingNow ? MaxChildrenCount : DesiredMaxChildrenCount;

            if (_viewsChildrenCount > maxChildrenCount)
            {
                RemoveChildren(Children.Where(c => c != _prevView && c != _nextView && !c.IsVisible).Take(_viewsChildrenCount - DesiredMaxChildrenCount).ToArray());
            }

            _inCoursePanDelay = 0;
        }

        private bool? CheckSwipe()
        {
            if(_timeDiffItems.Count < 2)
            {
                return false;
            }

            var lastItem = _timeDiffItems.Last();
            var firstItem = _timeDiffItems.First();

            var distDiff = lastItem.Diff - firstItem.Diff;

            if(Math.Sign(distDiff) != Math.Sign(lastItem.Diff))
            {
                return null;
            }

            var absDistDiff = Math.Abs(distDiff);
            var timeDiff = lastItem.Time - firstItem.Time;

            var acceptValue = SwipeThresholdDistance * timeDiff.TotalMilliseconds / SwipeThresholdTime.TotalMilliseconds;

            return absDistDiff >= acceptValue;
        }

        private void SetupDiffItems(double diff)
        {
            var timeNow = DateTime.Now;

            if(_timeDiffItems.Count >= 25)
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
            if(CurrentContext != null)
            {
                return 0;
            }

            var indexDelta = -Math.Sign(CurrentDiff);
            if (IsOnlyForwardDirection)
            {
                indexDelta = Math.Abs(indexDelta);
            }
            var newIndex = CurrentIndex + indexDelta;

            if (newIndex < 0 || newIndex >= _itemsCount)
            {
                if (!IsRecycled)
                {
                    return -1;
                }

                newIndex = GetRecycledIndex(newIndex);
            }

            return newIndex;
        }

        private bool TrySetSelectedBackView(double diff)
        {
            View invisibleView;
            if (diff > 0)
            {
                _currentBackView = _prevView;
                invisibleView = _nextView;
                _currentBackPanItemPosition = PanItemPosition.Prev;
            }
            else
            {
                _currentBackView = _nextView;
                invisibleView = _prevView;
                _currentBackPanItemPosition = PanItemPosition.Next;
            }

            if (invisibleView != null && invisibleView != _currentBackView)
            {
                invisibleView.IsVisible = false;
            }

            return _currentBackView != null;
        }

        private void SwapViews(bool isNext)
        {
            var view = _currentView;
            _currentView = _currentBackView;
            _currentBackView = view;

            if(isNext)
            {
                _nextView = _prevView;
                _prevView = _currentBackView;
                return;
            }
            _prevView = _nextView;
            _nextView = _currentBackView;
        }

        private View GetView(int index, PanItemPosition panIntemPosition)
        {
            var view = PrepareView(index, panIntemPosition, out object context);
            if(view == null)
            {
                return null;
            }
            InitProcessor(view, panIntemPosition);
            view.BindingContext = context;
            SetupLayout(view);
            if(panIntemPosition == PanItemPosition.Current)
            {
                AddChild(view, 0);
            }
            else
            {
                AddChild(view, _currentView);
            }
            return view;
        }

        private View PrepareView(int index, PanItemPosition panIntemPosition, out object context)
        {
            context = GetContext(index, panIntemPosition);
            if (context == null)
            {
                return null;
            }

            ChooseViewCreator(context, out object rule, out Func<View> creator);

            List<View> viewsList;
            if (!_viewsPool.TryGetValue(rule, out viewsList))
            {
                viewsList = new List<View>
                {
                    creator.Invoke()
                };
                _viewsPool.Add(rule, viewsList);
            }

            var notUsingViews = viewsList.Where(v => !CheckUsingNow(v));
            var currentContext = context;
            var view = notUsingViews.FirstOrDefault(v => v.BindingContext == currentContext)
                                    ?? notUsingViews.FirstOrDefault(v => v.BindingContext == null)
                                    ?? notUsingViews.FirstOrDefault(v => !CheckIsProcessingView(v));

            if (view == null)
            {
                view = creator.Invoke();
                viewsList.Add(view);
            }

            return view;
        }

        private void ChooseViewCreator(object context, out object creatorKey, out Func<View> creator)
        {
            if (DataTemplate != null)
            {
                var template = DataTemplate;
                if (DataTemplate is DataTemplateSelector selector)
                {
                    template = selector.SelectTemplate(context, this);
                }
                creatorKey = template;
                creator = () => template.CreateContent() as View;
                return;
            }

#pragma warning disable //Obsolete property
            var rule = ItemViewFactory?.GetRule(context);
#pragma warning restore

            creatorKey = rule;
            creator = rule.Creator;
        }

        private object GetContext(int index, PanItemPosition panIntemPosition)
        {
            if (CurrentContext != null)
            {
                switch (panIntemPosition)
                {
                    case PanItemPosition.Current:
                        return CurrentContext;
                    case PanItemPosition.Next:
                        return NextContext;
                    case PanItemPosition.Prev:
                        return PrevContext;
                }
            }

            if (_itemsCount < 0)
            {
                return null;
            }

            if (index < 0 || index >= _itemsCount)
            {
                if (!IsRecycled || (panIntemPosition != PanItemPosition.Current && _itemsCount < 2))
                {
                    return null;
                }

                index = GetRecycledIndex(index);
            }

            if(index < 0)
            {
                return null;
            }

            return Items[index];
        }

        private void SendChildrenToBackIfNeeded(View view, View topView)
        {
            if(view == null || topView == null)
            {
                return;
            }

            var currentIndex = Children.IndexOf(topView);
            var backIndex = Children.IndexOf(view);

            if (currentIndex < backIndex)
            {
                lock (_childLocker)
                {
                    SendChildToBack(view);
                }
            }
        }

        private void ClearBindingContext(View view)
        {
            if(view != null)
            {
                view.BindingContext = null;
            }
        }

        private void SetItemsCount()
        {
            if(_currentObservableCollection != null)
            {
                _currentObservableCollection.CollectionChanged -= OnObservableCollectionChanged;
            }

            if(Items is INotifyCollectionChanged observableCollection)
            {
                _currentObservableCollection = observableCollection;
                observableCollection.CollectionChanged += OnObservableCollectionChanged;
            }

            OnObservableCollectionChanged(Items, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        private void OnObservableCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            _itemsCount = Items?.Count ?? -1;

            ShouldSetIndexAfterPan = _isPanRunning;
            if(!_isPanRunning)
            {
                SetNewIndex();
            }
        }

        private void SetNewIndex()
        {
            var index = 0;
            if (_currentView != null)
            {
                for (var i = 0; i < _itemsCount; ++i)
                {
                    if(Items[i] == _currentView.BindingContext)
                    {
                        index = i;
                        break;
                    }
                }

                if (index < 0)
                {
                    index = CurrentIndex - 1;
                }
            }

            if (index < 0)
            {
                index = 0;
            }

            CurrentIndex = index;
        }

        private void AddChild(View view, int index = -1)
        {
            if (view == null || Children.Contains(view))
            {
                return;
            }

            lock (_childLocker)
            {
                ++_viewsChildrenCount;
                if (index < 0 || !Children.Any())
                {
                    Children.Add(view);
                    return;
                }
                Children.Insert(index, view);
            }
        }

        private void AddChild(View view, View topView)
        {
            if (view == null)
            {
                return;
            }

            if(Children.Contains(view))
            {
                SendChildrenToBackIfNeeded(view, topView);
                return;
            }

            lock (_childLocker)
            {
                ++_viewsChildrenCount;
                Children.Insert(Children.IndexOf(topView), view);
            }

        }

        private void RemoveChildren(View[] views)
        {
            if (views == null)
            {
                return;
            }

            lock (_childLocker)
            {
                _viewsChildrenCount -= views.Length;

                foreach (var view in views)
                {
                    Children.Remove(view);
                    ClearBindingContext(view);
                }
            }
        }

        private void SendChildToBack(View view)
        {
            Children.Remove(view);
            Children.Insert(0, view);
        }

        private bool CheckUsingNow(View view) => _viewsInUse.Contains(view);

        private bool CheckIsProcessingView(View view) => view == _currentView || view == _nextView || view == _prevView;

        private void AddRangeViewsInUse()
        {
            lock (_viewsInUseLocker)
            {
                var views = new View[] { _currentView, _nextView, _prevView };

                _viewsGestureCounter[_gestureId] = views;

                foreach (var view in views.Where(v => v != null))
                {
                    _viewsInUse.Add(view);
                }
            }
        }

        private void InitProcessor(View view, PanItemPosition panItemPosition)
        => (panItemPosition == PanItemPosition.Current
            ? FrontViewProcessor
            : BackViewProcessor).InitView(view, this, panItemPosition);

        private void RemoveRangeViewsInUse(Guid gestureId)
        {
            lock (_viewsInUseLocker)
            {
                var views = _viewsGestureCounter[gestureId];
                _viewsGestureCounter.Remove(gestureId);
                foreach (var view in views.ToArray())
                {
                    _viewsInUse.Remove(view);
                }

                if(_gestureId != gestureId)
                {
                    foreach (var view in views.ToArray())
                    {
                        if (view != null)
                        {
                            view.IsVisible = false;
                            ClearBindingContext(view);
                        }
                    }
                }
            }
        }

        private int GetRecycledIndex(int index)
        {
            if(_itemsCount <= 0)
            {
                return -1;
            }

            if(index < 0)
            {
                while(index < 0)
                {
                    index += _itemsCount;
                }
                return index;
            }

            while(index >= _itemsCount)
            {
                index -= _itemsCount;
            }
            return index;
        }

        private void FirePanStarted()
        {
            PanStarted?.Invoke(this, CurrentIndex, 0);
            PanStartedCommand?.Execute(CurrentIndex);
        }

        private void FirePanEnding(bool? isNextSelected, int index, double diff)
        {
            PanEnding?.Invoke(this, index, diff);
            PanEndingCommand?.Execute(index);
            if (isNextSelected.HasValue)
            {
                FirePositionChanging(isNextSelected.GetValueOrDefault());
            }

            CurrentDiff = 0;
        }

        private void FirePanEnded(bool? isNextSelected, int index, double diff)
        {
            PanEnded?.Invoke(this, index, diff);
            PanEndedCommand?.Execute(index);
            if(isNextSelected.HasValue)
            {
                FirePositionChanged(isNextSelected.GetValueOrDefault());
            }
        }

        private void FirePanChanged()
        {
            PanChanged?.Invoke(this, CurrentDiff);
            PanChangedCommand?.Execute(CurrentDiff);
        }

        public void FirePositionChanging(bool isNextSelected)
        {
            PositionChanging?.Invoke(this, isNextSelected);
            PositionChangingCommand?.Execute(isNextSelected);
        }

        private void FirePositionChanged(bool isNextSelected)
        {
            PositionChanged?.Invoke(this, isNextSelected);
            PositionChangedCommand?.Execute(isNextSelected);
        }
    }

    internal struct TimeDiffItem
    {
        public DateTime Time { get; set; }
        public double Diff { get; set; }
    }
}
