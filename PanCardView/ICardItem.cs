using System;

namespace PanCardView
{
    public interface ICardItem
    {
        bool HandleTouchStarted(Guid touchId);
        bool HandeTouchChanged(double totalX, double totalY);
        bool HandleTouchEnded(Guid touchId);
    }
}
