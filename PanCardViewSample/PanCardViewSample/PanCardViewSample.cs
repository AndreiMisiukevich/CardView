// 01(c) Andrei Misiukevich
using System;

using Xamarin.Forms;
using PanCardView;
using System.Collections.Generic;

namespace PanCardViewSample
{
    public class App : Application
    {
        public App()
        {
            var content = new ContentPage
            {
                Title = "PanCardViewSample",
                Content = new CardView(() =>
                {
                    var view = new ContentView();
                    view.SetBinding(VisualElement.BackgroundColorProperty, "Color");
                    return view;
                })
                {
                    Items = new List<object> 
                    { 
                        new { Color = Color.Red },
                        new { Color = Color.Yellow },
                        new { Color = Color.Green },
                        new { Color = Color.Blue },
                        new { Color = Color.Black }
                    }
                }
            };

            MainPage = new NavigationPage(content);
        }
    }
}
