using System;
using System.Collections.Generic;
using PanCardView;
using Xamarin.Forms;

namespace PanCardView.Processors.MultiProcessors
{
    public abstract class BaseTranslationProcessor : BaseSceneProcessor
    {
        public BaseTranslationProcessor()
        {
            AnimEasing = Easing.CubicInOut;
        }

        public double InitialBackPositionPercentage { get; set; } = .8;

        protected double GetInitialPosition(CardsView cardsView, int index)
        => cardsView.Width * InitialBackPositionPercentage * (index + 1);

        protected double GetInitialPosition(CardsView cardsView)
        => cardsView.Width * InitialBackPositionPercentage;

        protected override IEnumerable<View> SetupViews(IEnumerable<View> views, double val)
        {
            foreach (var view in views)
                if (view != null)
                    view.TranslationX = val;

            return views;
        }
    }
}
