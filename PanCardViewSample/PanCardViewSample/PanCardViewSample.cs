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
                Content = new CardsView
                {
                    Items = new List<object> 
                    { 
                        new { Color = Color.Red },
                        new { Color = Color.Yellow },
                        new { Color = Color.Green },
                        new { Color = Color.Blue },
                        new { Color = Color.Black }
                    },

                    ItemViewFactory = new SampleFactory()
                }
            };

            MainPage = new NavigationPage(content);
        }
    }

    public sealed class SampleFactory : CardViewItemFactory
    {
        private readonly CardViewFactoryRule _rule = new CardViewFactoryRule
        {
            Creator = () =>
            {
                var view = new ContentView();
                view.SetBinding(VisualElement.BackgroundColorProperty, "Color");
                return view;
            }
        };

        public override CardViewFactoryRule GetRule(object context) => _rule;
    }
}
