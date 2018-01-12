// 01(c) Andrei Misiukevich
using System;
using Xamarin.Forms;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

namespace PanCardView
{
    public class CardView : AbsoluteLayout
    {
        private const double Rad = 57.2957795;
        private const int AnimationLength = 150;

        public readonly BindableProperty CurrentIndexProperty = BindableProperty.Create(nameof(CurrentIndex), typeof(int), typeof(CardView), 0, BindingMode.TwoWay, propertyChanged: (bindable, oldValue, newValue) => {
            var view = bindable.AsCardView();
            if(view.ShouldIgnoreSetCurrentView)
            {
                view.ShouldIgnoreSetCurrentView = false;
                return;
            }
            view.SetCurrentView();
        });

        public readonly BindableProperty ItemsProperty = BindableProperty.Create(nameof(Items), typeof(IList<object>), typeof(CardView), null, propertyChanged: (bindable, oldValue, newValue) => {
            bindable.AsCardView().SetItemsCount();
            bindable.AsCardView().SetCurrentView();
        });

        public readonly BindableProperty ItemViewFactoryProperty = BindableProperty.Create(nameof(ItemViewFactory), typeof(CardViewItemFactory), typeof(CardView), null, propertyChanged: (bindable, oldValue, newValue) => {
            bindable.AsCardView().InitView();
        });

        public readonly BindableProperty MoveDistanceProperty = BindableProperty.Create(nameof(MoveDistance), typeof(double), typeof(CardView), 100.0);

        public readonly BindableProperty NextViewScaleProperty = BindableProperty.Create(nameof(NextViewScale), typeof(double), typeof(CardView), 0.8);

        private readonly Dictionary<CardViewFactoryRule, List<View>> _viewsPool = new Dictionary<CardViewFactoryRule, List<View>>();
        private View _currentView;
        private View _nextView;
        private View _prevView;
        private View _currentBackView;

        private int _itemsCount;
        private double _currentDiff;

        private bool ShouldIgnoreSetCurrentView { get; set; }

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
            get => (double)GetValue(MoveDistanceProperty);
            set => SetValue(MoveDistanceProperty, value);
        }

        public double NextViewScale
        {
            get => (double)GetValue(NextViewScaleProperty);
            set => SetValue(NextViewScaleProperty, value);
        }

        private void SetCurrentView()
        {
            if (Items != null && CurrentIndex < _itemsCount)
            {
                _currentView = GetView(CurrentIndex, _currentView);
            }
        }

        private void AddPanGesture(View view)
        {
            SetLayoutBounds(view, new Rectangle(0, 0, 1, 1));
            SetLayoutFlags(view, AbsoluteLayoutFlags.All);

            RemovePanGesture(view);
            var panGesture = new PanGestureRecognizer();
            panGesture.PanUpdated += OnPanUpdated;
            view.GestureRecognizers.Add(panGesture);
        }

        private void OnPanUpdated(object sender, PanUpdatedEventArgs e)
        {
            if(Items == null || !Items.Any())
            {
                return;
            }

            switch (e.StatusType)
            {
                case GestureStatus.Started:
                    HandleTouchStart();
                    break;
                case GestureStatus.Running:
                    HandleTouch(e.TotalX);
                    break;
                case GestureStatus.Completed:
                    HandleTouchEnd();
                    break;
            }
        }

        private void HandleTouchStart()
        {
            var nextIndex = CurrentIndex + 1;
            var prevIndex = CurrentIndex - 1;

            ClearBindingContext(_nextView);
            ClearBindingContext(_prevView);

            _nextView = GetView(nextIndex, _nextView, false, NextViewScale);
            _prevView = GetView(prevIndex, _prevView, false, NextViewScale);

            SetBackViewLayerPosition(_nextView);
            SetBackViewLayerPosition(_prevView);
            foreach(var child in Children.Where(c => c.BindingContext == null).ToArray())
            {
                Children.Remove(child);
            }
        }

        private void HandleTouch(double diff)
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

        private async void HandleTouchEnd()
        {
            var absDiff = Math.Abs(_currentDiff);
            if(absDiff > MoveDistance)
            {
                ResetView(_currentBackView);
                SwapViews();
                ShouldIgnoreSetCurrentView = true;
                CurrentIndex -= Math.Sign(_currentDiff);
            }
            else
            {
                ResetView(_currentView);
            }
            _currentDiff = 0;

            if (_currentBackView != null)
            {
                await _currentBackView.FadeTo(0, AnimationLength);
                _currentBackView.IsVisible = false;
                _currentBackView.Opacity = 1;
                _currentBackView.BindingContext = null;
                ResetView(_currentBackView);
            }
        }

        private void InitView()
        {
            foreach(var child in Children.ToArray())
            {
                Children.Remove(child);
                RemovePanGesture(child);
            }

            SetCurrentView();
        }

        private void ResetView(View view)
        {
            view.TranslationX = 0;
            view.Rotation = 0;
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

            AddPanGesture(view);

            if (view != null && !Children.Contains(view))
            {
                Children.Insert(0, view);
            }

            if (oldView != null && oldView != view)
            {
                Children.Remove(oldView);
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
                    Children.Remove(view);
                    Children.Insert(0, view);
                }
            }
        }

        private void RemovePanGesture(View view)
        {
            foreach (var gesture in view.GestureRecognizers.OfType<PanGestureRecognizer>().ToArray())
            {
                gesture.PanUpdated -= OnPanUpdated;
                view.GestureRecognizers.Remove(gesture);
            }
        }

        private void ClearBindingContext(View view)
        {
            if(view != null)
            {
                view.BindingContext = null;
            }
        }

        private void SetItemsCount() => _itemsCount = Items?.Count ?? -1;
    }
}
