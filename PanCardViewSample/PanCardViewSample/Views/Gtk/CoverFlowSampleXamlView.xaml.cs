using PanCardViewSample.ViewModels;
using Xamarin.Forms;

namespace PanCardViewSample.Views.Gtk
{
    public partial class CoverFlowSampleXamlView : ContentPage
    {
        public CoverFlowSampleXamlView()
        {
            InitializeComponent();

            var prevItem = new ToolbarItem
            {
                Text = "**Prev**",
                Icon = "prev",
                CommandParameter = false
            };
            prevItem.SetBinding(MenuItem.CommandProperty, nameof(CardsSampleViewModel.PanPositionChangedCommand));

            var nextItem = new ToolbarItem
            {
                Text = "**Next**",
                Icon = "next",
                CommandParameter = true
            };
            nextItem.SetBinding(MenuItem.CommandProperty, nameof(CardsSampleViewModel.PanPositionChangedCommand));

            ToolbarItems.Add(prevItem);
            ToolbarItems.Add(nextItem);
        }
    }
}
