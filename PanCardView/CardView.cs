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
        private const double MoveDistance = 100;
        private const double NextViewScale = 0.8;

        public readonly BindableProperty CurrentIndexProperty = BindableProperty.Create(nameof(CurrentIndex), typeof(int), typeof(CardView), 0, BindingMode.TwoWay, propertyChanged: (bindable, oldValue, newValue) => {
            var view = bindable as CardView;
            view.SetCurrentView();
        });

        public readonly BindableProperty ItemsProperty = BindableProperty.Create(nameof(Items), typeof(IList<object>), typeof(CardView), null, propertyChanged: (bindable, oldValue, newValue) => {
            var view = bindable as CardView;
            view.SetCurrentView();
        });

        public readonly BindableProperty ItemTemplateProperty = BindableProperty.Create(nameof(ItemTemplate), typeof(DataTemplate), typeof(CardView), null, propertyChanged: (bindable, oldValue, newValue) => {
            var view = bindable as CardView;
            view.InitView();
        });

        private View _currentView;
        private View _nextView;

        private double _currentDiff;

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

        public DataTemplate ItemTemplate 
        {
            get => GetValue(ItemTemplateProperty) as DataTemplate;
            set => SetValue(ItemTemplateProperty, value);
        }

        private void SetCurrentView()
        {
            if (Items != null && _currentView != null && CurrentIndex < Items.Count)
            {
                _currentView.BindingContext = Items[CurrentIndex];
            }
        }

        private void AddPanGesture(View view)
        {
            SetLayoutBounds(view, new Rectangle(0, 0, 1, 1));
            SetLayoutFlags(view, AbsoluteLayoutFlags.All);

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
            Children.Insert(0, _nextView);
            _nextView.Scale = NextViewScale;
        }

        private void HandleTouch(double diff)
        {
            var ind = diff > 0 
                ? CurrentIndex - 1 
                : CurrentIndex + 1;
            
            if(ind < 0 || ind >= Items.Count)
            {
                return;
            }
            _nextView.BindingContext = Items[ind];

            _currentView.TranslationX = diff;
            _currentView.Rotation = 0.3 * Math.Min(diff / Width, 1) * Rad;

            _currentDiff = diff;

            var calcScale = NextViewScale + Math.Abs((_currentDiff / MoveDistance) * (1 - NextViewScale));
            _nextView.Scale = Math.Min(calcScale, 1);
        }

        private void HandleTouchEnd()
        {
            var absDiff = Math.Abs(_currentDiff);
            if(absDiff > MoveDistance)
            {
                ResetView(_nextView);
                SwapViews();
                CurrentIndex -= Math.Sign(_currentDiff);
            }
            else
            {
                ResetView(_currentView);
            }
            _currentDiff = 0;
            Children.Remove(_nextView);

            ResetView(_nextView);
        }

        private void InitView()
        {
            foreach(var child in Children.ToArray())
            {
                Children.Remove(child);
                child.GestureRecognizers.Clear();
            }
            AddPanGesture(_currentView = ItemTemplate.CreateContent() as View);
            AddPanGesture(_nextView = ItemTemplate.CreateContent() as View);
            Children.Add(_currentView);

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
            _currentView = _nextView;
            _nextView = view;
        }
    }
}
