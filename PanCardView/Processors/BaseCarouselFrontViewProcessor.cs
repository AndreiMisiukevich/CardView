// 11(c) Andrei Misiukevich
using System;
using System.Threading.Tasks;
using PanCardView.Enums;
using Xamarin.Forms;

namespace PanCardView.Processors
{
    public class BaseCarouselFrontViewProcessor : ICardProcessor
    {
        public Task HandlePanApply(View view, PanItemPosition panItemPosition)
        {
            throw new NotImplementedException();
        }

        public void HandlePanChanged(View view, double xPos, PanItemPosition panItemPosition)
        {
            throw new NotImplementedException();
        }

        public Task HandlePanReset(View view, PanItemPosition panItemPosition)
        {
            throw new NotImplementedException();
        }

        public void InitView(View view, PanItemPosition panItemPosition)
        {
            throw new NotImplementedException();
        }
    }
}
