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

        /// <summary>
        /// The number of displayed views property.
        /// </summary>
        public static readonly BindableProperty NumberOfViewsProperty = BindableProperty.Create(nameof(NumberOfViews), typeof(int), typeof(CoverFlow), 3, propertyChanged: (bindable, oldValue, newValue) =>
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

        /// <summary>
        /// Gets or set number of displayed views.
        /// </summary>
        /// <value>The number of displayed views.</value>
        public int NumberOfViews
        {
            get => (int)GetValue(NumberOfViewsProperty);
            set => SetValue(NumberOfViewsProperty, value);
        }

        public bool IsCyclical
        {
            get => (bool)GetValue(IsCyclicalProperty);
            set => SetValue(IsCyclicalProperty, value);
        }

        //Just Five For Now.
        List<ContentView> DisplayedViews;
        //Just One at first.
        List<ContentView> RecycledViews;

        public ICardProcessor ViewProcessor { get; }
        private PanGestureRecognizer _panGesture;
        public int MiddleIndex;
        private double TmpTotalX = 0;
        private int ItemMaxOnAxis;
        private int ItemMinOnAxis;
        bool SizeAllocated = false;
        double MaxGraphicAxis = 0;
        double MarginBorder = 0;
        double Space = 0;

        public CoverFlow()
        {
            DisplayedViews = new List<ContentView>();
            RecycledViews = new List<ContentView>();

            SetPanGesture();
        }



        private void SetupLayout()
        {
            if (DisplayedViews.Any())
            {
                // Centered
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

            if (width < 0 || height < 0 || SizeAllocated)
                return;
            SizeAllocated = true;

            MaxGraphicAxis = width / 2; // Set width/2 to see all views

            // the larger width * 2
            Space = MaxGraphicAxis * 2 / DisplayedViews.Count();
            MarginBorder = Space / 2;
            var index = 0;
            foreach (var view in DisplayedViews)
            {
                var translate = Space * index++;
                if (translate > MaxGraphicAxis)
                {
                    translate += -(2 * MaxGraphicAxis);
                }
                view.TranslationX = translate;
            }

            // Positive Views
            var positiveViews = DisplayedViews.Where(v => v.TranslationX >= 0).OrderBy(v => v.TranslationX);
            index = 0;
            ItemMinOnAxis = index;
            foreach (var v in positiveViews)
            {
                v.Content.BindingContext = ItemsSource[index];
                v.BindingContext = index;
                IsVisible = true;
                ++index;
            }
            ItemMaxOnAxis = index -1;

            // Negative Views
            var negativeViews = DisplayedViews.Where(v => v.TranslationX < 0).OrderBy(v => v.TranslationX);
            if (IsCyclical)
            {
                index = ItemsSource.Count - negativeViews.Count();
                ItemMinOnAxis = index;
                foreach (var v in negativeViews)
                {
                    v.Content.BindingContext = ItemsSource[index];
                    v.BindingContext = index;
                    ++index;
                }
            }
            else // put negative View in Recycler View
            {
                foreach (var v in negativeViews)
                {
                    Children.Remove(v);
                    DisplayedViews.Remove(v);
                    RecycledViews.Add(v);
                }
            }
        }

        public void GenerateCoverList(object bindable)
        {
            if (bindable is CoverFlow CoverFlow && CoverFlow.ItemTemplate is DataTemplate itemTemplate && CoverFlow.ItemsSource is IList itemsSource)
            {
                //DataTemplate!...
                var indexItem = 0;
                var half = NumberOfViews / 2;
                for (int i = 0; i < NumberOfViews; ++i)
                {
                    if (indexItem == half + 1) // for setting Backed Items
                    {
                        indexItem = ItemsSource.Count - half;
                    }

                    // Use Something else for ItemIndex (Item Left & Item Right)
                    var newView = new ContentView()
                    {
                        HorizontalOptions = LayoutOptions.Fill,
                        VerticalOptions = LayoutOptions.Fill,
                        BindingContext = -1,
                        IsVisible = true,
                        Content = GenerateView(null),
                    };
                    DisplayedViews.Add(newView);
                    this.Children.Add(newView);
                    indexItem++;
                }

                var firstRecycledView = new ContentView()
                {
                    BindingContext = -1,
                    Content = GenerateView(null),
                };
                var secondRecycledView = new ContentView()
                {
                    BindingContext = -1,
                    Content = GenerateView(null),
                };
                RecycledViews.Add(firstRecycledView);
                RecycledViews.Add(secondRecycledView);
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

        public int GetMiddleIndex()
        {
            var CloseToMiddle = DisplayedViews.Where(v => v.IsVisible == true).OrderBy(v => Math.Abs((v.TranslationX)));
            return DisplayedViews.IndexOf(CloseToMiddle.First());
        }

        public int VerifyIndex(int index)
        {
            if (IsCyclical)
            {
                if (index < 0)
                    index += ItemsSource.Count;
                else if (index >= ItemsSource.Count)
                    index -= ItemsSource.Count;
            }
            else
                if (index >= ItemsSource.Count)
                index = -1;
            else if (index < 0)
                index = -1;
            return index;
        }

        public void RecyclerRightToLeft(ContentView v)
        {
            //use Somthing else than BindingContext for ItemIndex.
            var ItemIndex = VerifyIndex(ItemMinOnAxis - 1);
            ItemMaxOnAxis = VerifyIndex(ItemMaxOnAxis - 1);

            if (ItemIndex != ItemMinOnAxis)
            {
                //For Debug Part recycler from Sample ItemTemplate
                if (v.Content is AbsoluteLayout layout && layout.Children[1] is Frame frame && frame.Content is Label label )
                {
                    var type = ItemsSource[ItemIndex].GetType();
                    PropertyInfo numberPropertyInfo = type.GetProperty("Text");
                    string value = (string)numberPropertyInfo.GetValue(ItemsSource[ItemIndex], null);
                    Console.WriteLine($"{label.Text} wants to become {value}");
                }
                v.Content.BindingContext = ItemsSource[ItemIndex];
                v.BindingContext = ItemIndex;
                ItemMinOnAxis = ItemIndex;
            }
            else
            {
                v.Content.BindingContext = null;
                v.BindingContext = -1;
            }
        }

        public void RecyclerLeftToRight(ContentView v)
        {
            /*
            //use Somthing else than BindingContext for ItemIndex.
            var ItemIndex = (int)v.BindingContext + DisplayedViews.Count;
            if (ItemIndex >= ItemsSource.Count)
            {
                ItemIndex -= ItemsSource.Count;
            }
            */

            //use Somthing else than BindingContext for ItemIndex.
            var ItemIndex = VerifyIndex(ItemMaxOnAxis + 1);
            ItemMinOnAxis = VerifyIndex(ItemMinOnAxis + 1);

            if (ItemIndex != ItemMaxOnAxis)
            {
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
                ItemMaxOnAxis = ItemIndex;
            }
            else
            {
                v.Content.BindingContext = null;
                v.BindingContext = -1;
            }
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
            if (dragX > 0) // Movement --> right
            {
                foreach (var v in DisplayedViews)
                {
                    var translate = v.TranslationX + dragX;
                    if (v.TranslationX > 0 && translate > MaxGraphicAxis + MarginBorder && DisplayedViews.Count() > 1) // verify the second condition
                    {
                        //RecyclerRightToLeft(v);
                        ItemMaxOnAxis = VerifyIndex(ItemMaxOnAxis - 1);
                        RecycledViews.Add(v);
                        v.IsVisible = false;
                    }
                    v.TranslationX = translate;
                }
                if (DisplayedViews.Any())
                {
                    var MinViewonAxis = DisplayedViews.Min(v => v.TranslationX);
                    if (Math.Abs(MinViewonAxis + MaxGraphicAxis) > MarginBorder)
                    {
                        var index = VerifyIndex(ItemMinOnAxis - 1);
                        if (RecycledViews.Any() && index != -1)
                        {
                            var view = RecycledViews[0];
                            SetLayoutBounds(view, new Rectangle(0, 0, 1, 1));
                            SetLayoutFlags(view, AbsoluteLayoutFlags.All);
                            view.TranslationX = MinViewonAxis - Space;

                            ItemMinOnAxis = index;
                            view.BindingContext = index;
                            view.Content.BindingContext = ItemsSource[index];

                            DisplayedViews.Add(view);
                            Children.Add(view);
                            RecycledViews.Remove(view);
                            view.IsVisible = true;
                        }
                    }
                }
            }
            else if (dragX < 0) // Movement <-- Left
            {
                foreach (var v in DisplayedViews)
                {
                    var translate = v.TranslationX + dragX;
                    if (v.TranslationX < 0 && translate < -MaxGraphicAxis - MarginBorder && DisplayedViews.Count() > 1) // verify the second condition
                    {
                        //RecyclerLeftToRight(v);
                        ItemMinOnAxis = VerifyIndex(ItemMinOnAxis + 1);
                        RecycledViews.Add(v);
                        v.IsVisible = false;
                    }
                    v.TranslationX = translate;
                }
                if (DisplayedViews.Any())
                {
                    var MaxViewonAxis = DisplayedViews.Max(v => v.TranslationX);
                    if (Math.Abs(MaxViewonAxis - MaxGraphicAxis) > MarginBorder)
                    {
                        var index = VerifyIndex(ItemMaxOnAxis + 1);
                        if (RecycledViews.Any() && index != -1)
                        {
                            var view = RecycledViews[0];
                            SetLayoutBounds(view, new Rectangle(0, 0, 1, 1));
                            SetLayoutFlags(view, AbsoluteLayoutFlags.All);
                            view.TranslationX = MaxViewonAxis + Space;

                            ItemMaxOnAxis = index;
                            view.BindingContext = index;
                            view.Content.BindingContext = ItemsSource[index];

                            DisplayedViews.Add(view);
                            Children.Add(view);
                            RecycledViews.Remove(view);
                            view.IsVisible = true;
                        }
                    }
                }
            }

            // remove Undisplayed View
            foreach (var v in RecycledViews)
            {
                if (DisplayedViews.Contains(v))
                {
                    DisplayedViews.Remove(v);
                    Children.Remove(v);
                }
            }
        }

        private void OnDragEnd()
        {
            //CenterView();
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
                    if (translate > MaxGraphicAxis + MarginBorder)
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
                    if (translate < -MaxGraphicAxis - MarginBorder)
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
