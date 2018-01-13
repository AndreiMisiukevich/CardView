// 171(c) Andrei Misiukevich
using System;
using Xamarin.Forms;

namespace PanCardView
{
    public class BaseBackViewProcessor : ICardProcessor
    {
        private const double InitialScale = 0.8;

        public virtual void InitView(View view)
        {
            if(view != null)
            {
                view.IsVisible = false;
                view.Scale = InitialScale;
            }
        }

        public virtual void HandlePanChanged(View view, double xPos)
        {
            var parent = view.Parent as CardsView;
            var calcScale = InitialScale + Math.Abs((xPos / parent.MoveDistance) * (1 - InitialScale));
            view.Scale = Math.Min(calcScale, 1);
        }

        public virtual void HandlePanReset(View view)
        {
            if(view != null)
            {
                view.Scale = InitialScale;
            }
        }

        public virtual void HandlePanApply(View view)
        {
            view.Scale = 1;
        }
    }
}
