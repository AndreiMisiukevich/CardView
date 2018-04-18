using System.Collections.Generic;
using Xamarin.Forms;

namespace PanCardView.Utility
{
	public sealed class ViewsInUseSet
	{
		private readonly Dictionary<View, int> _viewsSet = new Dictionary<View, int>();

		public void Add(View view)
		=> _viewsSet[view] = Contains(view)
			? _viewsSet[view] + 1
			: 1;

		public int Remove(View view)
		{
			if (!Contains(view))
			{
				return -1;
			}

			var currentCount = _viewsSet[view] - 1;
			if (currentCount > 0)
			{
				_viewsSet[view] = currentCount;
			}
			else
			{
				_viewsSet.Remove(view);
			}
			return currentCount;
		}

		public bool Contains(View view)
		=> view != null && _viewsSet.ContainsKey(view);
	}
}