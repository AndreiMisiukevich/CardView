using Foundation;
using UIKit;
using PanCardView.iOS;
using FFImageLoading.Forms.Platform;

namespace PanCardViewSample.iOS
{
	[Register("AppDelegate")]
	public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
	{
		public override bool FinishedLaunching(UIApplication app, NSDictionary options)
		{
            global::Xamarin.Forms.Forms.Init();
            CachedImageRenderer.Init();
            CardsViewRenderer.Preserve();

			LoadApplication(new App());

			return base.FinishedLaunching(app, options);
		}
	}
}