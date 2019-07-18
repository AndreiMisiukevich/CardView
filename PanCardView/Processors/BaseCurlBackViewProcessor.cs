using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PanCardView.Enums;
using Xamarin.Forms;

namespace PanCardView.Processors
{
    public class BaseCurlBackViewProcessor : ICardBackViewProcessor
    {
        public Task HandleAutoNavigate(IEnumerable<View> views, CardsView cardsView, AnimationDirection animationDirection, IEnumerable<View> inactiveViews)
        {

            return Task.CompletedTask;
        }

        public void HandleCleanView(IEnumerable<View> views, CardsView cardsView)
        {
            
        }

        public void HandleInitView(IEnumerable<View> views, CardsView cardsView, AnimationDirection animationDirection)
        {
            
        }

        public Task HandlePanApply(IEnumerable<View> views, CardsView cardsView, AnimationDirection animationDirection, IEnumerable<View> inactiveViews)
            => Task.CompletedTask;

        public void HandlePanChanged(IEnumerable<View> views, CardsView cardsView, double xPos, AnimationDirection animationDirection, IEnumerable<View> inactiveViews)
        {
        }

        public Task HandlePanReset(IEnumerable<View> views, CardsView cardsView, AnimationDirection animationDirection, IEnumerable<View> inactiveViews)
            => Task.CompletedTask;
    }
}
