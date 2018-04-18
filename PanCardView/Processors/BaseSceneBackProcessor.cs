using PanCardView.Enums;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using static System.Math;

namespace PanCardView.Processors
{
	public class BaseSceneBackProcessor : ICardProcessor
	{
		public uint AnimationLength { get; set; } = 300;

		public Easing AnimEasing { get; set; } = Easing.SinInOut;

		public double InitialBackPositionPercentage { get; set; } = .5;

		public virtual void HandleInitView(IEnumerable<View> views, CardsView cardsView, AnimationDirection animationDirection)
		{
			if (cardsView.Width < 0)
			{
				return;
			}

			var viewsArr = views.ToArray();
			for (var i = 0; i < viewsArr.Length; ++i)
			{
				var view = viewsArr[i];
				if (view != null)
				{
					view.IsVisible = true;
					view.TranslationX = Sign((int)animationDirection) * GetInitialPosition(cardsView, i);
				}
			}
		}

		public virtual void HandlePanChanged(IEnumerable<View> views, CardsView cardsView, double xPos, AnimationDirection animationDirection, IEnumerable<View> inactiveViews)
		{
			var inactiveIntAnimationDirection = -((int)animationDirection);
			var inactiveAnimationDirection = animationDirection == AnimationDirection.Null
				 ? animationDirection
				 : (AnimationDirection)inactiveIntAnimationDirection;

			var handled = true;
			if (views.Any())
			{
				handled = HandlePanChanged(views, cardsView, xPos, animationDirection);
			}
			if (inactiveViews.Any() && handled)
			{
				HandlePanChanged(inactiveViews, cardsView, xPos, inactiveAnimationDirection, false);
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
			var width = GetInitialPosition(cardsView, 0);
			var destinationPos = GetInitialPosition(cardsView, 0);
			if (animationDirection != AnimationDirection.Prev)
			{
				destinationPos = -destinationPos;
			}

			new Animation(v => view.TranslationX = v, 0, destinationPos)
				.Commit(view, nameof(HandleAutoNavigate), 16, AnimationLength, AnimEasing, (v, t) =>
				{
					tcs.SetResult(true);
				});

			return tcs.Task;
		}

		public virtual Task HandlePanReset(IEnumerable<View> views, CardsView cardsView, AnimationDirection animationDirection, IEnumerable<View> inactiveViews)
		{
			var view = views.FirstOrDefault();

			if (view != null)
			{
				var tcs = new TaskCompletionSource<bool>();
				var width = GetInitialPosition(cardsView, 0);
				var animTimePercent = (width - Abs(view.TranslationX)) / width;
				var animLength = (uint)(AnimationLength * animTimePercent) * 3 / 2;
				new Animation(v => view.TranslationX = v, view.TranslationX, Sign((int)animationDirection) * width)
					.Commit(view, nameof(HandlePanReset), 16, animLength, AnimEasing, (v, t) => tcs.SetResult(true));
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

				var width = GetInitialPosition(cardsView, 0);
				var animTimePercent = Abs(width - Abs(view.TranslationX)) / width;
				var animLength = (uint)(AnimationLength * animTimePercent);
				new Animation(v => view.TranslationX = v, view.TranslationX, -Sign((int)animationDirection) * width)
					.Commit(view, nameof(HandlePanReset), 16, animLength, AnimEasing, (v, t) => tcs.SetResult(true));
				return tcs.Task;
			}
			return Task.FromResult(true);
		}

		private bool HandlePanChanged(IEnumerable<View> views, CardsView cardsView, double xPos, AnimationDirection animationDirection, bool checkWidth = true)
		{
			var result = false;
			var viewsArr = views.ToArray();

			for (var i = 0; i < viewsArr.Length; ++i)
			{
				var view = viewsArr[i];
				if (animationDirection == AnimationDirection.Null || view == null)
				{
					continue;
				}
				var width = GetInitialPosition(cardsView, i);
				var value = Sign((int)animationDirection) * width + xPos;
				if (checkWidth && (Abs(value) > width || (animationDirection == AnimationDirection.Prev && value > 0) || (animationDirection == AnimationDirection.Next && value < 0)))
				{
					continue;
				}
				result = true;
				view.TranslationX = value;
			}
			return result;
		}

		private double GetInitialPosition(CardsView cardsView, int index)
		=> cardsView.Width * InitialBackPositionPercentage * (index + 1);
	}
}