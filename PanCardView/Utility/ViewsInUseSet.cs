using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace PanCardView.Utility
{
	internal class ViewsInUseSet
	{
		private readonly Dictionary<View, int> _set = new Dictionary<View, int>();

		internal void Add(View view) 
		=> _set[view] = Contains(view)
			? _set[view] + 1
			: 1;

		internal bool Remove(View view)
		{
			if (!Contains(view))
			{
				return false;
			}

			var currentCount = _set[view] - 1;
			if (currentCount > 0)
			{
				_set[view] = currentCount;
			}
			return true;
		}

		internal bool Contains(View view) 
		=> view != null && _set.ContainsKey(view);
	}
}
