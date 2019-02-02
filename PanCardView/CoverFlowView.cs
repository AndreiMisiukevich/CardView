using System.Threading.Tasks;
using PanCardView.Extensions;
using PanCardView.Processors;
using Xamarin.Forms;

namespace PanCardView
{
    public class CoverFlowView : CarouselView
    {
        public static readonly BindableProperty PositionShiftPercentageProperty = BindableProperty.Create(nameof(PositionShiftPercentage), typeof(double), typeof(CoverFlowView), 1.0, propertyChanged: (bindable, oldValue, newValue) =>
        {
            bindable.AsCardsView().SetCurrentView();
        });

        public static readonly BindableProperty PositionShiftValueProperty = BindableProperty.Create(nameof(PositionShiftValue), typeof(double), typeof(CoverFlowView), 0.0, propertyChanged: (bindable, oldValue, newValue) =>
        {
            bindable.AsCardsView().SetCurrentView();
        });

        public CoverFlowView() : this(new BaseCoverFlowFrontViewProcessor(), new BaseCoverFlowBackViewProcessor())
        {
        }

        public CoverFlowView(ICardProcessor frontViewProcessor, ICardBackViewProcessor backViewProcessor) : base(frontViewProcessor, backViewProcessor)
        {
        }

        public double PositionShiftPercentage
        {
            get => (double)GetValue(PositionShiftPercentageProperty);
            set => SetValue(PositionShiftPercentageProperty, value);
        }

        public double PositionShiftValue
        {
            get => (double)GetValue(PositionShiftValueProperty);
            set => SetValue(PositionShiftValueProperty, value);
        }

        protected override int DefaultBackViewsDepth => 2;

        protected override int DefaultMaxChildrenCount => 17;

        protected override int DefaultDesiredMaxChildrenCount => 12;

        protected override Task<bool> TryAutoNavigate()
        => Task.FromResult(false);
    }
}
