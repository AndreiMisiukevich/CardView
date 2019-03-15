using Xamarin.Forms;
using Xamarin.Forms.Platform.GTK;
using GtkApp = Gtk.Application;

namespace PanCardViewSample.Gtk
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            GtkApp.Init();
            Forms.Init();
            var app = new App();
            var window = new FormsWindow();
            window.LoadApplication(app);
            window.Show();
            GtkApp.Run();
        }
    }
}
