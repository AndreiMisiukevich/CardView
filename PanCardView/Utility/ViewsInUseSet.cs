using System;
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

		public bool Remove(View view)
		{
			if (!Contains(view))
			{
				return false;
			}

			var currentCount = _viewsSet[view] - 1;
			if (currentCount > 0)
			{
				_viewsSet[view] = currentCount;
			}
			return true;
		}

		public bool Contains(View view) 
		=> view != null && _viewsSet.ContainsKey(view);
	}
}
