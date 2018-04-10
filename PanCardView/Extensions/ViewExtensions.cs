using Xamarin.Forms;

namespace PanCardView.Extensions
{
	public static class ViewExtensions
	{
		public static TView WithVisibility<TView>(this TView view, bool isVisible) where TView: View
		{
			if(view != null) 
			{
				view.IsVisible = isVisible;
			}
			return view;
		}
	}
}
