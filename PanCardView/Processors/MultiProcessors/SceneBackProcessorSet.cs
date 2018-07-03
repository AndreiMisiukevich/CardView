using System;
namespace PanCardView.Processors.MultiProcessors
{
	public class SceneBackProcessorSet : MultiProcessorSceneProcessor
    {
        public SceneBackProcessorSet()
        {
            TranslationProcessor = new TranslationBackSceneProcessor();

            Processors.Add(new ScaleBackSceneProcessor());
            Processors.Add(new OpacityBackSceneProcessor());
        }
    }
}
