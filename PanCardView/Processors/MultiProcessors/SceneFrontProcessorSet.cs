using System;
namespace PanCardView.Processors.MultiProcessors
{
    public class SceneFrontProcessorSet : MultiProcessorSceneProcessor
    {
        public SceneFrontProcessorSet()
        {
            TranslationProcessor = new TranslationFrontSceneProcessor();

            Processors.Add(new ScaleFrontSceneProcessor());
            Processors.Add(new OpacityFrontSceneProcessor());
        }
    }
}
