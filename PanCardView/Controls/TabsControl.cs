using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using PanCardView.Behaviors;
using PanCardView.Extensions;
using PanCardView.Utility;
using Xamarin.Forms;
using static System.Math;

namespace PanCardView.Controls
{
    public class TabsControl : AbsoluteLayout
    {
        public static readonly BindableProperty DiffProperty = BindableProperty.Create(nameof(Diff), typeof(double), typeof(TabsControl), 0.0, propertyChanged: (bindable, oldValue, newValue) =>
        {
            bindable.AsTabsView().UpdateStripePosition();
        });

        public static readonly BindableProperty MaxDiffProperty = BindableProperty.Create(nameof(MaxDiff), typeof(double), typeof(TabsControl), 0.0, propertyChanged: (bindable, oldValue, newValue) =>
        {
            bindable.AsTabsView().UpdateStripePosition();
        });

        public static readonly BindableProperty ItemsCountProperty = BindableProperty.Create(nameof(ItemsCount), typeof(int), typeof(TabsControl), -1);

        public static readonly BindableProperty SelectedIndexProperty = BindableProperty.Create(nameof(SelectedIndex), typeof(int), typeof(TabsControl), 0, BindingMode.TwoWay);

        public static readonly BindableProperty ItemTemplateProperty = BindableProperty.Create(nameof(ItemTemplate), typeof(DataTemplate), typeof(TabsControl), null, propertyChanged: (bindable, oldValue, newValue) =>
        {
            bindable.AsTabsView().ResetItemsLayout();
        });

        public static readonly BindableProperty StripeColorProperty = BindableProperty.Create(nameof(StripeColor), typeof(Color), typeof(TabsControl), Color.CadetBlue, propertyChanged: (bindable, oldValue, newValue) =>
        {
            bindable.AsTabsView().ResetStripeView();
        });

        public static readonly BindableProperty StripeHeightProperty = BindableProperty.Create(nameof(StripeHeight), typeof(double), typeof(TabsControl), 3.0, propertyChanged: (bindable, oldValue, newValue) =>
        {
            bindable.AsTabsView().ResetStripeView();
        });

        public static readonly BindableProperty IsCyclicalProperty = BindableProperty.Create(nameof(IsCyclical), typeof(bool), typeof(TabsControl), false, propertyChanged: (bindable, oldValue, newValue) =>
        {
            bindable.AsTabsView().ResetItemsLayout();
        });

        public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable), typeof(TabsControl), null, propertyChanged: (bindable, oldValue, newValue) =>
        {
            bindable.AsTabsView().ResetItemsSource(oldValue as IEnumerable);
        });

        public static readonly BindableProperty IsUserInteractionRunningProperty = BindableProperty.Create(nameof(IsUserInteractionRunning), typeof(bool), typeof(TabsControl), true, propertyChanged: (bindable, oldValue, newValue) =>
        {
            bindable.AsTabsView().ResetVisibility();
        });

        public static readonly BindableProperty IsAutoInteractionRunningProperty = BindableProperty.Create(nameof(IsAutoInteractionRunning), typeof(bool), typeof(TabsControl), true, propertyChanged: (bindable, oldValue, newValue) =>
        {
            bindable.AsTabsView().ResetVisibility();
        });

        public static readonly BindableProperty ToFadeDurationProperty = BindableProperty.Create(nameof(ToFadeDuration), typeof(int), typeof(TabsControl), 0);

        public static readonly BindableProperty UseParentAsBindingContextProperty = BindableProperty.Create(nameof(UseParentAsBindingContext), typeof(bool), typeof(TabsControl), true);

        static TabsControl()
        {
        }

        private readonly BoxView _currentItemStripeView = new BoxView();
        private readonly BoxView _nextItemStripeView = new BoxView();
        private CancellationTokenSource _fadeAnimationTokenSource;

        private readonly StackLayout _itemsStackLayout = new StackLayout
        {
            Spacing = 0,
            Orientation = StackOrientation.Horizontal,
        };

        public TabsControl()
        {
            Children.Add(_itemsStackLayout, new Rectangle(0, 0, 1, 1), AbsoluteLayoutFlags.All);
            Children.Add(_currentItemStripeView, new Rectangle(0, 1, 0, 0), AbsoluteLayoutFlags.YProportional);
            Children.Add(_nextItemStripeView, new Rectangle(0, 1, 0, 0), AbsoluteLayoutFlags.YProportional);

            this.SetBinding(DiffProperty, nameof(CardsView.ProcessorDiff));
            this.SetBinding(MaxDiffProperty, nameof(Width));
            this.SetBinding(SelectedIndexProperty, nameof(CardsView.SelectedIndex));
            this.SetBinding(ItemsSourceProperty, nameof(CardsView.ItemsSource));
            this.SetBinding(IsCyclicalProperty, nameof(CardsView.IsCyclical));
            this.SetBinding(IsUserInteractionRunningProperty, nameof(CardsView.IsUserInteractionRunning));
            this.SetBinding(IsAutoInteractionRunningProperty, nameof(CardsView.IsAutoInteractionRunning));

            SetLayoutBounds(this, new Rectangle(.5, 1, -1, -1));
            SetLayoutFlags(this, AbsoluteLayoutFlags.PositionProportional);
            Behaviors.Add(new ProtectedControlBehavior());
        }

        public double Diff
        {
            get => (double)GetValue(DiffProperty);
            set => SetValue(DiffProperty, value);
        }

        public double MaxDiff
        {
            get => (double)GetValue(MaxDiffProperty);
            set => SetValue(MaxDiffProperty, value);
        }

        public int ItemsCount
        {
            get => (int)GetValue(ItemsCountProperty);
            set => SetValue(ItemsCountProperty, value);
        }

        public int SelectedIndex
        {
            get => (int)GetValue(SelectedIndexProperty);
            set => SetValue(SelectedIndexProperty, value);
        }

        public DataTemplate ItemTemplate
        {
            get => GetValue(ItemTemplateProperty) as DataTemplate;
            set => SetValue(ItemTemplateProperty, value);
        }

        public Color StripeColor
        {
            get => (Color)GetValue(StripeColorProperty);
            set => SetValue(StripeColorProperty, value);
        }

        public double StripeHeight
        {
            get => (double)GetValue(StripeHeightProperty);
            set => SetValue(StripeHeightProperty, value);
        }

        public bool IsCyclical
        {
            get => (bool)GetValue(IsCyclicalProperty);
            set => SetValue(IsCyclicalProperty, value);
        }

        public IEnumerable ItemsSource
        {
            get => GetValue(ItemsSourceProperty) as IEnumerable;
            set => SetValue(ItemsSourceProperty, value);
        }

        public bool IsUserInteractionRunning
        {
            get => (bool)GetValue(IsUserInteractionRunningProperty);
            set => SetValue(IsUserInteractionRunningProperty, value);
        }

        public bool IsAutoInteractionRunning
        {
            get => (bool)GetValue(IsAutoInteractionRunningProperty);
            set => SetValue(IsAutoInteractionRunningProperty, value);
        }

        public int ToFadeDuration
        {
            get => (int)GetValue(ToFadeDurationProperty);
            set => SetValue(ToFadeDurationProperty, value);
        }

        public bool UseParentAsBindingContext
        {
            get => (bool)GetValue(UseParentAsBindingContextProperty);
            set => SetValue(UseParentAsBindingContextProperty, value);
        }

        public object this[int index] => ItemsSource?.FindValue(index);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void Preserve()
        {
        }

        protected virtual async void ResetVisibility(uint? appearingTime = null, Easing appearingEasing = null, uint? dissappearingTime = null, Easing disappearingEasing = null)
        {
            _fadeAnimationTokenSource?.Cancel();

            if (ItemsCount == 0)
            {
                Opacity = 0;
                IsVisible = false;
                return;
            }

            if (ToFadeDuration <= 0)
            {
                Opacity = 1;
                IsVisible = true;
                return;
            }

            if (IsUserInteractionRunning || IsAutoInteractionRunning)
            {
                IsVisible = true;

                await new AnimationWrapper(v => Opacity = v, Opacity, 1)
                    .Commit(this, nameof(ResetVisibility), 16, appearingTime ?? 330, appearingEasing ?? Easing.CubicInOut);
                return;
            }

            _fadeAnimationTokenSource = new CancellationTokenSource();
            var token = _fadeAnimationTokenSource.Token;

            await Task.Delay(ToFadeDuration);
            if (token.IsCancellationRequested)
            {
                return;
            }

            await new AnimationWrapper(v => Opacity = v, Opacity, 0)
                .Commit(this, nameof(ResetVisibility), 16, dissappearingTime ?? 330, disappearingEasing ?? Easing.SinOut);

            if (token.IsCancellationRequested)
            {
                return;
            }
            IsVisible = false;
        }

        protected override void OnParentSet()
        {
            base.OnParentSet();
            if (UseParentAsBindingContext && Parent is CardsView)
            {
                BindingContext = Parent;
            }
            ResetItemsLayout();
        }

        private void ResetItemsLayout()
        {
            if (Parent == null)
            {
                return;
            }

            try
            {
                BatchBegin();
                _itemsStackLayout.Children.Clear();
                if (ItemsSource == null)
                {
                    return;
                }

                ItemsCount = ItemsSource.Count();
                foreach (var item in ItemsSource)
                {
                    var itemView = ItemTemplate?.SelectTemplate(item)?.CreateView() ?? item as View;
                    if (itemView == null)
                    {
                        return;
                    }

                    itemView.BindingContext = item;
                    itemView.GestureRecognizers.Add(new TapGestureRecognizer
                    {
                        CommandParameter = item,
                        Command = new Command(p =>
                        {
                            SelectedIndex = ItemsSource.FindIndex(p);
                        })
                    });
                    _itemsStackLayout.Children.Add(itemView);
                }

                ResetStripeViewNonBatch();
                UpdateStripePositionNonBatch();
            }
            finally
            {
                BatchCommit();
            }
        }

        private void ResetItemsSource(IEnumerable oldCollection)
        {
            if (oldCollection is INotifyCollectionChanged oldObservableCollection)
            {
                oldObservableCollection.CollectionChanged -= OnObservableCollectionChanged;
            }

            if (ItemsSource is INotifyCollectionChanged observableCollection)
            {
                observableCollection.CollectionChanged += OnObservableCollectionChanged;
            }

            OnObservableCollectionChanged(null, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        private void OnObservableCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        => ResetItemsLayout();

        private void ResetStripeView()
        {
            try
            {
                BatchBegin();
                ResetStripeViewNonBatch();
            }
            finally
            {
                BatchCommit();
            }
        }

        private void ResetStripeViewNonBatch()
        {
            _itemsStackLayout.Margin = new Thickness(0, 0, 0, StripeHeight);
            _currentItemStripeView.Color = StripeColor;
            _nextItemStripeView.Color = StripeColor;
        }

        private void UpdateStripePosition()
        {
            try
            {
                BatchBegin();
                UpdateStripePositionNonBatch();
            }
            finally
            {
                BatchCommit();
            }
        }

        private void UpdateStripePositionNonBatch()
        {
            var diff = Diff;
            var selectedIndex = SelectedIndex.ToCyclicalIndex(ItemsCount);
            if (selectedIndex < 0)
            {
                return;
            }

            var affectedIndex = diff < 0
                    ? (selectedIndex + 1)
                    : diff > 0
                        ? (selectedIndex - 1)
                        : selectedIndex;

            if (IsCyclical)
            {
                affectedIndex = affectedIndex.ToCyclicalIndex(ItemsCount);
            }

            if (affectedIndex < 0 || affectedIndex >= ItemsCount)
            {
                return;
            }

            var itemProgress = Min(Abs(diff) / MaxDiff, 1);

            var currentItemView = _itemsStackLayout.Children[SelectedIndex];
            var affectedItemView = _itemsStackLayout.Children[affectedIndex];
            if (diff <= 0)
            {
                CalculateStripePosition(currentItemView, affectedItemView, itemProgress, selectedIndex > affectedIndex);
                return;
            }
            CalculateStripePosition(affectedItemView, currentItemView, 1 - itemProgress, selectedIndex < affectedIndex);
        }

        private void CalculateStripePosition(View firstView, View secondView, double itemProgress, bool needSecondStripe)
        {
            var x = firstView.X + firstView.Width * itemProgress;
            var width = firstView.Width * (1 - itemProgress) + secondView.Width * itemProgress;
            SetLayoutBounds(_currentItemStripeView, new Rectangle(x, 1, width, StripeHeight));
            if (needSecondStripe)
            {
                _nextItemStripeView.IsVisible = true;
                SetLayoutBounds(_nextItemStripeView, new Rectangle(secondView.X, 1, secondView.Width * itemProgress, StripeHeight));
                return;
            }
            _nextItemStripeView.IsVisible = false;
        }
    }
}
