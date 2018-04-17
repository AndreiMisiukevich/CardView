using System.Threading.Tasks;
using PanCardView.Enums;
using PanCardView.Extensions;
using Xamarin.Forms;

using static System.Math;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

namespace PanCardView.Processors
{
	public class BaseCarouselBackViewProcessor : ICardProcessor
	{
		public uint AnimationLength { get; set; } = 300;

		public Easing AnimEasing { get; set; } = Easing.SinInOut;

		public virtual void HandleInitView(IEnumerable<View> views, CardsView cardsView, AnimationDirection animationDirection)
		{
			var view = views.FirstOrDefault();
			if (view != null)
			{
				view.TranslationX = Sign((int)animationDirection) * cardsView.Width;
				view.IsVisible = false;
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

			if (animationDirection == AnimationDirection.Null)
			{
				return;
			}

			var value = Sign((int)animationDirection) * cardsView.Width + xPos;
			if (Abs(value) > cardsView.Width || (animationDirection == AnimationDirection.Prev && value > 0) || (animationDirection == AnimationDirection.Next && value < 0))
			{
				return;
			}
			view.TranslationX = value;
		}

		public virtual Task HandleAutoNavigate(IEnumerable<View> views, CardsView cardsView, AnimationDirection animationDirection, IEnumerable<View> inactiveViews)
		{
			var view = views.FirstOrDefault();
			if (view == null)
			{
				return Task.FromResult(false);
			}

			var tcs = new TaskCompletionSource<bool>();
			var destinationPos = animationDirection == AnimationDirection.Prev
			   ? cardsView.Width
			   : -cardsView.Width;

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

				var animTimePercent = (cardsView.Width - Abs(view.TranslationX)) / cardsView.Width;
				var animLength = (uint)(AnimationLength * animTimePercent) * 3 / 2;
				new Animation(v => view.TranslationX = v, view.TranslationX, Sign((int)animationDirection) * cardsView.Width)
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

				var animTimePercent = (cardsView.Width - Abs(view.TranslationX)) / cardsView.Width;
				var animLength = (uint)(AnimationLength * animTimePercent);
				new Animation(v => view.TranslationX = v, view.TranslationX, -Sign((int)animationDirection) * cardsView.Width)
					.Commit(view, nameof(HandlePanReset), 16, animLength, AnimEasing, (v, t) => tcs.SetResult(true));
				return tcs.Task;
			}
			return Task.FromResult(true);
		}
	}
}
