using PanCardView.Processors;
using Xamarin.Forms;

namespace PanCardView
{
	public class SceneView : CardsView
	{
		public SceneView() : this(new BaseSceneFrontProcessor(), new BaseSceneBackProcessor())
		{
		}

		public SceneView(ICardProcessor frontViewProcessor, ICardProcessor backViewProcessor) : base(frontViewProcessor, backViewProcessor)
		{
		}
	}
}
