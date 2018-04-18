using PanCardView.Processors;

namespace PanCardView
{
    public class SceneView : CardsView
	{
		public SceneView() : this(new BaseSceneFrontProcessor(), new BaseSceneBackProcessor())
		{
		}

		public SceneView(ICardProcessor frontViewProcessor, ICardProcessor backViewProcessor) : base(frontViewProcessor, backViewProcessor)
		{
			BackViewsDepth *= 2;
			MaxChildrenCount *= 2;
			DesiredMaxChildrenCount *= 2;
		}
	}
}