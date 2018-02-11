// 182(c) Andrei Misiukevich
using System;
namespace PanCardView
{
    public interface ICardItem
    {
        void HandleTouchStarted();
        bool HandeTouchChanged(double totalX, double totalY);
        void HandleTouchEnded();
    }
}
