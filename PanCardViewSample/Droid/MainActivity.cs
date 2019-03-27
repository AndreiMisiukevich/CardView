
using Android.App;
using Android.Content.PM;
using Android.OS;
using FFImageLoading.Forms.Platform;
using Xamarin.Forms;
using PanCardView.Droid;

namespace PanCardViewSample.Droid
{
	[Activity(Label = "PanCardViewSample.Droid", Icon = "@drawable/icon", Theme = "@style/MyTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
	public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
	{
		protected override void OnCreate(Bundle bundle)
		{
			TabLayoutResource = Resource.Layout.Tabbar;
			ToolbarResource = Resource.Layout.Toolbar;

			base.OnCreate(bundle);

			Forms.SetFlags("FastRenderers_Experimental");
			global::Xamarin.Forms.Forms.Init(this, bundle);
            CachedImageRenderer.Init(true);
            CardsViewRenderer.Preserve();

            LoadApplication(new App());
		}
	}
}