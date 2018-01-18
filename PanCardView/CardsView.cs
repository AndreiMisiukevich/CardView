using System;
using Xamarin.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Specialized;
using Xamarin.Forms.Internals;
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
        public event CardsViewPanStartEndHandler PanEnded;
        public event CardsViewPanChangedHandler PanChanged;
        public event CardsViewPositionChangedHandler PositionChanged;

        public static readonly BindableProperty CurrentIndexProperty = BindableProperty.Create(nameof(CurrentIndex), typeof(int), typeof(CardsView), 0, BindingMode.TwoWay, propertyChanged: (bindable, oldValue, newValue) => {
            var view = bindable.AsCardView();
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

        public static readonly BindableProperty ItemViewFactoryProperty = BindableProperty.Create(nameof(ItemViewFactory), typeof(CardViewItemFactory), typeof(CardsView), null, propertyChanged: (bindable, oldValue, newValue) => {
            bindable.AsCardView().SetCurrentView();
        });

        public static readonly BindableProperty MoveDistanceProperty = BindableProperty.Create(nameof(MoveDistance), typeof(double), typeof(CardsView), -1.0);

        public static readonly BindableProperty IsOnlyForwardDirectionProperty = BindableProperty.Create(nameof(IsOnlyForwardDirection), typeof(bool), typeof(CardsView), false);

        public static readonly BindableProperty PanDelayProperty = BindableProperty.Create(nameof(PanDelay), typeof(int), typeof(CardsView), 200);

        public static readonly BindableProperty IsPanInCourseProperty = BindableProperty.Create(nameof(IsPanInCourse), typeof(bool), typeof(CardsView), false);

        public static readonly BindableProperty IsRecycledProperty = BindableProperty.Create(nameof(IsRecycled), typeof(bool), typeof(CardsView), false);

        public static readonly BindableProperty MaxChildrenCountProperty = BindableProperty.Create(nameof(MaxChildrenCount), typeof(int), typeof(CardsView), 18);

        public static readonly BindableProperty DesiredMaxChildrenCountProperty = BindableProperty.Create(nameof(DesiredMaxChildrenCount), typeof(int), typeof(CardsView), 12);

        public static readonly BindableProperty PanStartedCommandProperty = BindableProperty.Create(nameof(PanStartedCommand), typeof(ICommand), typeof(CardsView), null);

        public static readonly BindableProperty PanEndedCommandProperty = BindableProperty.Create(nameof(PanEndedCommand), typeof(ICommand), typeof(CardsView), null);

        public static readonly BindableProperty PanChangedCommandProperty = BindableProperty.Create(nameof(PanChangedCommand), typeof(ICommand), typeof(CardsView), null);

        public static readonly BindableProperty PositionChangedCommandProperty = BindableProperty.Create(nameof(PositionChangedCommand), typeof(ICommand), typeof(CardsView), null);

        private readonly Dictionary<CardViewFactoryRule, List<View>> _viewsPool = new Dictionary<CardViewFactoryRule, List<View>>();
        private readonly HashSet<View> _viewsInUse = new HashSet<View>();
        private readonly Dictionary<Guid, View[]> _viewsGestureCounter = new Dictionary<Guid, View[]>();

        private readonly object _childLocker = new object();
        private readonly object _viewsInUseLocker = new object();
        private View _currentView;
        private View _nextView;
        private View _prevView;
        private View _currentBackView;

        private INotifyCollectionChanged _currentObservableCollection;

        private int _itemsCount = -1;
        private int _viewsChildrenCount;
        private bool _isPanRunning;
        private bool _isPanEndRequested = true;
        private Guid _gestureId;
        private DateTime _lastPanTime;
        private int _inCoursePanDelay;

        private bool _shouldSkipTouch;

        public CardsView() : this(null, null)
        {
        }

        public CardsView(ICardProcessor frontViewProcessor, ICardProcessor backViewProcessor)
        {
            FrontViewProcessor = frontViewProcessor ?? new BaseFrontViewProcessor();
            BackViewProcessor = backViewProcessor ?? new BaseBackViewProcessor();
            var panGesture = new PanGestureRecognizer();
            panGesture.PanUpdated += OnPanUpdated;
            GestureRecognizers.Add(panGesture);
        }

        public double CurrentDiff { get; private set; }

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

        public CardViewItemFactory ItemViewFactory 
        {
            get => GetValue(ItemViewFactoryProperty) as CardViewItemFactory;
            set => SetValue(ItemViewFactoryProperty, value);
        }

        public double MoveDistance
        {
            get
            {
                var dist = (double)GetValue(MoveDistanceProperty);
                return dist > 0
                        ? dist
                        : Width * 0.35;
            }
            set => SetValue(MoveDistanceProperty, value);
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

        public ICommand PositionChangedCommand
        {
            get => GetValue(PositionChangedCommandProperty) as ICommand;
            set => SetValue(PositionChangedCommandProperty, value);
        }

        public void OnPanUpdated(object sender, PanUpdatedEventArgs e)
        {
            if (_itemsCount < 0)
            {
                return;
            }

            switch (e.StatusType)
            {
                case GestureStatus.Started:
                    if (Device.RuntimePlatform != Device.Android || e.GestureId == -1)
                    {
                        OnTouchStarted();
                    }
                    break;
                case GestureStatus.Running:
                    OnTouchChanged(e.TotalX);
                    break;
                case GestureStatus.Canceled:
                case GestureStatus.Completed:
                    if (Device.RuntimePlatform != Device.Android || e.GestureId == -1)
                    {
                        OnTouchEnded();
                    }
                    break;
            }
        }

        private void SetCurrentView()
        {
            if (Items != null && CurrentIndex < _itemsCount)
            {
                _currentView = GetView(CurrentIndex, PanItemPosition.Current, true);
            }

            SetupBackViews();
        }

        private void SetupLayout(View view)
        {
            SetLayoutBounds(view, new Rectangle(0, 0, 1, 1));
            SetLayoutFlags(view, AbsoluteLayoutFlags.All);
        }

        private void OnTouchStarted()
        {
            if(!_isPanEndRequested)
            {
                return;
            }

            var deltaTime = DateTime.Now - _lastPanTime;
            if(deltaTime.TotalMilliseconds < PanDelay)
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

            SetupBackViews();
            AddRangeViewsInUse();
        }

        private void OnTouchChanged(double diff)
        {
            if(_shouldSkipTouch)
            {
                return;
            }

            View invisibleView;
            if(diff > 0)
            {
                _currentBackView = _prevView;
                invisibleView = _nextView;
            }
            else
            {
                _currentBackView = _nextView;
                invisibleView = _prevView;
            }

            if(invisibleView != null && invisibleView != _currentBackView)
            {
                invisibleView.IsVisible = false;
            }

            if (_currentBackView == null)
            {
                return;
            }

            _currentBackView.IsVisible = true;
            CurrentDiff = diff;
            FirePanChanged();

            FrontViewProcessor.HandlePanChanged(_currentView, diff);
            BackViewProcessor.HandlePanChanged(_currentBackView, diff);
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

            if (absDiff > MoveDistance)
            {
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
                        return;
                    }

                    newIndex = GetRecycledIndex(newIndex);
                }

                SwapViews();
                ShouldIgnoreSetCurrentView = true;

                CurrentIndex = newIndex;

                FirePanEnded(CurrentDiff < 0);

                await Task.WhenAll( //current view and backview were swapped
                    FrontViewProcessor.HandlePanApply(_currentBackView),
                    BackViewProcessor.HandlePanApply(_currentView)
                );
            }
            else
            {
                FirePanEnded();
                await Task.WhenAll(
                    FrontViewProcessor.HandlePanReset(_currentView),
                    BackViewProcessor.HandlePanReset(_currentBackView)
                );
            }

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
                SetupBackViews();
            }

            var maxChildrenCount = isProcessingNow ? MaxChildrenCount : DesiredMaxChildrenCount;

            if (_viewsChildrenCount > maxChildrenCount)
            {
                RemoveChildren(Children.Where(c => c != _prevView && c != _nextView && !c.IsVisible).Take(_viewsChildrenCount - DesiredMaxChildrenCount).ToArray());
            }

            if (IsPanInCourse)
            {
                _inCoursePanDelay = 0;
            }
        }

        private void SetupBackViews()
        {
            var nextIndex = CurrentIndex + 1;
            var prevIndex = IsOnlyForwardDirection
                ? nextIndex
                : CurrentIndex - 1;
            
            _nextView = GetView(nextIndex, PanItemPosition.Next);
            _prevView = GetView(prevIndex, PanItemPosition.Prev);

            SetBackViewLayerPosition(_nextView);
            SetBackViewLayerPosition(_prevView);
        }

        private void SwapViews()
        {
            var view = _currentView;
            _currentView = _currentBackView;
            _currentBackView = view;
        }

        private View GetView(int index, PanItemPosition panIntemPosition)
        {
            if(_itemsCount < 0)
            {
                return null;
            }
            
            if(index < 0 || index >= _itemsCount)
            {
                if(!IsRecycled || (panIntemPosition != PanItemPosition.Current && _itemsCount < 2))
                {
                    return null;
                }

                index = GetRecycledIndex(index);

            }

            var context = Items[index];

            var rule = ItemViewFactory?.GetRule(context);
            if(rule == null)
            {
                return null;
            }
            List<View> viewsList;
            if (!_viewsPool.TryGetValue(rule, out viewsList))
            {
                viewsList = new List<View> 
                {
                    rule.Creator.Invoke() 
                };
                _viewsPool.Add(rule, viewsList);
            }

            var notUsingViews = viewsList.Where(v => !CheckUsingNow(v));
            var view = notUsingViews.FirstOrDefault(v => v.BindingContext == context)
                                    ?? notUsingViews.FirstOrDefault(v => v.BindingContext == null)
                                    ?? notUsingViews.FirstOrDefault(v => !CheckIsProcessingView(v));

            if(view == null)
            {
                view = rule.Creator.Invoke();
                viewsList.Add(view);
            }

            InitProcessor(view, panIntemPosition);

            view.BindingContext = context;

            SetupLayout(view);

            AddChild(view, 0);

            return view;
        }

        private void SetBackViewLayerPosition(View view)
        {
            if(view == null)
            {
                return;
            }

            if(_currentView != null)
            {
                var currentIndex = Children.IndexOf(_currentView);
                var backIndex = Children.IndexOf(view);

                if(currentIndex < backIndex)
                {
                    lock(_childLocker)
                    {
                        SendChildToBack(view);
                    }
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
                if (index < 0)
                {
                    Children.Add(view);
                    return;
                }
                Children.Insert(index, view);
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
            : BackViewProcessor).InitView(view, panItemPosition);

        private void RemoveRangeViewsInUse(Guid gestureId)
        {
            lock (_viewsInUseLocker)
            {
                var views = _viewsGestureCounter[gestureId];
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
            while (index < 0 || index >= _itemsCount)
            {
                index = Math.Abs(_itemsCount - Math.Abs(index));
            }
            return index;
        }

        private void FirePanStarted()
        {
            PanStarted?.Invoke(this, CurrentIndex, 0);
            PanStartedCommand?.Execute(CurrentIndex);
        }

        private void FirePanEnded(bool? isIndexChanged = null)
        {
            PanEnded?.Invoke(this, CurrentIndex, CurrentDiff);
            PanEndedCommand?.Execute(CurrentIndex);
            if(isIndexChanged.HasValue)
            {
                FirePositionChanged(isIndexChanged.GetValueOrDefault());
            }
            CurrentDiff = 0;
        }

        private void FirePanChanged()
        {
            PanChanged?.Invoke(this, CurrentDiff);
            PanChangedCommand?.Execute(CurrentDiff);
        }

        private void FirePositionChanged(bool isNextSelected)
        {
            PositionChanged?.Invoke(this, isNextSelected);
            PositionChangedCommand?.Execute(isNextSelected);
        }
    }
}
