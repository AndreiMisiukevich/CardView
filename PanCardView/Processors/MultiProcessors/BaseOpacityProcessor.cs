using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace PanCardView.Processors.MultiProcessors
{
    public abstract class BaseOpacityProcessor : BaseSceneProcessor
    {
        public BaseOpacityProcessor()
        {
            AnimEasing = Easing.CubicInOut;
        }

        public double OpacityTuneVal = 400;

        public float InitialBackOpacity = 0.2f;
        public float InitialFrontOpacity = 1f;

        protected override IEnumerable<View> SetupViews(IEnumerable<View> views, double val)
        {
            foreach (var view in views)
                if (view != null)
                    view.Opacity = val;

            return views;
        }
    }
}
