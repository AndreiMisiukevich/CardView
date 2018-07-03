using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace PanCardView.Processors.MultiProcessors
{
    public abstract class BaseScaleProcessor : BaseSceneProcessor
    {
        public BaseScaleProcessor()
        {
            AnimEasing = Easing.CubicInOut;
        }

        public double ScaleTuneVal = 700;

        public float InitialBackScale = 0.7f;
        public float InitialFrontScale = 1f;

        protected override IEnumerable<View> SetupViews(IEnumerable<View> views, double val)
        {
            foreach (var view in views)
                if (view != null)
                    view.Scale = val;

            return views;
        }
    }
}
