using static System.Math;
using PanCardView.Enums;
using Xamarin.Forms;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace PanCardView.Processors
{
	public class BaseSceneBackProcessor : ICardProcessor
	{
		public uint AnimationLength { get; set; } = 300;

		public Easing AnimEasing { get; set; } = Easing.SinInOut;

		public double InitialBackPositionPercentage { get; set; } = .5;

		public virtual void HandleInitView(IEnumerable<View> views, CardsView cardsView, AnimationDirection animationDirection)
		{
			var view = views.FirstOrDefault();
			if (view != null)
			{
				view.TranslationX = Sign((int)animationDirection) * GetInitialPosition(cardsView);
				view.IsVisible = true;
			}
		}

		public virtual void HandlePanChanged(IEnumerable<View> views, CardsView cardsView, double xPos, AnimationDirection animationDirection, IEnumerable<View> inactiveViews)
		{
			var view = views.FirstOrDefault();
			var inactiveView = inactiveViews.FirstOrDefault();

			var inactiveIntAnimationDirection = -((int)animationDirection);
			var inactiveAnimationDirection = animationDirection == AnimationDirection.Null
				 ? animationDirection
				 : (AnimationDirection)inactiveIntAnimationDirection;

			var handled = true;
			if(view != null)
			{
				view.IsVisible = true;
				handled = HandlePanChanged(views, cardsView, xPos, animationDirection);
			}
			if (inactiveView != null && handled)
			{
				inactiveView.IsVisible = true;
				HandlePanChanged(inactiveViews, cardsView, xPos, inactiveAnimationDirection, false);
			}
		}

		public virtual Task HandleAutoNavigate(IEnumerable<View> views, CardsView cardsView, AnimationDirection animationDirection)
		{
			var view = views.FirstOrDefault();
			
			if (view == null)
			{
				return Task.FromResult(false);
			}

			var tcs = new TaskCompletionSource<bool>();
			var width =  GetInitialPosition(cardsView);
			var destinationPos = GetInitialPosition(cardsView);
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

		public virtual Task HandlePanReset(IEnumerable<View> views, CardsView cardsView, AnimationDirection animationDirection)
		{
			var view = views.FirstOrDefault();

			if (view != null)
			{
				var tcs = new TaskCompletionSource<bool>();
				var width = GetInitialPosition(cardsView);
				var animTimePercent = (width - Abs(view.TranslationX)) / width;
				var animLength = (uint)(AnimationLength * animTimePercent) * 3 / 2;
				new Animation(v => view.TranslationX = v, view.TranslationX, Sign((int)animationDirection) * width)
					.Commit(view, nameof(HandlePanReset), 16, animLength, AnimEasing, (v, t) => tcs.SetResult(true));
				return tcs.Task;
			}
			return Task.FromResult(true);
		}

		public virtual Task HandlePanApply(IEnumerable<View> views, CardsView cardsView, AnimationDirection animationDirection)
		{
			var view = views.FirstOrDefault();

			if (view != null)
			{
				var tcs = new TaskCompletionSource<bool>();

				var width = GetInitialPosition(cardsView);
				var animTimePercent = (width - Abs(view.TranslationX)) / width;
				var animLength = (uint)(AnimationLength * animTimePercent);
				new Animation(v => view.TranslationX = v, view.TranslationX, -Sign((int)animationDirection) * width)
					.Commit(view, nameof(HandlePanReset), 16, animLength, AnimEasing, (v, t) => tcs.SetResult(true));
				return tcs.Task;
			}
			return Task.FromResult(true);
		}

		private bool HandlePanChanged(IEnumerable<View> views, CardsView cardsView, double xPos, AnimationDirection animationDirection, bool checkWidth = true)
		{
			var view = views.FirstOrDefault();

			if (animationDirection == AnimationDirection.Null || view == null)
			{
				return false;
			}
			var width = GetInitialPosition(cardsView);
			var value = Sign((int)animationDirection) * width + xPos;
			if (checkWidth && (Abs(value) > width || (animationDirection == AnimationDirection.Prev && value > 0) || (animationDirection == AnimationDirection.Next && value < 0)))
			{
				return false;
			}
			view.TranslationX = value;
			return true;
		}

		private double GetInitialPosition(CardsView cardsView)
		=> cardsView.Width * InitialBackPositionPercentage;
	}
}
