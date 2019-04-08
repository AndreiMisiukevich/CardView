using Xamarin.Forms;

namespace PanCardView
{
    /*
     This is used to enable background gestures on iOS
     to be able to scroll a ScrollView by dragging an embeeded CardsView
     USAGE EXAMPLE:

        <ScrollView>

          ..more stuff..
   
          <panCardView:CardsViewWrapper 
                    HeightRequest="350" 
                    VerticalOptions="Start"  
                    HorizontalOptions="FillAndExpand">

                <panCardView:CarouselView               
                    x:Name="MyCarousel"
                    HorizontalOptions="FillAndExpand"
                    PropertyChanged="OnPropertyChanged_Carousel"
                    HeightRequest="350"
                    ItemsSource="{Binding Images}"
                    VerticalOptions="Start">

            </panCardView:CardsViewWrapper>

           ..more stuff..

           </ScrollView>
     */

    public class CardsViewWrapper : ContentView
    {
        public CardsView Child { get; set; }

        protected override void OnChildAdded(Element child)
        {
            if (child is CardsView)
            {
                Child = child as CardsView;
                if (Device.RuntimePlatform == Device.iOS)
                {
                    //disable gestures for CardsView on iOS we will invoke them programatically from renderer
                    Child.InputTransparent = true;
                }
            }
            base.OnChildAdded(child);
        }
    }
}
