using Xamarin.Forms;
using Xamarin.Forms.Platform.Tizen;
using FFImageLoading.Forms.Platform;
using PanCardView.Tizen;

namespace PanCardViewSample.Tizen
{
    class Program :FormsApplication
    {
        protected override void OnCreate()
        {
            base.OnCreate();
            LoadApplication(new App());
        }

        static void Main(string[] args)
        {
            var app = new Program();
            Forms.Init(app, true);
            CachedImageRenderer.Init(app);
            CardsViewRenderer.Preserve();
            app.Run(args);
        }
    }
}
