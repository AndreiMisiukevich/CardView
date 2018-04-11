using PanCardView.Processors;
using Xamarin.Forms;

namespace PanCardView
{
	public class SceneView : CardsView
	{
		public static readonly BindableProperty InitialPositionProperty = BindableProperty.Create(nameof(InitialPosition), typeof(double), typeof(SceneView), -1.0);

		public static readonly BindableProperty InitialPositionPercentageProperty = BindableProperty.Create(nameof(InitialPositionPercentage), typeof(double), typeof(SceneView), .5);

		public SceneView() : this(new BaseSceneFrontProcessor(), new BaseSceneBackProcessor())
		{
		}

		public SceneView(ICardProcessor frontViewProcessor, ICardProcessor backViewProcessor) : base(frontViewProcessor, backViewProcessor)
		{
		}

		public double InitialPosition
		{
			get
			{
				var pos = (double)GetValue(InitialPositionProperty);
				return pos > 0
						? pos
						: Width * InitialPositionPercentage;
			}
			set => SetValue(MoveDistanceProperty, value);
		}

		public double InitialPositionPercentage
		{
			get => (double)GetValue(InitialPositionPercentageProperty);
			set => SetValue(InitialPositionPercentageProperty, value);
		}
	}
}
