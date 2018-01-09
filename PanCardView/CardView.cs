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

        public readonly BindableProperty ItemsProperty = BindableProperty.Create(nameof(Items), typeof(IList<object>), typeof(CardView), null, propertyChanged: (bindable, oldValue, newValue) => {
            var view = bindable as CardView;
            view.SetCurrentView();
        });

        private View _currentView;
        private View _nextView;
        private int _currentIndex;

        private double _currentDiff;

        public IList<object> Items 
        {
            get => GetValue(ItemsProperty) as IList<object>;
            set => SetValue(ItemsProperty, value);
        }

        public CardView(Func<View> viewFactory)
        {
            AddPanGesture(_currentView = viewFactory.Invoke());
            AddPanGesture(_nextView = viewFactory.Invoke());

            Children.Add(_currentView);
        }

        private void SetCurrentView()
        {
            if (Items != null && _currentIndex < Items.Count)
            {
                _currentView.BindingContext = Items[_currentIndex];
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
                ? _currentIndex - 1 
                : _currentIndex + 1;
            
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
                InitView(_nextView);
                SwapViews();
                _currentIndex -= Math.Sign(_currentDiff);
            }
            else
            {
                InitView(_currentView);
            }
            _currentDiff = 0;
            Children.Remove(_nextView);

            InitView(_nextView);
        }

        private void InitView(View view)
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
