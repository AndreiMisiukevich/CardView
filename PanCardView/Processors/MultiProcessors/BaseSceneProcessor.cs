using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BareBones.Navigation.Interfaces;
using iQ.Mobile.UI.Common.Interfaces;
using PanCardView;
using PanCardView.Enums;
using PanCardView.Processors;
using Xamarin.Forms;

namespace PanCardView.Processors.MultiProcessors
{
    public abstract class BaseSceneProcessor : ICardProcessor
    {
        public Easing AnimEasing { get; set; } = Easing.SinInOut;

        public uint AnimationLength { get; set; } = 300;

        public static double ConvertRange(double originalStart, double originalEnd, // original range
                                          double newStart, double newEnd, // desired range
                                          double value) // value to convert
        {
            double scale = (newEnd - newStart) / (originalEnd - originalStart);
            return (newStart + ((value - originalStart) * scale));
        }

        protected virtual IEnumerable<View> SetupViews(IEnumerable<View> views, double val)
        {
            return views;
        }

        public abstract Task HandleAutoNavigate(IEnumerable<View> views, CardsView cardsView, AnimationDirection animationDirection, IEnumerable<View> inactiveViews);
        public abstract void HandleInitView(IEnumerable<View> views, CardsView cardsView, AnimationDirection animationDirection);
        public abstract Task HandlePanApply(IEnumerable<View> views, CardsView cardsView, AnimationDirection animationDirection, IEnumerable<View> inactiveViews);
        public abstract void HandlePanChanged(IEnumerable<View> views, CardsView cardsView, double xPos, AnimationDirection animationDirection, IEnumerable<View> inactiveViews);
        public abstract Task HandlePanReset(IEnumerable<View> views, CardsView cardsView, AnimationDirection animationDirection, IEnumerable<View> inactiveViews);
    }
}

