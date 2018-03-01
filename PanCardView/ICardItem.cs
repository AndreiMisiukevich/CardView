// 182(c) Andrei Misiukevich
using System;
namespace PanCardView
{
    public interface ICardItem
    {
        void HandleTouchStarted(Guid touchId);
        bool HandeTouchChanged(double totalX, double totalY);
        void HandleTouchEnded(Guid touchId);
    }
}
