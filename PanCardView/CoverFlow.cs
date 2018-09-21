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
using PanCardView.Enums;
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
        });

        /// <summary>
        /// The item template property.
        /// </summary>
        public static readonly BindableProperty ItemTemplateProperty = BindableProperty.Create(nameof(ItemTemplate), typeof(DataTemplate), typeof(CoverFlow), propertyChanged: (bindable, oldValue, newValue) =>
        {
            bindable.AsCoverView().GenerateCoverList(bindable);
        });

        /// <summary>
        /// The number of displayed views property.
        /// </summary>
        public static readonly BindableProperty NumberOfViewsProperty = BindableProperty.Create(nameof(NumberOfViews), typeof(int), typeof(CoverFlow), 3, propertyChanged: (bindable, oldValue, newValue) =>
        {
            bindable.AsCoverView().GenerateCoverList(bindable);
        });

        /// <summary>
        /// The Cyclical property.
        /// </summary>
        public static readonly BindableProperty IsCyclicalProperty = BindableProperty.Create(nameof(IsCyclical), typeof(bool), typeof(CoverFlow), true);

        /// <summary>
        /// The Cyclical property.
        /// </summary>
        public static readonly BindableProperty FirstItemPositionProperty = BindableProperty.Create(nameof(FirstItemPosition), typeof(ItemPosition), typeof(CoverFlow), ItemPosition.Left);

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

        /// <summary>
        /// Gets or set the cyclical value.
        /// </summary>
        /// <value>The cyclical value.</value>
        public bool IsCyclical
        {
            get => (bool)GetValue(IsCyclicalProperty);
            set => SetValue(IsCyclicalProperty, value);
        }

        /// <summary>
        /// Gets or set the first item position.
        /// </summary>
        /// <value>The first item position.</value>
        public ItemPosition FirstItemPosition
        {
            get => (ItemPosition)GetValue(FirstItemPositionProperty);
            set => SetValue(FirstItemPositionProperty, value);
        }

        // Like this.Children
        List<View> DisplayedViews;
        // Just Two at first. but one for each template at the end.
        List<View> RecycledViews;

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
            this.IsClippedToBounds = true;
            DisplayedViews = new List<View>();
            RecycledViews = new List<View>();

            SetPanGesture();
        }

        private void SetupLayout()
        {
            if (DisplayedViews.Any())
            {
                // Centered
                foreach (var view in DisplayedViews.Where(v => v != null))
                {
                    SetupBoundsView(view);
                }
            }
        }

        private void SetupBoundsView(View view)
        {
            SetLayoutBounds(view, new Rectangle(0, 0, 1, 1));
            SetLayoutFlags(view, AbsoluteLayoutFlags.All);
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            if (width < 0 || height < 0 || SizeAllocated && DisplayedViews.Any())
                return;
            SizeAllocated = true;

            Space = Width / NumberOfViews;
            MaxGraphicAxis = width / 2 + Space / 2;
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

            BindingItemsToViews();
        }

        public void BindingItemsToViews()
        {
            // Positive Views
            var positiveViews = DisplayedViews.Where(v => v.TranslationX >= (int)FirstItemPosition * Width / 2 - MarginBorder).OrderBy(v => v.TranslationX);
            BindingPositiveViews(positiveViews);

            // Negative Views
            var negativeViews = DisplayedViews.Where(v => v.TranslationX < (int)FirstItemPosition * Width / 2 - MarginBorder).OrderBy(v => v.TranslationX);
            BindingNegativeViews(negativeViews);
        }

        public void BindingPositiveViews(IEnumerable<View> positiveViews)
        {
            // Positive Views
            var index = 0;
            ItemMinOnAxis = index;
            foreach (var v in positiveViews)
            {
                v.BindingContext = ItemsSource[index];
                IsVisible = true;
                ++index;
            }
            ItemMaxOnAxis = index - 1;
        }

        public void BindingNegativeViews(IEnumerable<View> negativeViews)
        {
            if (IsCyclical)
            {
                var index = ItemsSource.Count - negativeViews.Count();
                ItemMinOnAxis = index;
                foreach (var v in negativeViews)
                {
                    v.BindingContext = ItemsSource[index];
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
                //DataTemplate Selector?!...
                for (int i = 0; i < NumberOfViews + 1; ++i) // +2 if Paire?
                {
                    var newView =  GenerateView(null);
                    DisplayedViews.Add(newView);
                    this.Children.Add(newView);
                }

                for (int i = 0; i < 2; ++i)
                {
                    var recycleView = GenerateView(null);
                    RecycledViews.Add(recycleView);
                }

                SetupLayout();
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
                            SetupBoundsView(view);
                            view.TranslationX = MinViewonAxis - Space;

                            ItemMinOnAxis = index;
                            view.BindingContext = ItemsSource[index];

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
                            SetupBoundsView(view);
                            view.TranslationX = MaxViewonAxis + Space;

                            ItemMaxOnAxis = index;
                            view.BindingContext = ItemsSource[index];

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

                        //RecyclerRightToLeft(v);
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
                        //RecyclerLeftToRight(v);
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
