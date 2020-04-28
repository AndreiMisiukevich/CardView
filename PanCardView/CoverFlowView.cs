using System;
using System.Collections.Specialized;
using System.ComponentModel;
using PanCardView.Extensions;
using PanCardView.Processors;
using Xamarin.Forms;
using static System.Math;

namespace PanCardView
{
    public class CoverFlowView : CarouselView
    {
        public static readonly BindableProperty PositionShiftPercentageProperty = BindableProperty.Create(nameof(PositionShiftPercentage), typeof(double), typeof(CoverFlowView), .0, propertyChanged: (bindable, oldValue, newValue) =>
        {
            bindable.AsCardsView().ForceRedrawViews();
        });

        public static readonly BindableProperty PositionShiftValueProperty = BindableProperty.Create(nameof(PositionShiftValue), typeof(double), typeof(CoverFlowView), .0, propertyChanged: (bindable, oldValue, newValue) =>
        {
            bindable.AsCardsView().ForceRedrawViews();
        });

        private bool _shouldForceHardSetCurrentView;

        public CoverFlowView() : this(new CarouselProcessor())
        {
        }

        public CoverFlowView(IProcessor processor) : base(processor)
        {
        }

        [Obsolete]
        public CoverFlowView(ICardProcessor frontViewProcessor, ICardBackViewProcessor backViewProcessor)
            : base(frontViewProcessor ?? new BaseCoverFlowFrontViewProcessor(), backViewProcessor ?? new BaseCoverFlowBackViewProcessor())
        {
        }

        /// <summary>
        /// Shift to current view percentage (percengate of CoverFlowView Size)
        /// </summary>
        /// <value>The position shift value.</value>
        public double PositionShiftPercentage
        {
            get => (double)GetValue(PositionShiftPercentageProperty);
            set => SetValue(PositionShiftPercentageProperty, value);
        }

        /// <summary>
        /// Shift to current view in absolute points
        /// </summary>
        /// <value>The position shift value.</value>
        public double PositionShiftValue
        {
            get => (double)GetValue(PositionShiftValueProperty);
            set => SetValue(PositionShiftValueProperty, value);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public new static void Preserve()
        {
        }

        protected override int DefaultBackViewsDepth => 2;

        protected override int DefaultMaxChildrenCount => 17;

        protected override int DefaultDesiredMaxChildrenCount => 12;

        protected override void OnObservableCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (sender != null)
            {
                _shouldForceHardSetCurrentView = true;
            }
            base.OnObservableCollectionChanged(sender, e);
        }

        protected override bool CheckIsHardSetCurrentView()
        {
            if (_shouldForceHardSetCurrentView)
            {
                _shouldForceHardSetCurrentView = false;
                return true;
            }

            var oldIndex = OldIndex;
            var index = SelectedIndex;
            var isCyclical = IsCyclical;

            if (isCyclical)
            {
                oldIndex = oldIndex.ToCyclicalIndex(ItemsCount);
                index = index.ToCyclicalIndex(ItemsCount);
            }

            var hasFirstElement = Min(oldIndex, index) == 0;
            var hasLastElement = Max(oldIndex, index) == ItemsCount - 1;

            return Abs(index - oldIndex) > 1 && (!isCyclical || !hasFirstElement || !hasLastElement);
        }
    }
}