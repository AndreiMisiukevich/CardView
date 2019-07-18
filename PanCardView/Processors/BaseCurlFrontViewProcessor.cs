using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PanCardView.Enums;
using Xamarin.Forms;

namespace PanCardView.Processors
{
    public class BaseCurlFrontViewProcessor : ICardProcessor
    {
        public Task HandleAutoNavigate(IEnumerable<View> views, CardsView cardsView, AnimationDirection animationDirection, IEnumerable<View> inactiveViews)
        {
            throw new NotImplementedException();
        }

        public void HandleInitView(IEnumerable<View> views, CardsView cardsView, AnimationDirection animationDirection)
        {
            throw new NotImplementedException();
        }

        public Task HandlePanApply(IEnumerable<View> views, CardsView cardsView, AnimationDirection animationDirection, IEnumerable<View> inactiveViews)
        {
            throw new NotImplementedException();
        }

        public void HandlePanChanged(IEnumerable<View> views, CardsView cardsView, double xPos, AnimationDirection animationDirection, IEnumerable<View> inactiveViews)
        {
            throw new NotImplementedException();
        }

        public Task HandlePanReset(IEnumerable<View> views, CardsView cardsView, AnimationDirection animationDirection, IEnumerable<View> inactiveViews)
        {
            throw new NotImplementedException();
        }
    }
}
