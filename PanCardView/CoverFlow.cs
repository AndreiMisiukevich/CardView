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
using System.Threading.Tasks;

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
            if ((double)newValue <= 0)
            {
                Console.WriteLine("Automation Number Of Views is not supported yet");
                throw new NotImplementedException("Automation Number Of Views is not supported yet");
            }
            bindable.AsCoverView().GenerateCoverList(bindable);
        });

        /// <summary>
        /// The Spacing between items property.
        /// </summary>
        public static readonly BindableProperty SpacingProperty = BindableProperty.Create(nameof(Spacing), typeof(double), typeof(CoverFlow), 0.0);

        /// <summary>
        /// The Cyclical property.
        /// </summary>
        public static readonly BindableProperty IsCyclicalProperty = BindableProperty.Create(nameof(IsCyclical), typeof(bool), typeof(CoverFlow), true);

        /// <summary>
        /// The First Item position property.
        /// </summary>
        public static readonly BindableProperty FirstItemPositionProperty = BindableProperty.Create(nameof(FirstItemPosition), typeof(Position), typeof(CoverFlow), Position.Left);


        /// <summary>
        /// The view position property.
        /// </summary>
        public static readonly BindableProperty ViewPositionProperty = BindableProperty.Create(nameof(ViewPosition), typeof(Position), typeof(CoverFlow), Position.Center);


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
        /// Gets or set the space between Items.
        /// </summary>
        /// <value>The Spacing value.</value>
        public double Spacing
        {
            get => (double)GetValue(SpacingProperty);
            set => SetValue(SpacingProperty, value);
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
        public Position FirstItemPosition
        {
            get => (Position)GetValue(FirstItemPositionProperty);
            set => SetValue(FirstItemPositionProperty, value);
        }

        /// <summary>
        /// Gets or set the view position.
        /// </summary>
        /// <value>The first view position.</value>
        public Position ViewPosition
        {
            get => (Position)GetValue(ViewPositionProperty);
            set => SetValue(ViewPositionProperty, value);
        }

        IAbsoluteList<View> DisplayedViews => this.Children;

        // Just a Unique Template. but will be available for each template at the end.
        List<View> RecycledViews;

        // Processor
        BaseCoverFlowProcessor ViewProcessor { get; }

        public double MaxGraphicAxis = 0;
        public double Space = 0;
        public double MarginBorder = 0;

        public int ItemMaxOnAxis;
        public int ItemMinOnAxis;
        public int MiddleIndex;
        private PanGestureRecognizer _panGesture;
        private double TmpTotalX = 0;
        bool _isViewsInited = false;

        public CoverFlow(BaseCoverFlowProcessor baseCoverFlowProcessor)
        {
            this.ViewProcessor = baseCoverFlowProcessor;
            this.ViewProcessor.CoverFlow = this;
            _constructor();
        }

        public CoverFlow()
        {
            this.ViewProcessor = new BaseCoverFlowProcessor(this);
            _constructor();
        }

        void _constructor()
        {
            this.IsClippedToBounds = true;
            RecycledViews = new List<View>();

            SetPanGesture();
        }

        void SetupLayout()
        {
            if (DisplayedViews.Any())
            {
                foreach (var view in DisplayedViews.Where(v => v != null))
                {
                    SetupBoundsView(view);
                }
            }
        }

        void SetupBoundsView(View view)
        {
            SetLayoutBounds(view, new Rectangle(0, 0, 1, 1));
            SetLayoutFlags(view, AbsoluteLayoutFlags.All);
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            if (width < 0 || height < 0 || _isViewsInited && DisplayedViews.Any())
                return;

            Space = (Width / NumberOfViews) + Spacing;
            MaxGraphicAxis = (width + Space) / 2;
            MarginBorder = Space / 2;
            ViewProcessor.HandleInitViews(DisplayedViews, ViewPosition);
            BindingItemsToViews();
            _isViewsInited = true;
        }

        public void BindingItemsToViews()
        {
            // Positive Views
            var positiveViews = DisplayedViews.Where(v => v.TranslationX >= (int)FirstItemPosition * (Width / 2 - MarginBorder)).OrderBy(v => v.TranslationX);
            BindingPositiveViews(positiveViews);

            // Negative Views
            var negativeViews = DisplayedViews.Where(v => v.TranslationX < (int)FirstItemPosition * (Width / 2 - MarginBorder)).OrderBy(v => v.TranslationX);
            BindingNegativeViews(negativeViews);
        }

        public void BindingPositiveViews(IEnumerable<View> positiveViews)
        {
            var index = 0;
            ItemMinOnAxis = index;
            foreach (var v in positiveViews)
            {
                v.BindingContext = ItemsSource[VerifyIndex(index)];
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
                    v.IsVisible = true;
                    v.BindingContext = ItemsSource[VerifyIndex(index)];
                    ++index;
                }
            }
            else
            {
                foreach (var v in negativeViews)
                    v.IsVisible = false;
            }
        }

        public void GenerateCoverList(object bindable)
        {
            if (bindable is CoverFlow CoverFlow && CoverFlow.ItemTemplate is DataTemplate itemTemplate && CoverFlow.ItemsSource is IList itemsSource)
            {
                //DataTemplate Selector?!... Need refacto
                //And Binding
                for (int i = 0; i < NumberOfViews; ++i)
                {
                    var newView = GenerateView(null);
                    DisplayedViews.Add(newView);
                }

                SetupLayout();
            }
        }

        public View GenerateView(object context)
        {
            var template = ItemTemplate;
            if (template is DataTemplateSelector selector)
            {
                Console.WriteLine("DataTemplateSelector is not supported yet");
                throw new NotImplementedException("DataTemplateSelector is not supported yet");
                //template = selector.SelectTemplate(context, this);
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
            this.AbortAnimation("CenterViews");
            TmpTotalX = 0;
        }

        private void OnDragging(double dragX)
        {
            var direction = (dragX > 0) ? AnimationDirection.Prev : AnimationDirection.Next;
            if (DisplayedViews.Any())
            {
                ViewProcessor.HandlePanChanged(DisplayedViews, dragX, direction, RecycledViews);

                //Remove unseen views that have been added to recycled List
                RemoveUnDisplayedViews();

                //Recycler View to Display new Item
                RecyclerView(direction);
            }
        }

        private void OnDragEnd()
        {
            CenterViews();
        }

        public void RemoveUnDisplayedViews()
        {
            foreach (var v in RecycledViews)
            {
                if (DisplayedViews.Contains(v))
                {
                    DisplayedViews.Remove(v);
                }
            }
        }

        public View RecyclerView(AnimationDirection direction, bool force = false)
        {
            var index = -1;
            var translate = 0.0;
            View view = null;
            if (direction == AnimationDirection.Prev)
            {
                var MinViewonAxis = DisplayedViews.Min(v => v.TranslationX);
                if (Math.Abs(MinViewonAxis + MaxGraphicAxis) > MarginBorder || force)
                {
                    index = VerifyIndex(ItemMinOnAxis - 1);
                    ItemMinOnAxis = (index != -1) ? index : ItemMinOnAxis;
                    translate = MinViewonAxis + (int)direction * Space;
                }
            }
            else if (direction == AnimationDirection.Next)
            {
                var MaxViewonAxis = DisplayedViews.Max(v => v.TranslationX);
                if (Math.Abs(MaxViewonAxis - MaxGraphicAxis) > MarginBorder || force)
                {
                    index = VerifyIndex(ItemMaxOnAxis + 1);
                    ItemMaxOnAxis = (index != -1) ? index : ItemMaxOnAxis;
                    translate = MaxViewonAxis + (int)direction * Space;
                }
            }
            if (index != -1)
            {
                if (RecycledViews.Any()) //Check for Type Template will be Here !
                {
                    view = RecycledViews[0]; // Recycle old one !!!
                    view.BindingContext = ItemsSource[index];
                    RecycledViews.Remove(view);
                }
                else
                {
                    view = GenerateView(ItemsSource[index]); //create new One !!!!
                    SetupBoundsView(view);
                }
                view.TranslationX = translate;

                DisplayedViews.Add(view);

                view.IsVisible = true;
            }
            return view;
        }

        public void CenterViews()
        {
            var MiddleIndex = GetMiddleIndex();
            var dragX = -DisplayedViews[MiddleIndex].TranslationX;
            var direction = (dragX > 0) ? AnimationDirection.Prev : AnimationDirection.Next;

            Animation a = new Animation();

            var maxTranslate = MaxGraphicAxis + MarginBorder;
            foreach (var v in DisplayedViews)
            {
                var translate = v.TranslationX + dragX;
                if (Math.Abs(translate) > maxTranslate
                    && DisplayedViews.Count() > 1)
                {
                    if (direction == AnimationDirection.Prev) // Movement --> right
                        ItemMaxOnAxis = VerifyIndex(ItemMaxOnAxis - 1);
                    else if (direction == AnimationDirection.Next)  // Movement <-- Left
                        ItemMinOnAxis = VerifyIndex(ItemMinOnAxis + 1);
                    RecycledViews.Add(v);
                    v.IsVisible = false;
                }
                else
                {
                    a.Add(0, 1, new Animation(f => v.TranslationX = f, v.TranslationX, translate, Easing.SinOut, null));
                }
            }
            a.Commit(this, "CenterViews", 60, 400);
        }
    }
}
