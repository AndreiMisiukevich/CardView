using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Reflection;
using PanCardView.Controls;
using PanCardView.Extensions;
using PanCardView.Processors;
using Xamarin.Forms;

namespace PanCardView
{
    [Obsolete("NOT FINISHED YET! IT WILL BE AVAILABLE IN NEXT RELEASES!")]
    public class CoverFlow : AbsoluteLayout
    {
        /// <summary>
        /// CoverFlow items source property.
        /// </summary>
        public static BindableProperty ItemsSourceProperty = BindableProperty.Create(nameof(ItemsSource), typeof(IList), typeof(CoverFlow), null, propertyChanged: (bindable, oldValue, newValue) =>
        {
            bindable.AsCoverView().GenerateCoverList(bindable);
            bindable.AsCoverView().SetupLayout();
        });

        /// <summary>
        /// The item template property.
        /// </summary>
        public static readonly BindableProperty ItemTemplateProperty = BindableProperty.Create(nameof(ItemTemplate), typeof(DataTemplate), typeof(CoverFlow), propertyChanged: (bindable, oldValue, newValue) =>
        {
            bindable.AsCoverView().GenerateCoverList(bindable);
            bindable.AsCoverView().SetupLayout();
        });

        public static readonly BindableProperty IsCyclicalProperty = BindableProperty.Create(nameof(IsCyclical), typeof(bool), typeof(CoverFlow), false);

        /// <summary>
        /// Gets or set the cover list items source.
        /// </summary>
        /// <value>The cover list items source.</value>
        public IList ItemsSource
        {
            get => GetValue(ItemsSourceProperty) as IList;
            set => SetValue(ItemsSourceProperty, value);
        }

        /// <summary>
        /// Gets or set the cover list item template.
        /// </summary>
        /// <value>The cover list items template.</value>
        public DataTemplate ItemTemplate
        {
            get => GetValue(ItemTemplateProperty) as DataTemplate;
            set => SetValue(ItemTemplateProperty, value);
        }

        public bool IsCyclical
        {
            get => (bool)GetValue(IsCyclicalProperty);
            set => SetValue(IsCyclicalProperty, value);
        }

        //Just Five For Now.
        List<ContentView> DisplayedViews;

        public ICardProcessor ViewProcessor { get; }
        private PanGestureRecognizer _panGesture;
        public int MiddleIndex;
        private double TmpTotalX = 0;
        private int ItemLeft;
        private int ItemRight;
        double MaxGraphicAxis = 0;

        public CoverFlow()
        {
            DisplayedViews = new List<ContentView>();

            SetPanGesture();
        }



        private void SetupLayout()
        {
            if (DisplayedViews.Any())
            {
                foreach (var view in DisplayedViews.Where(v => v != null))
                {
                    SetLayoutBounds(view, new Rectangle(0, 0, 1, 1));
                    SetLayoutFlags(view, AbsoluteLayoutFlags.All);
                }
            }
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            if (width < 0 || height < 0)
                return;

            MaxGraphicAxis = width; // Set width/2 to see all views

            // the larger width * 2
            var space = MaxGraphicAxis * 2 / DisplayedViews.Count();
            var index = 0;
            foreach (var view in DisplayedViews)
            {
                var translate = space * index++;
                if (translate > MaxGraphicAxis)
                {
                    translate += -(2 * MaxGraphicAxis);
                }
                view.TranslationX = translate;
            }
        }

        public View GenerateView(object context)
        {
            var template = ItemTemplate;
            while (template is DataTemplateSelector selector)
            {
                template = selector.SelectTemplate(context, this);
            }

            var view = template != null
               ? template.CreateContent() as View
               : context as View;

            var test = template.CreateContent();
            if (view != null && view != context)
            {
                view.BindingContext = context;
            }

            return view;
        }

        public void GenerateCoverList(object bindable)
        {
            if (bindable is CoverFlow CoverFlow && CoverFlow.ItemTemplate is DataTemplate itemTemplate && CoverFlow.ItemsSource is IList itemsSource)
            {
                //DataTemplate!...
                var indexItem = 0;
                for (int i = 0; i < 5; ++i)
                {
                    if (indexItem == 3) // for setting Backed Items
                    {
                        indexItem = ItemsSource.Count - 2;
                    }

                    // Use Something else for ItemIndex (Item Left & Item Right)
                    var newView = new ContentView()
                    {
                        BindingContext = indexItem,
                        Content = GenerateView(ItemsSource[indexItem])
                    };
                    DisplayedViews.Add(newView);
                    this.Children.Add(newView);
                    indexItem++;
                }
            }
        }

        public int GetMiddleIndex()
        {
            var CloseToMiddle = DisplayedViews.OrderBy(v => Math.Abs((v.TranslationX)));
            return DisplayedViews.IndexOf(CloseToMiddle.First());
        }

        public void RecyclerRightToLeft(ContentView v)
        {
            //use Somthing else than BindingContext for ItemIndex.
            var ItemIndex = (int)v.BindingContext - DisplayedViews.Count;
            if (ItemIndex < 0)
            {
                ItemIndex += ItemsSource.Count;
            }

            //For Debug Part recycler from Sample ItemTemplate
            if (v.Content is AbsoluteLayout layout && layout.Children[1] is Frame frame && frame.Content is Label label)
            {
                var type = ItemsSource[ItemIndex].GetType();
                PropertyInfo numberPropertyInfo = type.GetProperty("Text");
                string value = (string)numberPropertyInfo.GetValue(ItemsSource[ItemIndex], null);
                Console.WriteLine($"{label.Text} wants to become {value}");
            }

            v.Content.BindingContext = ItemsSource[ItemIndex];
            v.BindingContext = ItemIndex;
        }

        public void RecyclerLeftToRight(ContentView v)
        {
            //use Somthing else than BindingContext for ItemIndex.
            var ItemIndex = (int)v.BindingContext + DisplayedViews.Count;
            if (ItemIndex >= ItemsSource.Count)
            {
                ItemIndex -= ItemsSource.Count;
            }

            //For Debug Part recycler from Sample ItemTemplate
            if (v.Content is AbsoluteLayout layout && layout.Children[1] is Frame frame && frame.Content is Label label)
            {
                var type = ItemsSource[ItemIndex].GetType();
                PropertyInfo numberPropertyInfo = type.GetProperty("Text");
                string value = (string)numberPropertyInfo.GetValue(ItemsSource[ItemIndex], null);
                Console.WriteLine($"{label.Text} wants to become {value}");
            }

            v.Content.BindingContext = ItemsSource[ItemIndex];
            v.BindingContext = ItemIndex;
        }


        void SetPanGesture()
        {
            _panGesture = new PanGestureRecognizer();
            _panGesture.PanUpdated += OnPanUpdated;
            this.GestureRecognizers.Add(_panGesture);
        }

        public void OnPanUpdated(object sender, PanUpdatedEventArgs e)
        {
            if (ItemsSource.Count > 0)
            {
                switch (e.StatusType)
                {
                    case GestureStatus.Started:
                        OnDragStarted();
                        return;
                    case GestureStatus.Running:
                        var dragX = e.TotalX - TmpTotalX;
                        OnDragging(dragX);
                        TmpTotalX = e.TotalX;
                        return;
                    case GestureStatus.Canceled:
                    case GestureStatus.Completed:
                        OnDragEnd();
                        return;
                }
            }
            return;
        }

        private void OnDragStarted()
        {
            TmpTotalX = 0;
        }

        private void OnDragging(double dragX)
        {
            foreach (var v in DisplayedViews)
            {
                var translate = v.TranslationX + dragX;

                if (dragX > 0) // Movement --> right
                {
                    //Console.WriteLine("Move: -->");
                    if (translate > MaxGraphicAxis)
                    {
                        translate = translate - (2 * MaxGraphicAxis);
                    }
                    if (v.TranslationX > 0 && translate < 0)
                    {

                        RecyclerRightToLeft(v);
                    }
                }
                else if (dragX < 0) // Movement <-- Left
                {
                    //Console.WriteLine("Move: <--");
                    if (translate < -MaxGraphicAxis)
                    {
                        translate = translate + (2 * MaxGraphicAxis);
                    }
                    if (v.TranslationX < 0 && translate > 0)
                    {
                        RecyclerLeftToRight(v);
                    }
                }

                v.TranslationX = translate;
            }
        }

        private void OnDragEnd()
        {
            CenterView();
        }


        // Passing from -x to x is not animated correctly
        // Also Got an issue when animation is not fisnish when we start a new drag gesture,
        // the ContentView is recycled for a side, but still on the other.
        public void CenterView()
        {
            MiddleIndex = GetMiddleIndex();
            var dragX = -DisplayedViews[MiddleIndex].TranslationX;
            int index = 0;

            Animation a = new Animation();
            foreach (var v in DisplayedViews)
            {
                var translate = v.TranslationX + dragX;
                var tmpTranslate = v.TranslationX + dragX;


                if (dragX > 0) // Movement --> right
                {
                    //Console.WriteLine("Move: -->");
                    if (translate > MaxGraphicAxis)
                    {
                        translate = translate - (2 * MaxGraphicAxis);
                    }
                    if (v.TranslationX > 0 && translate < 0)
                    {

                        RecyclerRightToLeft(v);
                        a.Add(0, 0.5, new Animation(f => v.TranslationX = f, v.TranslationX, MaxGraphicAxis, Easing.Linear, null));
                        a.Add(0.4, 0.8, new Animation(f => v.TranslationX = f, -MaxGraphicAxis, translate, Easing.SinOut, null));
                    }
                    else
                        a.Add(0, 1, new Animation(f => v.TranslationX = f, v.TranslationX, translate, Easing.SinOut, null));
                }
                else if (dragX < 0) // Movement <-- Left
                {
                    //Console.WriteLine("Move: <--");
                    if (translate < -MaxGraphicAxis)
                    {
                        translate = translate + (2 * MaxGraphicAxis);
                    }
                    if (v.TranslationX < 0 && translate > 0)
                    {
                        RecyclerLeftToRight(v);
                        a.Add(0, 0.5, new Animation(f => v.TranslationX = f, v.TranslationX, -MaxGraphicAxis, Easing.Linear, null));
                        a.Add(0.4, 0.8, new Animation(f => v.TranslationX = f, MaxGraphicAxis, translate, Easing.SinOut, null));
                    }
                    else
                        a.Add(0, 1, new Animation(f => v.TranslationX = f, v.TranslationX, translate, Easing.SinOut, null));
                }

                index++;
            }
            a.Commit(this, "SimpleAnimation", 60, 400);
        }
    }
}
