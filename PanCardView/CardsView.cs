// 01(c) Andrei Misiukevich
using System;
using Xamarin.Forms;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Xamarin.Forms.Internals;

namespace PanCardView
{
    public class CardsView : AbsoluteLayout
    {
        private const double Rad = 57.2957795;
        private const int AnimationLength = 150;

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

        public static readonly BindableProperty NextViewScaleProperty = BindableProperty.Create(nameof(NextViewScale), typeof(double), typeof(CardsView), 0.8);

        private readonly Dictionary<CardViewFactoryRule, List<View>> _viewsPool = new Dictionary<CardViewFactoryRule, List<View>>();

        private readonly object _childLocker = new object();
        private View _currentView;
        private View _nextView;
        private View _prevView;
        private View _currentBackView;

        private INotifyCollectionChanged _currentObservableCollection;

        private int _itemsCount;
        private double _currentDiff;
        private bool _isPanRunning;
        private bool _isPanEndRequested = true;

        public CardsView()
        {
            var panGesture = new PanGestureRecognizer();
            panGesture.PanUpdated += OnPanUpdated;
            GestureRecognizers.Add(panGesture);
        }

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

        public double NextViewScale
        {
            get => (double)GetValue(NextViewScaleProperty);
            set => SetValue(NextViewScaleProperty, value);
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
                _currentView = GetView(CurrentIndex, _currentView);
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

            _currentView.TranslationX = diff;
            _currentView.Rotation = 0.3 * Math.Min(diff / Width, 1) * Rad;

            _currentDiff = diff;

            var calcScale = NextViewScale + Math.Abs((_currentDiff / MoveDistance) * (1 - NextViewScale));
            _currentBackView.Scale = Math.Min(calcScale, 1);
        }

        private async void OnTouchEnded()
        {
            if(_isPanEndRequested)
            {
                return;
            }

            _isPanEndRequested = true; 
            var absDiff = Math.Abs(_currentDiff);

            if(absDiff > MoveDistance)
            {
                ResetPanPosition(_currentBackView);
                SwapViews();
                ShouldIgnoreSetCurrentView = true;
                CurrentIndex -= Math.Sign(_currentDiff);
            }
            else
            {
                ResetPanPosition(_currentView);
            }
            _currentDiff = 0;

            if (_currentBackView != null)
            {
                var backView = _currentBackView;
                await backView.FadeTo(0, AnimationLength);
                backView.IsVisible = false;
                backView.Opacity = 1;
                ResetPanPosition(backView);
            }
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

        private void SetupBackViews(bool shouldClearBindings)
        {
            var nextIndex = CurrentIndex + 1;
            var prevIndex = CurrentIndex - 1;

            if (shouldClearBindings)
            {
                ClearBindingContext(_nextView);
                ClearBindingContext(_prevView);
            }

            _nextView = GetView(nextIndex, _nextView, false, NextViewScale);
            _prevView = GetView(prevIndex, _prevView, false, NextViewScale);

            SetBackViewLayerPosition(_nextView);
            SetBackViewLayerPosition(_prevView);
            foreach (var child in Children.Where(ShouldBeRemoved).ToArray())
            {
                RemoveChild(child);
            }
        }

        private void ResetPanPosition(View view)
        {
            if (view != null)
            {
                view.TranslationX = 0;
                view.Rotation = 0;
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

        private View GetView(int index, View oldView, bool isVisible = true, double scale = 1)
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
            var view = viewsList.FirstOrDefault(v => v.BindingContext == null);

            if(view == null)
            {
                view = rule.Creator.Invoke();
                viewsList.Add(view);
            }

            view.IsVisible = isVisible;
            view.Scale = scale;
            view.BindingContext = context;

            SetupLayout(view);

            if (view != null && !Children.Contains(view))
            {
                AddChild(view, 0);
            }

            if (oldView != null && oldView != view)
            {
                RemoveChild(oldView);
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
            }
        }

        private bool ShouldBeRemoved(View view)
        => view != _currentView && view != _nextView && view != _prevView;
    }
}
