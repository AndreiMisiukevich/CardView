using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PanCardView.Extensions;
using PanCardView.Processors;
using PanCardView.Enums;
using Xamarin.Forms;

namespace PanCardView
{
    [Obsolete("ISN'T FINISHED YET. CAN BE CHANGED.")]
    public class CoverFlowView : AbsoluteLayout
    {
        /// <summary>
        /// CoverFlow items source property.
        /// </summary>
        public static BindableProperty ItemsSourceProperty = BindableProperty.Create(nameof(ItemsSource), typeof(IList), typeof(CoverFlowView), null, propertyChanged: (bindable, oldValue, newValue) =>
        {
            bindable.AsCoverFlowView().GenerateCoverList(bindable);
        });

        /// <summary>
        /// The item template property.
        /// </summary>
        public static readonly BindableProperty ItemTemplateProperty = BindableProperty.Create(nameof(ItemTemplate), typeof(DataTemplate), typeof(CoverFlowView), propertyChanged: (bindable, oldValue, newValue) =>
        {
            bindable.AsCoverFlowView().GenerateCoverList(bindable);
        });

        /// <summary>
        /// The number of displayed views property.
        /// </summary>
        public static readonly BindableProperty NumberOfViewsProperty = BindableProperty.Create(nameof(NumberOfViews), typeof(int), typeof(CoverFlowView), 3, propertyChanged: (bindable, oldValue, newValue) =>
        {
            if ((double)newValue <= 0)
            {
                Console.WriteLine("Automation Number Of Views is not supported yet");
                throw new NotImplementedException("Automation Number Of Views is not supported yet");
            }
            bindable.AsCoverFlowView().GenerateCoverList(bindable);
        });

        /// <summary>
        /// The Spacing between items property.
        /// </summary>
        public static readonly BindableProperty SpacingProperty = BindableProperty.Create(nameof(Spacing), typeof(double), typeof(CoverFlowView), default(double));

        /// <summary>
        /// The Cyclical property.
        /// </summary>
        public static readonly BindableProperty IsCyclicalProperty = BindableProperty.Create(nameof(IsCyclical), typeof(bool), typeof(CoverFlowView), true);

        /// <summary>
        /// The First Item position property.
        /// </summary>
        public static readonly BindableProperty FirstItemPositionProperty = BindableProperty.Create(nameof(FirstItemPosition), typeof(CoverItemPosition), typeof(CoverFlowView), CoverItemPosition.Left);

        /// <summary>
        /// The view position property.
        /// </summary>
        public static readonly BindableProperty ViewAlignmentProperty = BindableProperty.Create(nameof(ViewAlignment), typeof(CoverItemPosition), typeof(CoverFlowView), CoverItemPosition.Center);

        private readonly PanGestureRecognizer _panGesture = new PanGestureRecognizer();
        private List<View> _recycledViews = new List<View>();
        private BaseCoverFlowProcessor ViewProcessor { get; }

        private bool IsViewsInited;
        private bool _isOrientationChanged;

        private double _width = -1;
        private double _height = -1;
        private double _tmpTotalX;

        public CoverFlowView(BaseCoverFlowProcessor baseCoverFlowProcessor)
        {
            ViewProcessor = baseCoverFlowProcessor;
            ViewProcessor.CoverFlow = this;
            IsClippedToBounds = true;

            SetPanGesture();
        }

        public CoverFlowView() : this(new BaseCoverFlowProcessor())
        {
        }

        /// <summary>
        /// Gets or set the displayed views list.
        /// </summary>
        /// <value>The displayed views list.</value>
        private IAbsoluteList<View> DisplayedViews => Children;

        /// <summary>
        /// Gets or set the recycled views list.
        /// </summary>
        /// <value>The recycled views list.</value>
        private List<View> RecycledViews
        {
            get => _recycledViews;
            set => _recycledViews = value ?? new List<View>();
        }

        /// <summary>
        /// Gets the maximun value on axis that translation could go.
        /// </summary>
        /// <value>the maximun value on axis that translation could go.</value>
        public double MaxGraphicAxis { get; private set; }

        /// <summary>
        /// Gets the space between each center's item views.
        /// </summary>
        /// <value>the space between each center's item views.</value>
        public double Space { get; private set; }

        /// <summary>
        /// Gets the allowed margin between the center of the item view
        /// and the edge of the CoverFlow.
        /// </summary>
        /// <value>the allowed margin between the center of the item view 
        /// and the edge of the CoverFlow.</value>
        public double MarginBorder { get; private set; }

        /// <summary>
        /// Gets the maximun item index on axis.
        /// </summary>
        /// <value>the maximun item index on axis.</value>
        public int ItemMaxOnAxis { get; set; }

        /// <summary>
        /// Gets the minimum item index on axis.
        /// </summary>
        /// <value>the minimum item index on axis.</value>
        public int ItemMinOnAxis { get; set; }

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
        /// <value>The first item position useful when is Cyclical.</value>
        public CoverItemPosition FirstItemPosition
        {
            get => (CoverItemPosition)GetValue(FirstItemPositionProperty);
            set => SetValue(FirstItemPositionProperty, value);
        }

        /// <summary>
        /// Gets or set the view alignment.
        /// </summary>
        /// <value>The view alignment.</value>
        public CoverItemPosition ViewAlignment
        {
            get => (CoverItemPosition)GetValue(ViewAlignmentProperty);
            set => SetValue(ViewAlignmentProperty, value);
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
                        var dragX = e.TotalX - _tmpTotalX;
                        OnDragging(dragX);
                        _tmpTotalX = e.TotalX;
                        return;
                    case GestureStatus.Canceled:
                    case GestureStatus.Completed:
                        OnDragEnd();
                        return;
                }
            }
            return;
        }

        public int VerifyIndex(int index)
        {
            if (IsCyclical)
            {
                if (index < 0)
                {
                    index += ItemsSource.Count;
                }
                else if (index >= ItemsSource.Count)
                {
                    index -= ItemsSource.Count;
                }
            }
            else
                if (index >= ItemsSource.Count || index < 0)
                {
                    index = -1;
                }
            return index;
        }

        protected void SetupLayout()
        {
            if (DisplayedViews.Any())
            {
                foreach (var view in DisplayedViews.Where(v => v != null))
                {
                    SetupBoundsView(view);
                }
            }
        }

        protected void SetupBoundsView(View view)
        {
            SetLayoutBounds(view, new Rectangle(0, 0, 1, 1));
            SetLayoutFlags(view, AbsoluteLayoutFlags.All);
        }


        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(_width, _height);

            if (Math.Abs(_width - width) > 0.1 || Math.Abs(_height - height) > 0.1)
            {
                _isOrientationChanged = true;
            }

            if (width < 0 || height < 0 || !DisplayedViews.Any())
            {
                return;
            }

            if (_isOrientationChanged)
            {
                Space = (Width / NumberOfViews) + Spacing;
                MaxGraphicAxis = (Width + Space) / 2;
                MarginBorder = Space / 2;
                OnOrientationChanged(width, height);
                _isOrientationChanged = false;
            }

            _width = width;
            _height = height;

            if (!IsViewsInited)
            {
                ViewProcessor.HandleInitViews(DisplayedViews, ViewAlignment);
                BindingItemsToViews();
                IsViewsInited = true;
            }
        }

        /* Don't Erase
         * For Debug Only
         * 
         * Use this OnSizeAllocated to see the logic on Horizontal Orientation
         * After inition on Vertical Orientation.
         * 
        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            if (width < 0 || height < 0 || IsViewsInited && DisplayedViews.Any())
                return;

            Space = (Width / NumberOfViews) + Spacing;
            MaxGraphicAxis = (width + Space) / 2;
            MarginBorder = Space / 2;
            ViewProcessor.HandleInitViews(DisplayedViews, ViewPosition);
            BindingItemsToViews();
            IsViewsInited = true;
        }
        */

        protected virtual void OnOrientationChanged(double w, double h)
        {
            var coefWidth = w / _width;
            var coefHeight = h / _height;
            foreach (var v in DisplayedViews)
            {
                v.TranslationX *= coefWidth;
            }
        }

        protected virtual void BindingItemsToViews()
        {
            // Positive Views
            var positiveViews = DisplayedViews.Where(v => v.TranslationX >= (int)FirstItemPosition * (Width / 2 - MarginBorder)).OrderBy(v => v.TranslationX);
            BindingPositiveViews(positiveViews);

            // Negative Views
            var negativeViews = DisplayedViews.Where(v => v.TranslationX < (int)FirstItemPosition * (Width / 2 - MarginBorder)).OrderBy(v => v.TranslationX);
            BindingNegativeViews(negativeViews);
        }

        protected virtual void BindingPositiveViews(IEnumerable<View> positiveViews)
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

        protected virtual void BindingNegativeViews(IEnumerable<View> negativeViews)
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
                {
                    v.IsVisible = false;
                }
            }
        }

        protected virtual void GenerateCoverList(object bindable)
        {
            if (bindable is CoverFlowView CoverFlow && CoverFlow.ItemTemplate is DataTemplate itemTemplate && CoverFlow.ItemsSource is IList itemsSource)
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

        protected virtual View GenerateView(object context)
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

        private int GetMiddleIndex()
        {
            var CloseToMiddle = DisplayedViews.Where(v => v.IsVisible == true).OrderBy(v => Math.Abs((v.TranslationX)));
            return DisplayedViews.IndexOf(CloseToMiddle.First());
        }

        private int GetIndexCloseToAlignment(CoverItemPosition viewPosition)
        {
            var position = (Width / 2 - MarginBorder) * (int)viewPosition;
            var CloseToPositionView = DisplayedViews.Where(v => v.IsVisible == true).OrderBy(v => Math.Abs((v.TranslationX - position)));
            return DisplayedViews.IndexOf(CloseToPositionView.First());
        }

        private void SetPanGesture()
        {
            _panGesture.PanUpdated += OnPanUpdated;
            GestureRecognizers.Add(_panGesture);
        }

        private void OnDragStarted()
        {
            this.AbortAnimation("AlignViews");
            _tmpTotalX = 0;
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

        /* OnDragEnd is not well developped
         * 
         * The Idea would be at runtime(during animation)
         * To add old View to recycledList when they are far away(Math.Abs(translate) > maxTranslate)
         * And recycle a view and display it at the other side.
         * And give at this new display view the end of the animatiom(just the end :))
         * 
         */
        private void OnDragEnd()
        {
            var position = ViewAlignment;

            //For Simple list (End & Start List)
            if (!IsCyclical && ViewAlignment != CoverItemPosition.Center && _tmpTotalX < 0 && ItemMaxOnAxis == ItemsSource.Count - 1)
            {
                position = CoverItemPosition.Right;
            }
            if (!IsCyclical && ViewAlignment != CoverItemPosition.Center && _tmpTotalX > 0 && ItemMinOnAxis == 0)
            {
                position = CoverItemPosition.Left;
            }

            double border = (Width / 2 - MarginBorder) * (int)position;
            int indexCloseToPos = GetIndexCloseToAlignment(position);
            var dragX = border - DisplayedViews[indexCloseToPos].TranslationX;

            var direction = (dragX > 0) ? AnimationDirection.Prev : AnimationDirection.Next;

            //Forced Recycler View to Display Item before animation running 
            // Need to be deleted
            for (var i = Math.Abs(dragX) / Space; i > 1; --i)
            {
                RecyclerView(direction, true);
            }

            //The animation need refacto Too by doing the recycle logic too.
            ViewProcessor.HandlePanApply(DisplayedViews, dragX, position, RecycledViews);

        }

        private void RemoveUnDisplayedViews()
        {
            foreach (var v in RecycledViews)
            {
                if (DisplayedViews.Contains(v))
                {
                    DisplayedViews.Remove(v);
                }
            }
        }

        private View RecyclerView(AnimationDirection direction, bool force = false)
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
    }
}
