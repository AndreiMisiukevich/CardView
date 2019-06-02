using Xamarin.Forms;
using PanCardView.Behaviors;
using System.Threading;
using PanCardView.Extensions;
using PanCardView.Utility;
using System.Threading.Tasks;
using System.ComponentModel;

namespace PanCardView.Controls
{
    public class ArrowControl : ContentView
    {
        public static readonly BindableProperty SelectedIndexProperty = BindableProperty.Create(nameof(SelectedIndex), typeof(int), typeof(ArrowControl), 0, BindingMode.TwoWay, propertyChanged: (bindable, oldValue, newValue) =>
        {
            bindable.AsArrowControl().ResetVisibility();
        });

        public static readonly BindableProperty ItemsCountProperty = BindableProperty.Create(nameof(ItemsCount), typeof(int), typeof(ArrowControl), 0, propertyChanged: (bindable, oldValue, newValue) =>
        {
            bindable.AsArrowControl().ResetVisibility();
        });

        public static readonly BindableProperty IsCyclicalProperty = BindableProperty.Create(nameof(IsCyclical), typeof(bool), typeof(ArrowControl), false, propertyChanged: (bindable, oldValue, newValue) =>
        {
            bindable.AsArrowControl().ResetVisibility();
        });

        public static readonly BindableProperty IsUserInteractionRunningProperty = BindableProperty.Create(nameof(IsUserInteractionRunning), typeof(bool), typeof(ArrowControl), true, propertyChanged: (bindable, oldValue, newValue) =>
        {
            bindable.AsArrowControl().ResetVisibility();
        });

        public static readonly BindableProperty IsAutoInteractionRunningProperty = BindableProperty.Create(nameof(IsAutoInteractionRunning), typeof(bool), typeof(ArrowControl), true, propertyChanged: (bindable, oldValue, newValue) =>
        {
            bindable.AsArrowControl().ResetVisibility();
        });

        public static readonly BindableProperty IsRightToLeftFlowDirectionEnabledProperty = BindableProperty.Create(nameof(IsRightToLeftFlowDirectionEnabled), typeof(bool), typeof(ArrowControl), false, propertyChanged: (bindable, oldValue, newValue) =>
        {
            bindable.AsArrowControl().OnIsRightToLeftFlowDirectionEnabledChnaged();
        });

        public static readonly BindableProperty ImageSourceProperty = BindableProperty.Create(nameof(ImageSource), typeof(ImageSource), typeof(ArrowControl), defaultValueCreator: b => b.AsArrowControl().DefaultImageSource, propertyChanged: (bindable, oldValue, newValue) =>
        {
            bindable.AsArrowControl().OnImageSourceChanged();
        });

        public static readonly BindableProperty UseParentAsBindingContextProperty = BindableProperty.Create(nameof(UseParentAsBindingContext), typeof(bool), typeof(ArrowControl), true);

        public static readonly BindableProperty ToFadeDurationProperty = BindableProperty.Create(nameof(ToFadeDuration), typeof(int), typeof(ArrowControl), 0);

        public static readonly BindableProperty IsRightProperty = BindableProperty.Create(nameof(IsRight), typeof(bool), typeof(ArrowControl), true);

        private CancellationTokenSource _fadeAnimationTokenSource;

        public int SelectedIndex
        {
            get => (int)GetValue(SelectedIndexProperty);
            set => SetValue(SelectedIndexProperty, value);
        }

        public int ItemsCount
        {
            get => (int)GetValue(ItemsCountProperty);
            set => SetValue(ItemsCountProperty, value);
        }

        public bool IsCyclical
        {
            get => (bool)GetValue(IsCyclicalProperty);
            set => SetValue(IsCyclicalProperty, value);
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

        public bool UseParentAsBindingContext
        {
            get => (bool)GetValue(UseParentAsBindingContextProperty);
            set => SetValue(UseParentAsBindingContextProperty, value);
        }

        public bool IsRightToLeftFlowDirectionEnabled
        {
            get => (bool)GetValue(IsRightToLeftFlowDirectionEnabledProperty);
            set => SetValue(IsRightToLeftFlowDirectionEnabledProperty, value);
        }

        public ImageSource ImageSource
        {
            get => (ImageSource)GetValue(ImageSourceProperty);
            set => SetValue(ImageSourceProperty, value);
        }

        public int ToFadeDuration
        {
            get => (int)GetValue(ToFadeDurationProperty);
            set => SetValue(ToFadeDurationProperty, value);
        }

        public bool IsRight
        {
            get => (bool)GetValue(IsRightProperty);
            set => SetValue(IsRightProperty, value);
        }

        protected Image ContentImage { get; }

        public ArrowControl()
        {
            Content = ContentImage = new Image
            {
                Aspect = Aspect.AspectFill,
                VerticalOptions = LayoutOptions.FillAndExpand,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                InputTransparent = true,
                Source = ImageSource
            };

            WidthRequest = 40;
            HeightRequest = 40;

            Margin = new Thickness(20, 10);
            AbsoluteLayout.SetLayoutFlags(this, AbsoluteLayoutFlags.PositionProportional);

            this.SetBinding(SelectedIndexProperty, nameof(CardsView.SelectedIndex));
            this.SetBinding(ItemsCountProperty, nameof(CardsView.ItemsCount));
            this.SetBinding(IsCyclicalProperty, nameof(CardsView.IsCyclical));
            this.SetBinding(IsUserInteractionRunningProperty, nameof(CardsView.IsUserInteractionRunning));
            this.SetBinding(IsAutoInteractionRunningProperty, nameof(CardsView.IsAutoInteractionRunning));
            this.SetBinding(IsRightToLeftFlowDirectionEnabledProperty, nameof(CardsView.IsRightToLeftFlowDirectionEnabled));

            Behaviors.Add(new ProtectedControlBehavior());

            GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(OnTapped)
            });
        }

        protected virtual ImageSource DefaultImageSource => null;

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void Preserve()
        {
        }

        protected override void OnParentSet()
        {
            base.OnParentSet();
            if (UseParentAsBindingContext)
            {
                BindingContext = Parent;
            }
        }

        protected virtual async void ResetVisibility(uint? appearingTime = null, Easing appearingEasing = null, uint? dissappearingTime = null, Easing disappearingEasing = null)
        {
            _fadeAnimationTokenSource?.Cancel();

            var isAvailable = CheckAvailability();

            IsEnabled = isAvailable;

            if (ToFadeDuration <= 0 && isAvailable)
            {
                IsVisible = true;

                await new AnimationWrapper(v => Opacity = v, Opacity, 1)
                    .Commit(this, nameof(ResetVisibility), 16, appearingTime ?? 330, appearingEasing ?? Easing.CubicInOut);
                return;
            }

            if (isAvailable && (IsUserInteractionRunning || IsAutoInteractionRunning))
            {
                IsVisible = true;

                await new AnimationWrapper(v => Opacity = v, Opacity, 1)
                    .Commit(this, nameof(ResetVisibility), 16, appearingTime ?? 330, appearingEasing ?? Easing.CubicInOut);
                return;
            }

            _fadeAnimationTokenSource = new CancellationTokenSource();
            var token = _fadeAnimationTokenSource.Token;

            await Task.Delay(ToFadeDuration > 0 && isAvailable ? ToFadeDuration : 5);
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

        protected virtual void OnIsRightToLeftFlowDirectionEnabledChnaged()
        {
            Rotation = IsRightToLeftFlowDirectionEnabled ? 180 : 0;
            ResetVisibility();
        }

        protected virtual void OnImageSourceChanged() => ContentImage.Source = ImageSource;

        protected virtual void OnTapped()
        {
            if (IsUserInteractionRunning || IsAutoInteractionRunning)
            {
                return;
            }
            SelectedIndex = (SelectedIndex + (IsRight ? 1 : -1)).ToCyclingIndex(ItemsCount);
        }

        private bool CheckAvailability()
        {
            if (ItemsCount < 2)
            {
                return false;
            }

            if (IsCyclical)
            {
                return true;
            }

            var cyclingIndex = SelectedIndex.ToCyclingIndex(ItemsCount);

            if (cyclingIndex == (IsRight ? ItemsCount - 1 : 0))
            {
                return false;
            }

            return true;
        }
    }
}
