using PanCardView.Enums;
using PanCardView.Extensions;
using PanCardView.Utility;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using static PanCardView.Processors.Constants;
using static System.Math;

namespace PanCardView.Processors
{
    public class CarouselProcessor: IProcessor
    {
        public uint AnimationLength { get; set; } = 300;

        public Easing AnimEasing { get; set; } = Easing.SinInOut;

        public double NoItemMaxPanDistance { get; set; } = 25;

        public double ScaleFactor { get; set; } = 1;

        public double OpacityFactor { get; set; } = 1;

        public double RotationFactor { get; set; } = 0;

        public double RotationXFactor { get; set; } = 0;

        public double RotationYFactor { get; set; } = 0;

        public void Init(CardsView cardsView, params ProcessorItem[] items)
        {
            foreach (var item in items)
            {
                if (item.IsFront)
                {
                    SetTranslationX(item.Views.FirstOrDefault(), 0, cardsView, true);
                    continue;
                }
                SetTranslationX(item.Views.FirstOrDefault(), Sign((int)item.Direction) * cardsView.GetSize(), cardsView, false);
            }
        }

        public void Change(CardsView cardsView, double xPos, params ProcessorItem[] items)
        {
            
        }

        public Task Proceed(CardsView cardsView, params ProcessorItem[] items)
        {
            
        }

        public Task Reset(CardsView cardsView, params ProcessorItem[] items)
        {
            
        }

        public void Clean(CardsView cardsView, params ProcessorItem[] items)
        {

        }

        public Task Navigate(CardsView cardsView, params ProcessorItem[] items)
        {

        }

        protected virtual void SetTranslationX(View view, double value, CardsView cardsView, bool? isVisible = null)
        {
            if (view == null)
            {
                return;
            }

            try
            {
                view.BatchBegin();
                view.Scale = CalculateFactoredProperty(value, ScaleFactor, cardsView);
                view.Opacity = CalculateFactoredProperty(value, OpacityFactor, cardsView);
                view.Rotation = CalculateFactoredProperty(value, RotationFactor, cardsView, 0) * Angle360 * Sign(-value);
                view.RotationX = CalculateFactoredProperty(value, RotationXFactor, cardsView, 0) * Angle180 * Sign(-value);
                view.RotationY = CalculateFactoredProperty(value, RotationYFactor, cardsView, 0) * Angle180 * Sign(-value);
                var translation = value - Sign(value) * cardsView.GetSize(view) * 0.5 * (1 - view.Scale);
                if (cardsView.IsHorizontalOrientation)
                {
                    view.TranslationX = translation;
                }
                else
                {
                    view.TranslationY = translation;
                }
                view.IsVisible = isVisible ?? view.IsVisible;
                cardsView.ProcessorDiff = value;
            }
            finally
            {
                view.BatchCommit();
            }
        }

        protected virtual double CalculateFactoredProperty(double value, double factor, CardsView cardsView, double defaultFactorValue = 1)
            => Abs(value) * (factor - defaultFactorValue) / cardsView.GetSize() + defaultFactorValue;
    }
}
