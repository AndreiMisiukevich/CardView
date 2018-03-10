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

		public readonly BindableProperty SelectedIndicatorStyleProperty = BindableProperty.Create(nameof(SelectedIndicatorStyle), typeof(Style), typeof(IndicatorsControl), null, propertyChanged: (bindable, oldValue, newValue) =>
		{
			bindable.AsIndicatorsControl().ResetIndicatorsStyles();
		});

		public readonly BindableProperty UnselectedIndicatorStyleProperty = BindableProperty.Create(nameof(UnselectedIndicatorStyle), typeof(Style), typeof(IndicatorsControl), null, propertyChanged: (bindable, oldValue, newValue) =>
		{
			bindable.AsIndicatorsControl().ResetIndicatorsStyles();
		});


        public IndicatorsControl()
        {
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

		public Style SelectedIndicatorStyle
		{
			get => (GetValue(SelectedIndicatorStyleProperty) as Style) ?? Styles.DefaultSelectedIndicatorItemStyle;
			set => SetValue(SelectedIndicatorStyleProperty, value);
		}

		public Style UnselectedIndicatorStyle
		{
			get => (GetValue(UnselectedIndicatorStyleProperty) as Style) ?? Styles.DefaultUnselectedIndicatorItemStyle;
			set => SetValue(UnselectedIndicatorStyleProperty, value);
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
        => new IndicatorItemView();

		protected virtual void ApplySelectedStyle(View view, int index)
		=> view.Style = SelectedIndicatorStyle;

		protected virtual void ApplyUnselectedStyle(View view, int index)
		=> view.Style = UnselectedIndicatorStyle;

        private void ApplyStyle(View view, int recycledIndex)
        {
            try
            {
                view.BatchBegin();
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
			try
			{
				BatchBegin();
				var recycledIndex = CurrentIndex.ToRecycledIndex(IndicatorsCount);
				foreach (var child in Children)
				{
					ApplyStyle(child, recycledIndex);
				}
			}
			finally
			{
				BatchCommit();
			}
        }

		private void ResetIndicatorsCount(int oldValue, int newValue)
		{
			try
			{
				BatchBegin();
				if (oldValue < 0)
				{
					oldValue = 0;
				}

				if (oldValue > newValue)
				{
					foreach (var view in Children.Where((v, i) => i >= newValue).ToArray())
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