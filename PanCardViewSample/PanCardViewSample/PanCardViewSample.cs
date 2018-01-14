// 01(c) Andrei Misiukevich
using System;

using Xamarin.Forms;
using PanCardView;
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections.ObjectModel;
using FFImageLoading.Forms;

namespace PanCardViewSample
{
    public class App : Application
    {
        public App()
        {
            MainPage = new NavigationPage(new CardsSampleView());
        }
    }
}
