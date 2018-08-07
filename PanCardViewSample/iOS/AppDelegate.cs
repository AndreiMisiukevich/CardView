using FFImageLoading.Forms.Touch;
using Foundation;
using UIKit;
using PanCardView.iOS;

namespace PanCardViewSample.iOS
{
	[Register("AppDelegate")]
	public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
	{
		public override bool FinishedLaunching(UIApplication app, NSDictionary options)
		{
			CachedImageRenderer.Init();
            CardsViewRenderer.Preserve();

			global::Xamarin.Forms.Forms.Init();

			LoadApplication(new App());

			return base.FinishedLaunching(app, options);
		}
	}
}