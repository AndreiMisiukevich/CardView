using Xamarin.Forms;
using PanCardView.Extensions;
using System.Linq;
using PanCardView.Behaviors;

namespace PanCardView.Controls
{
    public class IndicatorsControl : StackLayout
    {
        public readonly BindableProperty CurrentIndexProperty = BindableProperty.Create(nameof(CurrentIndex), typeof(int), typeof(IndicatorsControl), 0, propertyChanged: (bindable, oldValue, newValue) =>
        {
            bindable.AsIndicatorsControl().ResetIndicatorsStyles();
        });

        public readonly BindableProperty IndicatorsCountProperty = BindableProperty.Create(nameof(IndicatorsCount), typeof(int), typeof(IndicatorsControl), 0, propertyChanged: (bindable, oldValue, newValue) =>
        {
            bindable.AsIndicatorsControl().ResetIndicatorsCount((int)oldValue, (int)newValue);
        });

		protected readonly Color _indicatorColor;
		protected readonly int _indicatorSize;
		protected readonly int _indicatorRadius;

        public IndicatorsControl(Color color, int size)
        {
            _indicatorColor = color;
            _indicatorSize = size;
			_indicatorRadius = size / 2;

            Spacing = 5;
            Orientation = StackOrientation.Horizontal;
            InputTransparent = true;

            this.SetBinding(CurrentIndexProperty, nameof(CardsView.CurrentIndex));
            this.SetBinding(IndicatorsCountProperty, nameof(CardsView.ItemsCount));

            Margin = new Thickness(10, 20);
            AbsoluteLayout.SetLayoutBounds(this, new Rectangle(.5, 1, -1, -1));
            AbsoluteLayout.SetLayoutFlags(this, AbsoluteLayoutFlags.PositionProportional);

            Behaviors.Add(new ProtectedControlBehavior());
        }

        public IndicatorsControl(Color color) : this(color, 10)
        {
        }

        public IndicatorsControl(int size) : this(Color.White.MultiplyAlpha(.8), size)
        {
        }

        public IndicatorsControl() : this(Color.White.MultiplyAlpha(.8), 10)
        {
        }

        public int CurrentIndex
        {
            get => (int)GetValue(CurrentIndexProperty);
            set => SetValue(CurrentIndexProperty, value);
        }

        public int IndicatorsCount
        {
            get => (int)GetValue(IndicatorsCountProperty);
            set => SetValue(IndicatorsCountProperty, value);
        }

        protected override void OnParentSet()
        {
            base.OnParentSet();
            if(Parent is CardsView)
            {
                BindingContext = Parent;
            }
        }

        protected virtual View BuildIndicatorItem()
        => new IndicatorItemView
        {
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center,
            HasShadow = false,
            Padding = 0
        };

        protected virtual void ApplySelectedStyle(View view, int index)
        {
            view.BackgroundColor = _indicatorColor;
        }

        protected virtual void ApplyUnselectedStyle(View view, int index)
        {
            var item = view as IndicatorItemView;
            item.BackgroundColor = Color.Transparent;
            item.OutlineColor = _indicatorColor;
        }

        protected virtual void ApplyBaseStyle(View view, int index)
        {
            var item = view as IndicatorItemView;
            item.WidthRequest = _indicatorSize;
            item.HeightRequest = _indicatorSize;
			item.CornerRadius = _indicatorRadius;
        }

        private void ApplyStyle(View view, int recycledIndex)
        {
            try
            {
                view.BatchBegin();
				ApplyBaseStyle(view, recycledIndex);
                if (Children.IndexOf(view) == recycledIndex)
                {
					ApplySelectedStyle(view, recycledIndex);
                    return;
                }
                ApplyUnselectedStyle(view, recycledIndex);
            }
            finally
            {
                view.BatchCommit();
            }
        }

        private void ResetIndicatorsStyles()
        {
            var recycledIndex = CurrentIndex.ToRecycledIndex(IndicatorsCount);
            foreach (var child in Children)
            {
                ApplyStyle(child, recycledIndex);
            }
        }

        private void ResetIndicatorsCount(int oldValue, int newValue)
        {
            try
            {
                BatchBegin();
                if(oldValue < 0)
                {
                    oldValue = 0;
                }

                if (oldValue > newValue)
                {
                    foreach(var view in Children.Where((v, i) => i >= newValue).ToArray())
                    {
                        Children.Remove(view);
                    }
                    return;
                }

                for (var i = 0; i < newValue - oldValue; ++i)
                {
                    var item = BuildIndicatorItem();
                    Children.Add(item);
                    var recycledIndex = CurrentIndex.ToRecycledIndex(newValue);
                    ApplyStyle(item, recycledIndex);
                }

            }
            finally
            {
                BatchCommit();
            }
        }
    }
}