using System.Collections.Specialized;
using System.Threading.Tasks;
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
            bindable.AsCardsView().SetCurrentView();
        });

        public static readonly BindableProperty PositionShiftValueProperty = BindableProperty.Create(nameof(PositionShiftValue), typeof(double), typeof(CoverFlowView), .0, propertyChanged: (bindable, oldValue, newValue) =>
        {
            bindable.AsCardsView().SetCurrentView();
        });

        private bool _shouldForceHardSetCurrentView;

        public CoverFlowView() : this(new BaseCoverFlowFrontViewProcessor(), new BaseCoverFlowBackViewProcessor())
        {
        }

        public CoverFlowView(ICardProcessor frontViewProcessor, ICardBackViewProcessor backViewProcessor) : base(frontViewProcessor, backViewProcessor)
        {
        }

        /// <summary>
        /// Shift to current view percentage (percengate of CoverFlowView.Width)
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
                oldIndex = oldIndex.ToCyclingIndex(ItemsCount);
                index = index.ToCyclingIndex(ItemsCount);
            }

            var hasFirstElement = Min(oldIndex, index) == 0;
            var hasLastElement = Max(oldIndex, index) == ItemsCount - 1;

            if (Abs(index - oldIndex) > 1 && (!isCyclical || !hasFirstElement || !hasLastElement))
            {
                return true;
            }

            return false;
        }
    }
}