using static System.Math;
using PanCardView.Enums;
using PanCardView.Extensions;
using Xamarin.Forms;
using System.Threading.Tasks;

namespace PanCardView.Processors
{
	public class BaseSceneFrontProcessor : ICardProcessor
	{
		public uint AnimationLength { get; set; } = 300;

		public Easing AnimEasing { get; set; } = Easing.SinInOut;

		public double NoItemMaxPanDistance { get; set; } = 25;

		public virtual void HandleInitView(View view, CardsView cardsView, AnimationDirection animationDirection)
		{
			if (view != null)
			{
				view.TranslationX = 0;
				view.IsVisible = true;
			}
		}

		public virtual void HandlePanChanged(View view, CardsView cardsView, double xPos, AnimationDirection animationDirection, View inactiveView)
		{
			if(view != null)
			{
				view.IsVisible = true;
			}
			if(inactiveView != null)
			{
				inactiveView.IsVisible = false;
			}

			var width = GetInitialPosition(cardsView);
			if (Abs(xPos) > width || (animationDirection == AnimationDirection.Prev && xPos < 0) || (animationDirection == AnimationDirection.Next && xPos > 0))
			{
				return;
			}

			if (animationDirection == AnimationDirection.Null)
			{
				xPos = Sign(xPos) * Min(Abs(xPos / 4), NoItemMaxPanDistance);
			}

			view.TranslationX = xPos;
		}

		public virtual Task HandleAutoNavigate(View view, CardsView cardsView, AnimationDirection animationDirection)
		{
			if (view == null)
			{
				return Task.FromResult(false);
			}

			var tcs = new TaskCompletionSource<bool>();
			view.IsVisible = true;
			new Animation(v => view.TranslationX = v, view.TranslationX, 0)
				.Commit(view, nameof(HandleAutoNavigate), 16, AnimationLength, AnimEasing, (d, b) => tcs.SetResult(true));
			return tcs.Task;
		}

		public virtual Task HandlePanReset(View view, CardsView cardsView, AnimationDirection animationDirection)
		{
			if (view != null)
			{
				var tcs = new TaskCompletionSource<bool>();
				var width = GetInitialPosition(cardsView);
				var animTimePercent = 1 - (width - Abs(view.TranslationX)) / width;
				var animLength = (uint)(AnimationLength * animTimePercent) * 3 / 2;
				new Animation(v => view.TranslationX = v, view.TranslationX, 0)
					.Commit(view, nameof(HandlePanApply), 16, animLength, AnimEasing, (v, t) => tcs.SetResult(true));
				return tcs.Task;
			}
			return Task.FromResult(true);
		}

		public virtual Task HandlePanApply(View view, CardsView cardsView, AnimationDirection animationDirection)
		{
			if (view != null)
			{
				var tcs = new TaskCompletionSource<bool>();
				var width = GetInitialPosition(cardsView);
				var animTimePercent = 1 - (width - Abs(view.TranslationX)) / width;
				var animLength = (uint)(AnimationLength * animTimePercent);
				new Animation(v => view.TranslationX = v, view.TranslationX, 0)
					.Commit(view, nameof(HandlePanApply), 16, animLength, AnimEasing, (v, t) => tcs.SetResult(true));
				return tcs.Task;
			}
			return Task.FromResult(true);
		}

		private double GetInitialPosition(CardsView cardsView)
		=> cardsView.AsSceneView().InitialPosition;
	}
}
