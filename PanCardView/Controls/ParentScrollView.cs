﻿using Xamarin.Forms;

namespace PanCardView.Controls
{
    public class ParentScrollView : ScrollView, IOrdinateHandlerParentView
    {
        private double _prevY;

        public virtual void HandleOrdinateValue(double y, bool isFirst)
        {
            if (isFirst)
            {
                _prevY = 0;
            }

            var newValue = ScrollY + _prevY - y;
            _prevY = y;
            if (Content == null ||
                newValue < 0 ||
                newValue > (Content.Height - Height))
            {
                return;
            }

            ScrollToAsync(0, newValue, false);
        }
    }
}