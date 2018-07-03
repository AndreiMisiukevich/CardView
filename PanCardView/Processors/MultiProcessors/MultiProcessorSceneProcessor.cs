using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PanCardView;
using PanCardView.Enums;
using Xamarin.Forms;

namespace PanCardView.Processors.MultiProcessors
    public class MultiProcessorSceneProcessor : BaseSceneProcessor
    {
        protected BaseTranslationProcessor TranslationProcessor { get; set; }

        public IList<BaseSceneProcessor> Processors { get; set; } = new List<BaseSceneProcessor>();

        public override Task HandleAutoNavigate(IEnumerable<View> views, CardsView cardsView, AnimationDirection animationDirection, IEnumerable<View> inactiveViews)
        {
            foreach (var processor in Processors)
                processor.HandleAutoNavigate(views, cardsView, animationDirection, inactiveViews);

            return TranslationProcessor.HandleAutoNavigate(views, cardsView, animationDirection, inactiveViews);;
        }

        public override void HandleInitView(IEnumerable<View> views, CardsView cardsView, AnimationDirection animationDirection)
        {
            foreach (var processor in Processors)
                processor.HandleInitView(views, cardsView, animationDirection);

            TranslationProcessor.HandleInitView(views, cardsView, animationDirection);
        }

        public override Task HandlePanApply(IEnumerable<View> views, CardsView cardsView, AnimationDirection animationDirection, IEnumerable<View> inactiveViews)
        {
            foreach (var processor in Processors)
                processor.HandlePanApply(views, cardsView, animationDirection, inactiveViews);

            return TranslationProcessor.HandlePanApply(views, cardsView, animationDirection, inactiveViews);;
        }

        public override void HandlePanChanged(IEnumerable<View> views, CardsView cardsView, double xPos, AnimationDirection animationDirection, IEnumerable<View> inactiveViews)
        {
            foreach (var processor in Processors)
                processor.HandlePanChanged(views, cardsView, xPos, animationDirection, inactiveViews);

            TranslationProcessor.HandlePanChanged(views, cardsView, xPos, animationDirection, inactiveViews);
        }

        public override Task HandlePanReset(IEnumerable<View> views, CardsView cardsView, AnimationDirection animationDirection, IEnumerable<View> inactiveViews)
        {
            foreach (var processor in Processors)
                processor.HandlePanReset(views, cardsView, animationDirection, inactiveViews);

            return TranslationProcessor.HandlePanReset(views, cardsView, animationDirection, inactiveViews);
        }
    }
}
