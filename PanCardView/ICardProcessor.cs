// 171(c) Andrei Misiukevich
using System;
using Xamarin.Forms;
namespace PanCardView
{
    public interface ICardProcessor
    {
        void InitView(View view);
        void HandlePanChanged(View view, double xPos);
        void HandlePanReset(View view);
        void HandlePanApply(View view);
    }
}
