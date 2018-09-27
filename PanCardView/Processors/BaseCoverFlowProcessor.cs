using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using PanCardView.Enums;
using PanCardView.Utility;
using Xamarin.Forms;
using static Xamarin.Forms.AbsoluteLayout;

namespace PanCardView.Processors
{
    public class BaseCoverFlowProcessor
    {
        public CoverFlow CoverFlow;
        public uint AnimationLength { get; set; } = 300;

        public Easing AnimEasing { get; set; } = Easing.CubicOut;

        public BaseCoverFlowProcessor(CoverFlow coverFlow)
        {
            CoverFlow = coverFlow;
        }

        public BaseCoverFlowProcessor()
        {
        }

        public void HandleInitViews(IAbsoluteList<View> displayedViews)
        {
            var PreviousItemtranslation = 0.0;
            var translate = 0.0;
            foreach (var view in displayedViews)
            {
                if (translate > CoverFlow.MaxGraphicAxis)
                {
                    translate = -PreviousItemtranslation;
                }
                view.TranslationX = translate;
                PreviousItemtranslation = translate;
                translate = translate + CoverFlow.Space;
            }
        }

        public Task HandleAutoNavigate(List<View> views, AnimationDirection animationDirection, IEnumerable<View> inactiveViews)
        {
            throw new NotImplementedException();
        }

        public void HandlePanApply(IAbsoluteList<View> displayedViews, double dragX, AnimationDirection direction, List<View> recycledViews)
        {
            var maxTranslate = CoverFlow.MaxGraphicAxis + CoverFlow.MarginBorder;

            Animation a = new Animation();

            foreach (var v in displayedViews)
            {
                var translate = v.TranslationX + dragX;
                if (Math.Abs(translate) > maxTranslate)
                {
                    var diff = Math.Abs(translate) - maxTranslate;
                    var coef = 1 - (diff / Math.Abs(dragX));
                    a.Add(0, coef, new Animation(f => v.TranslationX = f, v.TranslationX, -(int)direction * maxTranslate, Easing.Linear, () => 
                    {
                        if (direction == AnimationDirection.Prev) // Movement --> right
                            CoverFlow.ItemMaxOnAxis = CoverFlow.VerifyIndex(CoverFlow.ItemMaxOnAxis - 1);
                        else if (direction == AnimationDirection.Next)  // Movement <-- Left
                            CoverFlow.ItemMinOnAxis = CoverFlow.VerifyIndex(CoverFlow.ItemMinOnAxis + 1);
                        v.IsVisible = false;
                        recycledViews.Add(v);

                        /* Unworking code here but have to be done
                        // Remove Views from displayed List
                        CoverFlow.RemoveUnDisplayedViews();

                        // RecyclerView
                        var recycledView = CoverFlow.RecyclerView(direction);

                        // Add animation at runtime
                        a.Add((diff / dragX), 1, new Animation(f => v.TranslationX = f, v.TranslationX, translate, Easing.Linear, null));
                        */
                    }));
                }
                else
                    a.Add(0, 1, new Animation(f => v.TranslationX = f, v.TranslationX, translate, Easing.Linear, null));
            }
            a.Commit(CoverFlow, "SimpleAnimation", 60, 800, Easing.Linear, finished: (d,b) => CoverFlow.RemoveUnDisplayedViews());
        }

        public void HandlePanChanged(IAbsoluteList<View> displayedViews, double dragX, AnimationDirection direction, List<View> recycledViews)
        {
            var maxTranslate = CoverFlow.MaxGraphicAxis + CoverFlow.MarginBorder;
            foreach (var v in displayedViews)
            {
                var translate = v.TranslationX + dragX;
                if (Math.Abs(translate) > maxTranslate
                    && displayedViews.Count() > 1)
                {
                    if (direction == AnimationDirection.Prev) // Movement --> right
                        CoverFlow.ItemMaxOnAxis = CoverFlow.VerifyIndex(CoverFlow.ItemMaxOnAxis - 1);
                    else if (direction == AnimationDirection.Next)  // Movement <-- Left
                        CoverFlow.ItemMinOnAxis = CoverFlow.VerifyIndex(CoverFlow.ItemMinOnAxis + 1);
                    recycledViews.Add(v);
                    v.IsVisible = false;
                }
                v.TranslationX = translate;
            }
        }

        public Task HandlePanReset(IEnumerable<View> views, AnimationDirection animationDirection, IEnumerable<View> inactiveViews)
        {
            throw new NotImplementedException();
        }
    }
}
