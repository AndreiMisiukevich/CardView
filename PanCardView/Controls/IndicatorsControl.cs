using Xamarin.Forms;
using PanCardView.Extensions;
using System.Linq;

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

        private readonly Color _color;
        private readonly int _size;

        public IndicatorsControl(Color color, int size)
        {
            _color = color;
            _size = size;

            Spacing = 5;
            Orientation = StackOrientation.Horizontal;
            InputTransparent = true;

            this.SetBinding(CurrentIndexProperty, nameof(CardsView.CurrentIndex));
            this.SetBinding(IndicatorsCountProperty, nameof(CardsView.ItemsCount));

            AbsoluteLayout.SetLayoutBounds(this, new Rectangle(.5, 1, -1, size + 20));
            AbsoluteLayout.SetLayoutFlags(this, AbsoluteLayoutFlags.PositionProportional);
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
        => new Frame
        {
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center,
            HasShadow = false,
            Padding = 0
        };

        protected virtual void ApplyStyle(View view, int recycledIndex)
        {
            var item = view as Frame;
            try
            {
                item.BatchBegin();

                item.WidthRequest = _size;
                item.HeightRequest = _size;
                item.CornerRadius = _size / 2;

                if (Children.IndexOf(item) == recycledIndex)
                {
                    item.BackgroundColor = _color;
                    return;
                }
                item.BackgroundColor = Device.RuntimePlatform == Device.iOS
                    ? Color.Transparent
                    : Color.White.MultiplyAlpha(.4);
                item.OutlineColor = _color;
            }
            finally
            {
                item.BatchCommit();
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