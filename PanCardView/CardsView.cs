// 01(c) Andrei Misiukevich
using System;
using Xamarin.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Specialized;
using Xamarin.Forms.Internals;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PanCardView
{
    public delegate void CardsViewPanStartEndHandler(CardsView view, int index, double diff);
    public delegate void CardsViewPanChangedHandler(CardsView view, double diff);
    public delegate void CardsViewIndexChangedHandler(CardsView view, int index);

    public class CardsView : AbsoluteLayout
    {
        public event CardsViewPanStartEndHandler PanStarted;
        public event CardsViewPanStartEndHandler PanEnded;
        public event CardsViewPanChangedHandler PanChanged;
        public event CardsViewIndexChangedHandler IndexChanged;

        public static readonly BindableProperty CurrentIndexProperty = BindableProperty.Create(nameof(CurrentIndex), typeof(int), typeof(CardsView), 0, BindingMode.TwoWay, propertyChanged: (bindable, oldValue, newValue) => {
            var view = bindable.AsCardView();
            if(view.ShouldIgnoreSetCurrentView)
            {
                view.ShouldIgnoreSetCurrentView = false;
                return;
            }
            view.SetCurrentView();
        });

        public static readonly BindableProperty ItemsProperty = BindableProperty.Create(nameof(Items), typeof(IList<object>), typeof(CardsView), null, propertyChanged: (bindable, oldValue, newValue) => {
            bindable.AsCardView().SetItemsCount();
            bindable.AsCardView().SetCurrentView();
        });

        public static readonly BindableProperty ItemViewFactoryProperty = BindableProperty.Create(nameof(ItemViewFactory), typeof(CardViewItemFactory), typeof(CardsView), null, propertyChanged: (bindable, oldValue, newValue) => {
            bindable.AsCardView().InitView();
        });

        public static readonly BindableProperty MoveDistanceProperty = BindableProperty.Create(nameof(MoveDistance), typeof(double), typeof(CardsView), -1.0);

        public static readonly BindableProperty IsOnlyForwardDirectionProperty = BindableProperty.Create(nameof(IsOnlyForwardDirection), typeof(bool), typeof(CardsView), false);

        public static readonly BindableProperty PanStartedCommandProperty = BindableProperty.Create(nameof(PanStartedCommand), typeof(ICommand), typeof(CardsView), null);

        public static readonly BindableProperty PanEndedCommandProperty = BindableProperty.Create(nameof(PanEndedCommand), typeof(ICommand), typeof(CardsView), null);

        public static readonly BindableProperty PanChangedCommandProperty = BindableProperty.Create(nameof(PanChangedCommand), typeof(ICommand), typeof(CardsView), null);

        public static readonly BindableProperty IndexChangedCommandProperty = BindableProperty.Create(nameof(IndexChangedCommand), typeof(ICommand), typeof(CardsView), null);

        private readonly Dictionary<CardViewFactoryRule, List<View>> _viewsPool = new Dictionary<CardViewFactoryRule, List<View>>();
        private readonly List<View> _viewsInUse = new List<View>();

        private readonly object _childLocker = new object();
        private View _currentView;
        private View _nextView;
        private View _prevView;
        private View _currentBackView;

        private INotifyCollectionChanged _currentObservableCollection;

        private int _itemsCount;
        private bool _isPanRunning;
        private bool _isPanEndRequested = true;

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

        public IList<object> Items 
        {
            get => GetValue(ItemsProperty) as IList<object>;
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

        public ICommand IndexChangedCommand
        {
            get => GetValue(IndexChangedCommandProperty) as ICommand;
            set => SetValue(IndexChangedCommandProperty, value);
        }

        public void OnPanUpdated(object sender, PanUpdatedEventArgs e)
        {
            if (Items == null || !Items.Any())
            {
                return;
            }

            switch (e.StatusType)
            {
                case GestureStatus.Started:
                    OnTouchStarted();
                    break;
                case GestureStatus.Running:
                    OnTouchChanged(e.TotalX);
                    break;
                case GestureStatus.Canceled:
                case GestureStatus.Completed:
                    OnTouchEnded();
                    break;
            }
        }

        private void SetCurrentView()
        {
            if (Items != null && CurrentIndex < _itemsCount)
            {
                _currentView = GetView(CurrentIndex, _currentView, FrontViewProcessor);
            }
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
            FirePanStarted();
            _isPanRunning = true;
            _isPanEndRequested = false; 
            if(_currentBackView != null)
            {
                ViewExtensions.CancelAnimations(_currentBackView);
            }

            SetupBackViews(false);
        }

        private void OnTouchChanged(double diff)
        {
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

            if(invisibleView != null)
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
            if(_isPanEndRequested)
            {
                return;
            }
            _isPanEndRequested = true; 
            var absDiff = Math.Abs(CurrentDiff);

            if (absDiff > MoveDistance)
            {
                SwapViews();
                ShouldIgnoreSetCurrentView = true;
                if (IsOnlyForwardDirection)
                {
                    CurrentIndex += 1;
                }

                var indexDelta = -Math.Sign(CurrentDiff);
                if (IsOnlyForwardDirection)
                {
                    indexDelta = Math.Abs(indexDelta);
                }
                CurrentIndex += indexDelta;

                FirePanEnded(true);

                await Task.WhenAll( //current view and backview were swapped
                    FrontViewProcessor.HandlePanApply(_currentBackView),
                    BackViewProcessor.HandlePanApply(_currentView)
                );
            }
            else
            {
                FirePanEnded(false);
                await Task.WhenAll(
                    FrontViewProcessor.HandlePanReset(_currentView),
                    BackViewProcessor.HandlePanReset(_currentBackView)
                );
            }
            CurrentDiff = 0;

            _isPanRunning = false;

            if(ShouldSetIndexAfterPan)
            {
                ShouldSetIndexAfterPan = false;
                SetNewIndex();
            }

            SetupBackViews(true);
        }

        private void InitView()
        {
            foreach(var child in Children.ToArray())
            {
                RemoveChild(child);
            }

            SetCurrentView();
        }

        private void SetupBackViews(bool isOnEndTouchAction)
        {
            var nextIndex = CurrentIndex + 1;
            var prevIndex = IsOnlyForwardDirection 
                ? nextIndex 
                : CurrentIndex - 1;

            if (isOnEndTouchAction)
            {
                ClearBindingContext(_nextView);
                ClearBindingContext(_prevView);
            }

            _nextView = GetView(nextIndex, _nextView, BackViewProcessor);
            _prevView = GetView(prevIndex, _prevView, BackViewProcessor);

            SetBackViewLayerPosition(_nextView);
            SetBackViewLayerPosition(_prevView);

            _viewsInUse.Clear();
            if (!isOnEndTouchAction)
            {
                AddRangeViewsInUse(_currentView, _nextView, _prevView);
            }

            if (!isOnEndTouchAction)
            {
                foreach (var child in Children.Where(ShouldBeRemoved).ToArray())
                {
                    RemoveChild(child);
                }
            }
        }

        private void SwapViews()
        {
            var view = _currentView;
            _currentView = _currentBackView;
            _currentBackView = view;

            _nextView = null;
            _prevView = null;
        }

        private View GetView(int index, View oldView, ICardProcessor processor)
        {
            if(index < 0 || index >= _itemsCount)
            {
                return null;
            }

            var context = Items[index];

            if(oldView?.BindingContext == context)
            {
                return oldView;
            }

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
            var view = viewsList.FirstOrDefault(v => v.BindingContext == context) 
                                ?? viewsList.FirstOrDefault(v => v.BindingContext == null && !_viewsInUse.Contains(v));

            if(view == null)
            {
                view = rule.Creator.Invoke();
                viewsList.Add(view);
            }

            processor.InitView(view);
            view.BindingContext = context;

            SetupLayout(view);

            AddChild(view, 0);

            if (oldView != null && oldView != view)
            {
                oldView.BindingContext = null;
            }

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
                    RemoveChild(view);
                    AddChild(view, 0);
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
                index = Items.IndexOf(item => item == _currentView.BindingContext);
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
                if (index < 0)
                {
                    Children.Add(view);
                    return;
                }
                Children.Insert(index, view);
            }
        }

        private void RemoveChild(View view)
        {
            lock (_childLocker)
            {
                Children.Remove(view);
                view.BindingContext = null;
            }
        }

        private bool ShouldBeRemoved(View view)
        => view != _currentView && view != _nextView && view != _prevView;

        private void AddRangeViewsInUse(params View[] views)
        => _viewsInUse.AddRange(views.Where(v => v != null));

        private void FirePanStarted()
        {
            PanStarted?.Invoke(this, CurrentIndex, 0);
            PanStartedCommand?.Execute(CurrentIndex);
        }

        private void FirePanEnded(bool isIndexChanged)
        {
            PanEnded?.Invoke(this, CurrentIndex, CurrentDiff);
            PanEndedCommand?.Execute(CurrentIndex);
            if(isIndexChanged)
            {
                FireIndexChanged();
            }
        }

        private void FirePanChanged()
        {
            PanChanged?.Invoke(this, CurrentDiff);
            PanChangedCommand?.Execute(CurrentDiff);
        }

        private void FireIndexChanged()
        {
            IndexChanged?.Invoke(this, CurrentIndex);
            IndexChangedCommand?.Execute(CurrentIndex);
        }
    }
}
