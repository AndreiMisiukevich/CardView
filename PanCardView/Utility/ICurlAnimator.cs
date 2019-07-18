using System.Collections.Generic;
using System.Threading.Tasks;
using PanCardView.Enums;
using Xamarin.Forms;

namespace PanCardView.Utility
{
    public interface ICurlAnimator
    {
        Task Process(IEnumerable<View> views, CardsView cardsView, AnimationDirection animationDirection, IEnumerable<View> inactiveViews, uint duration);
    }
}
