using PanCardView.Enums;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using static System.Math;

namespace PanCardView.Processors
{
	public class BaseSceneFrontProcessor : ICardProcessor
	{
		public uint AnimationLength { get; set; } = 300;

		public Easing AnimEasing { get; set; } = Easing.SinInOut;

		public double NoItemMaxPanDistance { get; set; } = 25;

		public double InitialBackPositionPercentage { get; set; } = .5;

		public virtual void HandleInitView(IEnumerable<View> views, CardsView cardsView, AnimationDirection animationDirection)
		{
			var view = views.FirstOrDefault();
			if (view != null)
			{
				view.TranslationX = 0;
				view.IsVisible = true;
			}
		}

		public virtual void HandlePanChanged(IEnumerable<View> views, CardsView cardsView, double xPos, AnimationDirection animationDirection, IEnumerable<View> inactiveViews)
		{
			var view = views.FirstOrDefault();
			var inactiveView = inactiveViews.FirstOrDefault();

			if (view != null)
			{
				view.IsVisible = true;
			}
			if (inactiveView != null)
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

			if (view != null)
			{
				view.TranslationX = xPos;
			}
		}

		public virtual Task HandleAutoNavigate(IEnumerable<View> views, CardsView cardsView, AnimationDirection animationDirection, IEnumerable<View> inactiveViews)
		{
			var view = views.FirstOrDefault();
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

		public virtual Task HandlePanReset(IEnumerable<View> views, CardsView cardsView, AnimationDirection animationDirection, IEnumerable<View> inactiveViews)
		{
			var view = views.FirstOrDefault();
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

		public virtual Task HandlePanApply(IEnumerable<View> views, CardsView cardsView, AnimationDirection animationDirection, IEnumerable<View> inactiveViews)
		{
			var view = views.FirstOrDefault();
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
		=> cardsView.Width * InitialBackPositionPercentage;
	}
}