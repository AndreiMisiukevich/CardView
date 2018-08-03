using PanCardView.Behaviors;
using PanCardView.Controls;
using PanCardView.Enums;
using PanCardView.Extensions;
using PanCardView.Processors;
using PanCardView.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using static System.Math;
using System.Threading;
using System.Runtime.CompilerServices;
using PanCardView.EventArgs;
using PanCardView.Delegates;
using System.Reflection;

namespace PanCardView
{
	public class CardsView : AbsoluteLayout
	{
		public static readonly BindableProperty SelectedIndexProperty = BindableProperty.Create(nameof(SelectedIndex), typeof(int), typeof(CardsView), -1, BindingMode.TwoWay, propertyChanged: (bindable, oldValue, newValue) =>
		{
			var view = bindable.AsCardsView();
			view.SetSelectedItem();
			view.OldIndex = (int)oldValue;
			if (view.ShouldIgnoreSetCurrentView)
			{
				view.ShouldIgnoreSetCurrentView = false;
				return;
			}
			view.SetCurrentView();
		});

		public static readonly BindableProperty SelectedItemProperty = BindableProperty.Create(nameof(SelectedItem), typeof(object), typeof(CardsView), null, BindingMode.TwoWay, propertyChanged: (bindable, oldValue, newValue) =>
		{
			var view = bindable.AsCardsView();
			view.SelectedIndex = view.ItemsSource?.IndexOf(newValue) ?? -1;
		});

		public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(nameof(ItemsSource), typeof(IList), typeof(CardsView), null, propertyChanged: (bindable, oldValue, newValue) =>
		{
			bindable.AsCardsView().SetItemsCount();
		});

		public static readonly BindableProperty ItemTemplateProperty = BindableProperty.Create(nameof(ItemTemplate), typeof(DataTemplate), typeof(CardsView), propertyChanged: (bindable, oldValue, newValue) =>
		{
			bindable.AsCardsView().SetCurrentView();
		});

		public static readonly BindableProperty BackViewsDepthProperty = BindableProperty.Create(nameof(BackViewsDepth), typeof(int), typeof(CardsView), 1, propertyChanged: (bindable, oldValue, newValue) =>
		{
			bindable.AsCardsView().SetCurrentView();
		});

		public static readonly BindableProperty IsRightToLeftFlowDirectionEnabledProperty = BindableProperty.Create(nameof(IsRightToLeftFlowDirectionEnabled), typeof(bool), typeof(CardsView), false, propertyChanged: (bindable, oldValue, newValue) =>
		{
			bindable.AsCardsView().SetCurrentView();
		});

		public static readonly BindableProperty SlideShowDurationProperty = BindableProperty.Create(nameof(SlideShowDuration), typeof(int), typeof(CardsView), 0, propertyChanged: (bindable, oldValue, newValue) =>
		{
			bindable.AsCardsView().AdjustSlideShow();
		});

		public static readonly BindableProperty IsUserInteractionRunningProperty = BindableProperty.Create(nameof(IsUserInteractionRunning), typeof(bool), typeof(CardsView), false, BindingMode.OneWayToSource, propertyChanged: (bindable, oldValue, newValue) =>
		{
			bindable.AsCardsView().AdjustSlideShow((bool)newValue);
		});

		public static readonly BindableProperty IsAutoInteractionRunningProperty = BindableProperty.Create(nameof(IsAutoInteractionRunning), typeof(bool), typeof(CardsView), false, BindingMode.OneWayToSource, propertyChanged: (bindable, oldValue, newValue) =>
		{
			bindable.AsCardsView().AdjustSlideShow((bool)newValue);
		});

		public static BindableProperty ItemsCountProperty = BindableProperty.Create(nameof(ItemsCount), typeof(int), typeof(CardsView), -1);

		public static readonly BindableProperty IsUserInteractionEnabledProperty = BindableProperty.Create(nameof(IsUserInteractionEnabled), typeof(bool), typeof(CardsView), true);

		public static readonly BindableProperty MoveDistanceProperty = BindableProperty.Create(nameof(MoveDistance), typeof(double), typeof(CardsView), -1.0);

		public static readonly BindableProperty MoveWidthPercentageProperty = BindableProperty.Create(nameof(MoveWidthPercentage), typeof(double), typeof(CardsView), .325);

		public static readonly BindableProperty IsOnlyForwardDirectionProperty = BindableProperty.Create(nameof(IsOnlyForwardDirection), typeof(bool), typeof(CardsView), false);

		public static readonly BindableProperty IsViewCacheEnabledProperty = BindableProperty.Create(nameof(IsViewCacheEnabled), typeof(bool), typeof(CardsView), true);

		public static readonly BindableProperty UserInteractionDelayProperty = BindableProperty.Create(nameof(UserInteractionDelay), typeof(int), typeof(CardsView), 200);

		public static readonly BindableProperty IsUserInteractionInCourseProperty = BindableProperty.Create(nameof(IsUserInteractionInCourse), typeof(bool), typeof(CardsView), true);

		public static readonly BindableProperty IsCyclicalProperty = BindableProperty.Create(nameof(IsCyclical), typeof(bool), typeof(CardsView), false);

        public static readonly BindableProperty IsAutoNavigatingAimationEnabledProperty = BindableProperty.Create(nameof(IsAutoNavigatingAimationEnabled), typeof(bool), typeof(CardsView), true);

		public static readonly BindableProperty MaxChildrenCountProperty = BindableProperty.Create(nameof(MaxChildrenCount), typeof(int), typeof(CardsView), 12);

		public static readonly BindableProperty DesiredMaxChildrenCountProperty = BindableProperty.Create(nameof(DesiredMaxChildrenCount), typeof(int), typeof(CardsView), 6);

		public static readonly BindableProperty SwipeThresholdDistanceProperty = BindableProperty.Create(nameof(SwipeThresholdDistance), typeof(double), typeof(CardsView), 17.0);

		public static readonly BindableProperty MoveThresholdDistanceProperty = BindableProperty.Create(nameof(MoveThresholdDistance), typeof(double), typeof(CardsView), 3.0);

		public static readonly BindableProperty SwipeThresholdTimeProperty = BindableProperty.Create(nameof(SwipeThresholdTime), typeof(TimeSpan), typeof(CardsView), TimeSpan.FromMilliseconds(Device.RuntimePlatform == Device.Android ? 100 : 60));

		public static readonly BindableProperty UserInteractedCommandProperty = BindableProperty.Create(nameof(UserInteractedCommand), typeof(ICommand), typeof(CardsView), null);

		public static readonly BindableProperty ItemDisappearingCommandProperty = BindableProperty.Create(nameof(ItemDisappearingCommand), typeof(ICommand), typeof(CardsView), null);

		public static readonly BindableProperty ItemAppearingCommandProperty = BindableProperty.Create(nameof(ItemAppearingCommand), typeof(ICommand), typeof(CardsView), null);

		public static readonly BindableProperty ItepTappedCommandProperty = BindableProperty.Create(nameof(ItemTappedCommand), typeof(ICommand), typeof(CardsView), null);

		public event CardsViewUserInteractedHandler UserInteracted;
		public event CardsViewItemDisappearingHandler ItemDisappearing;
		public event CardsViewItemAppearingHandler ItemAppearing;
		public event CardsViewCardTappedHandler ItemTapped;

		private readonly object _childLocker = new object();
		private readonly object _viewsInUseLocker = new object();
		private readonly object _setCurrentViewLocker = new object();
        private readonly object _orientationChangedLocker = new object();

		private readonly Dictionary<object, List<View>> _viewsPool = new Dictionary<object, List<View>>();
		private readonly Dictionary<Guid, IEnumerable<View>> _viewsGestureCounter = new Dictionary<Guid, IEnumerable<View>>();
		private readonly List<TimeDiffItem> _timeDiffItems = new List<TimeDiffItem>();
		private readonly ViewsInUseSet _viewsInUse = new ViewsInUseSet();
		private readonly InteractionQueue _interactions = new InteractionQueue();
		private readonly TapGestureRecognizer _tapGesture = new TapGestureRecognizer();
		private readonly ContextAssignedBehavior _contextAssignedBehavior = new ContextAssignedBehavior();

		private IEnumerable<View> _prevViews = Enumerable.Empty<View>();
		private IEnumerable<View> _nextViews = Enumerable.Empty<View>();
		private IEnumerable<View> _currentBackViews = Enumerable.Empty<View>();
		private IEnumerable<View> _currentInactiveBackViews = Enumerable.Empty<View>();

		private AnimationDirection _currentBackAnimationDirection;
		private INotifyCollectionChanged _currentObservableCollection;

		private int _viewsChildrenCount;
		private int _inCoursePanDelay;
		private bool _isPanEndRequested = true;
		private bool _shouldSkipTouch;
		private bool _isViewsInited;
		private bool _hasRenderer;
		private bool? _isPortraitOrientation;
		private bool? _shouldScrollParent;
		private DateTime _lastPanTime;
		private CancellationTokenSource _slideshowTokenSource;

		public CardsView() : this(new BaseCardFrontViewProcessor(), new BaseCardBackViewProcessor())
		{
		}

		public CardsView(ICardProcessor frontViewProcessor, ICardProcessor backViewProcessor)
		{
			FrontViewProcessor = frontViewProcessor;
			BackViewProcessor = backViewProcessor;

			if (Device.RuntimePlatform != Device.Android)
			{
				var panGesture = new PanGestureRecognizer();
				panGesture.PanUpdated += OnPanUpdated;
				GestureRecognizers.Add(panGesture);
			}

			_tapGesture.Tapped += OnCardTapped;
		}

		private bool ShouldIgnoreSetCurrentView { get; set; }

		private bool ShouldSetIndexAfterPan { get; set; }

		private IEnumerable<View> PrevViews
		{
			get => _prevViews;
			set => _prevViews = value ?? Enumerable.Empty<View>();
		}

		private IEnumerable<View> NextViews
		{
			get => _nextViews;
			set => _nextViews = value ?? Enumerable.Empty<View>();
		}

		private IEnumerable<View> CurrentBackViews
		{
			get => _currentBackViews;
			set => _currentBackViews = value ?? Enumerable.Empty<View>();
		}

		private IEnumerable<View> CurrentInactiveBackViews
		{
			get => _currentInactiveBackViews;
			set => _currentInactiveBackViews = value ?? Enumerable.Empty<View>();
		}

		private View CurrentView { get; set; }

		public double CurrentDiff { get; private set; }

		public int OldIndex { get; private set; } = -1;

		public ICardProcessor FrontViewProcessor { get; }

		public ICardProcessor BackViewProcessor { get; }

		public int SelectedIndex
		{
			get => (int)GetValue(SelectedIndexProperty);
			set => SetValue(SelectedIndexProperty, value);
		}

		public object SelectedItem
		{
			get => GetValue(SelectedItemProperty);
			set => SetValue(SelectedItemProperty, value);
		}

		public IList ItemsSource
		{
			get => GetValue(ItemsSourceProperty) as IList;
			set => SetValue(ItemsSourceProperty, value);
		}

		public DataTemplate ItemTemplate
		{
			get => GetValue(ItemTemplateProperty) as DataTemplate;
			set => SetValue(ItemTemplateProperty, value);
		}

		public int BackViewsDepth
		{
			get => (int)GetValue(BackViewsDepthProperty);
			set => SetValue(BackViewsDepthProperty, value);
		}

		public bool IsRightToLeftFlowDirectionEnabled
		{
			get => (bool)GetValue(IsRightToLeftFlowDirectionEnabledProperty);
			set => SetValue(IsRightToLeftFlowDirectionEnabledProperty, value);
		}

		public int SlideShowDuration
		{
			get => (int)GetValue(SlideShowDurationProperty);
			set => SetValue(SlideShowDurationProperty, value);
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

		public int ItemsCount
		{
			get => (int)GetValue(ItemsCountProperty);
			private set => SetValue(ItemsCountProperty, value);
		}

		public bool IsUserInteractionEnabled
		{
			get => (bool)GetValue(IsUserInteractionEnabledProperty);
			set => SetValue(IsUserInteractionEnabledProperty, value);
		}

		public double MoveDistance
		{
			get
			{
				var dist = (double)GetValue(MoveDistanceProperty);
				return dist > 0
						? dist
						: Width * MoveWidthPercentage;
			}
			set => SetValue(MoveDistanceProperty, value);
		}

		public double MoveWidthPercentage
		{
			get => (double)GetValue(MoveWidthPercentageProperty);
			set => SetValue(MoveWidthPercentageProperty, value);
		}

		public bool IsOnlyForwardDirection
		{
			get => (bool)GetValue(IsOnlyForwardDirectionProperty);
			set => SetValue(IsOnlyForwardDirectionProperty, value);
		}

		public bool IsViewCacheEnabled
		{
			get => (bool)GetValue(IsViewCacheEnabledProperty);
			set => SetValue(IsViewCacheEnabledProperty, value);
		}

		public int UserInteractionDelay
		{
			get => IsUserInteractionInCourse ? _inCoursePanDelay : (int)GetValue(UserInteractionDelayProperty);
			set => SetValue(UserInteractionDelayProperty, value);
		}

		public bool IsUserInteractionInCourse
		{
			get => (bool)GetValue(IsUserInteractionInCourseProperty);
			set => SetValue(IsUserInteractionInCourseProperty, value);
		}

		public bool IsCyclical
		{
			get => (bool)GetValue(IsCyclicalProperty);
			set => SetValue(IsCyclicalProperty, value);
		}

        public bool IsAutoNavigatingAimationEnabled
        {
            get => (bool)GetValue(IsAutoNavigatingAimationEnabledProperty);
            set => SetValue(IsAutoNavigatingAimationEnabledProperty, value);
        }

		public int MaxChildrenCount
		{
			get => (int)GetValue(MaxChildrenCountProperty);
			set => SetValue(MaxChildrenCountProperty, value);
		}

		public int DesiredMaxChildrenCount
		{
			get => (int)GetValue(DesiredMaxChildrenCountProperty);
			set => SetValue(DesiredMaxChildrenCountProperty, value);
		}

		public double SwipeThresholdDistance
		{
			get => (double)GetValue(SwipeThresholdDistanceProperty);
			set => SetValue(SwipeThresholdDistanceProperty, value);
		}

		/// <summary>
		/// Only for Android
		/// </summary>
		/// <value>The move threshold distance.</value>
		public double MoveThresholdDistance
		{
			get => (double)GetValue(MoveThresholdDistanceProperty);
			set => SetValue(MoveThresholdDistanceProperty, value);
		}

		public TimeSpan SwipeThresholdTime
		{
			get => (TimeSpan)GetValue(SwipeThresholdTimeProperty);
			set => SetValue(SwipeThresholdTimeProperty, value);
		}

		public ICommand UserInteractedCommand
		{
			get => GetValue(UserInteractedCommandProperty) as ICommand;
			set => SetValue(UserInteractedCommandProperty, value);
		}

		public ICommand ItemDisappearingCommand
		{
			get => GetValue(ItemDisappearingCommandProperty) as ICommand;
			set => SetValue(ItemDisappearingCommandProperty, value);
		}

		public ICommand ItemAppearingCommand
		{
			get => GetValue(ItemAppearingCommandProperty) as ICommand;
			set => SetValue(ItemAppearingCommandProperty, value);
		}

		public ICommand ItemTappedCommand
		{
			get => GetValue(ItepTappedCommandProperty) as ICommand;
			set => SetValue(ItepTappedCommandProperty, value);
		}

		public void OnPanUpdated(object sender, PanUpdatedEventArgs e)
		=> OnPanUpdated(e);

		public void OnPanUpdated(PanUpdatedEventArgs e, bool? isSwiped = null)
		{
			if (ItemsCount <= 0)
			{
				return;
			}

			switch (e.StatusType)
			{
				case GestureStatus.Started:
					OnTouchStarted();
					return;
				case GestureStatus.Running:
					HandleParentScroll(e);
					OnTouchChanged(e.TotalX);
					return;
				case GestureStatus.Canceled:
				case GestureStatus.Completed:
					OnTouchEnded(isSwiped);
					return;
			}
		}

		protected virtual void OnOrientationChanged()
		{
            lock (_orientationChangedLocker)
            {
                if (CurrentView != null && ItemTemplate != null)
                {
                    var currentViewPair = _viewsPool.FirstOrDefault(p => p.Value.Contains(CurrentView));
                    currentViewPair.Value.Clear();
                    currentViewPair.Value.Add(CurrentView);
                    _viewsPool.Clear();
                    _viewsPool.Add(currentViewPair.Key, currentViewPair.Value);
                }

                SetCurrentView();
                RemoveUnprocessingChildren();
                LayoutChildren(X, Y, Width, Height);
                ForceLayout();
            }
		}

        protected virtual void SetupBackViews()
		{
			SetupNextView();
			SetupPrevView();

			if (IsRightToLeftFlowDirectionEnabled)
			{
				var nextViews = NextViews;
				NextViews = PrevViews;
				PrevViews = nextViews;
			}
		}

		protected virtual void SetupLayout(params View[] views)
		{
			foreach (var view in views.Where(v => v != null))
			{
				SetLayoutBounds(view, new Rectangle(0, 0, 1, 1));
				SetLayoutFlags(view, AbsoluteLayoutFlags.All);
				view.GestureRecognizers.Remove(_tapGesture);
				view.GestureRecognizers.Add(_tapGesture);
			}
		}

		protected virtual async void SetCurrentView()
		{
			if (!_hasRenderer || Parent == null || await TryAutoNavigate())
			{
				return;
			}

			lock (_setCurrentViewLocker)
			{
				if (ItemsSource != null)
				{
					var oldView = CurrentView;
					CurrentView = GetViews(AnimationDirection.Current, FrontViewProcessor, SelectedIndex).FirstOrDefault();
					var newView = CurrentView;

					if (CurrentView == null && SelectedIndex >= 0)
					{
						ShouldIgnoreSetCurrentView = true;
						SelectedIndex = -1;
					}
					else if (SelectedIndex != OldIndex)
					{
						var isNextSelected = SelectedIndex > OldIndex;
						FireItemDisappearing(InteractionType.User, isNextSelected, oldView.GetItem());
						FireItemAppearing(InteractionType.User, isNextSelected, newView.GetItem());
					}

					SetupBackViews();
				}
			}
		}

		protected virtual void SetSelectedItem()
		{
			var index = SelectedIndex;
			if (IsCyclical)
			{
				index = index.ToCyclingIndex(ItemsCount);
			}

			if (!CheckIndexValid(index))
			{
				SelectedItem = null;
				return;
			}

			SelectedItem = ItemsSource[index];
		}

		protected virtual async void AdjustSlideShow(bool isForceStop = false)
		{
			_slideshowTokenSource?.Cancel();
			if (isForceStop)
			{
				return;
			}

			if (SlideShowDuration > 0)
			{
				_slideshowTokenSource = new CancellationTokenSource();
				var token = _slideshowTokenSource.Token;
				while (true)
				{
					await Task.Delay(SlideShowDuration);
					if (token.IsCancellationRequested)
					{
						return;
					}
					if (ItemsCount > 0)
					{
						SelectedIndex = (SelectedIndex.ToCyclingIndex(ItemsCount) + 1).ToCyclingIndex(ItemsCount);
					}
				}
			}
		}

		protected virtual async Task<bool> TryAutoNavigate()
		{
            if (CurrentView == null || !IsAutoNavigatingAimationEnabled)
			{
				return false;
			}

			var context = GetContext(SelectedIndex, AnimationDirection.Current);

			if (CurrentView.BindingContext == context || CurrentView == context)
			{
				return false;
			}

			var animationDirection = GetAutoNavigateAnimationDirection();
			var realDirection = animationDirection;
			if (IsRightToLeftFlowDirectionEnabled)
			{
				realDirection = ((AnimationDirection)Sign(-(int)realDirection));
			}

			var oldView = CurrentView;
			var newView = PrepareView(SelectedIndex, AnimationDirection.Current, Enumerable.Empty<View>());
			CurrentView = newView;

			BackViewProcessor.HandleInitView(Enumerable.Repeat(CurrentView, 1), this, realDirection);

			SetupLayout(CurrentView);

			AddChild(oldView, CurrentView);

			var animationId = Guid.NewGuid();
			StartAutoNavigation(oldView, animationId, animationDirection);
			var autoNavigationTask = Task.WhenAll(
				BackViewProcessor.HandleAutoNavigate(Enumerable.Repeat(oldView, 1), this, realDirection, CurrentInactiveBackViews),
				FrontViewProcessor.HandleAutoNavigate(Enumerable.Repeat(CurrentView, 1), this, realDirection, Enumerable.Empty<View>()));

			if (ItemTemplate != null)
			{
				SetupBackViews();
			}

			await autoNavigationTask;
			EndAutoNavigation(oldView, newView, animationId, animationDirection);

			return true;
		}

		protected virtual void SetupNextView()
		{
			var indeces = new int[BackViewsDepth];
			for (int i = 0; i < indeces.Length; ++i)
			{
				indeces[i] = SelectedIndex + 1 + i;
			}

			NextViews = GetViews(AnimationDirection.Next, BackViewProcessor, indeces);
		}

		protected virtual void SetupPrevView()
		{
			var prevIndex = IsOnlyForwardDirection
				? SelectedIndex + 1
				: SelectedIndex - 1;


			var indeces = new int[BackViewsDepth];
			for (int i = 0; i < indeces.Length; ++i)
			{
				var incValue = i + 1;
				if (!IsOnlyForwardDirection)
				{
					incValue = -incValue;
				}
				indeces[i] = SelectedIndex + incValue;
			}

			PrevViews = GetViews(AnimationDirection.Prev, BackViewProcessor, indeces);
		}

		protected virtual bool CheckIsProtectedView(View view) => view.Behaviors.Any(b => b is ProtectedControlBehavior);

		protected virtual bool CheckIsCacheEnabled(DataTemplate template) => IsViewCacheEnabled;

		protected override void OnSizeAllocated(double width, double height)
		{
			base.OnSizeAllocated(width, height);
			if (!_isViewsInited && width > 0 && height > 0)
			{
				_isViewsInited = true;
				_isPortraitOrientation = height > width;
				FrontViewProcessor.HandleInitView(Enumerable.Repeat(CurrentView, 1), this, AnimationDirection.Current);
				BackViewProcessor.HandleInitView(PrevViews, this, AnimationDirection.Prev);
				BackViewProcessor.HandleInitView(NextViews, this, AnimationDirection.Next);
			}
			if (_isPortraitOrientation.HasValue && _isPortraitOrientation != height > width)
			{
				_isPortraitOrientation = !_isPortraitOrientation;
				OnOrientationChanged();
			}
		}

		protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			base.OnPropertyChanged(propertyName);
			switch (propertyName)
			{
				case "Renderer":
					_hasRenderer = !_hasRenderer;
					if (_hasRenderer)
					{
						SetCurrentView();
						return;
					}
					AdjustSlideShow(true);
					return;
			}
		}

		protected override void OnParentSet()
		{
			base.OnParentSet();
			SetCurrentView();
		}

		private void StartAutoNavigation(View oldView, Guid animationId, AnimationDirection animationDirection)
		{
			if (oldView != null)
			{
				_interactions.Add(animationId, InteractionType.Auto, InteractionState.Removing);
				IsAutoInteractionRunning = true;
				if (IsUserInteractionInCourse)
				{
					_inCoursePanDelay = int.MaxValue;
				}
				lock (_viewsInUseLocker)
				{
					_viewsInUse.Add(oldView);
				}
				FireItemDisappearing(InteractionType.Auto, animationDirection != AnimationDirection.Prev, oldView.GetItem());
			}
		}

		private void EndAutoNavigation(View oldView, View newView, Guid animationId, AnimationDirection animationDirection)
		{
			_inCoursePanDelay = 0;
			if (oldView != null)
			{
				lock (_viewsInUseLocker)
				{
					_viewsInUse.Remove(oldView);
					CleanView(oldView);
				}
			}
			IsAutoInteractionRunning = false;
			var isProcessingNow = !_interactions.CheckLastId(animationId);
			RemoveRedundantChildren(isProcessingNow);
			FireItemAppearing(InteractionType.Auto, animationDirection != AnimationDirection.Prev, newView.GetItem());
			_interactions.Remove(animationId);
		}

		private AnimationDirection GetAutoNavigateAnimationDirection()
		{
			if (!IsCyclical)
			{
				return SelectedIndex < OldIndex
					   ? AnimationDirection.Prev
					   : AnimationDirection.Next;
			}

			var recIndex = SelectedIndex.ToCyclingIndex(ItemsCount);
			var oldRecIndex = OldIndex.ToCyclingIndex(ItemsCount);

			var deltaIndex = recIndex - oldRecIndex;
			if (Abs(deltaIndex) == 1)
			{
				return deltaIndex > 0
					? AnimationDirection.Next
					 : AnimationDirection.Prev;
			}

			return deltaIndex > 0
					? AnimationDirection.Prev
					 : AnimationDirection.Next;
		}

		private void OnTouchStarted()
		{
			_shouldScrollParent = null;
			if (!_isPanEndRequested)
			{
				return;
			}

			var deltaTime = DateTime.Now - _lastPanTime;
			if (!IsUserInteractionEnabled || deltaTime.TotalMilliseconds < UserInteractionDelay || CurrentView == null)
			{
				_shouldSkipTouch = true;
				return;
			}
			_shouldSkipTouch = false;

			if (IsUserInteractionInCourse)
			{
				_inCoursePanDelay = int.MaxValue;
			}

			var gestureId = Guid.NewGuid();
			_interactions.Add(gestureId, InteractionType.User);

			FireUserInteracted(UserInteractionStatus.Started, 0, SelectedIndex);
			IsUserInteractionRunning = true;
			_isPanEndRequested = false;

			SetupBackViews();
			AddRangeViewsInUse(gestureId);

			_timeDiffItems.Add(new TimeDiffItem
			{
				Time = DateTime.Now,
				Diff = 0
			});
		}

		private void OnTouchChanged(double diff)
		{
			if (_shouldSkipTouch || (_shouldScrollParent ?? false))
			{
				return;
			}

			ResetActiveInactiveBackViews(diff);

			CurrentDiff = diff;

			SetupDiffItems(diff);

			FireUserInteracted(UserInteractionStatus.Running, CurrentDiff, SelectedIndex);

			FrontViewProcessor.HandlePanChanged(Enumerable.Repeat(CurrentView, 1), this, diff, _currentBackAnimationDirection, Enumerable.Empty<View>());
			BackViewProcessor.HandlePanChanged(CurrentBackViews, this, diff, _currentBackAnimationDirection, CurrentInactiveBackViews);
		}

		private async void OnTouchEnded(bool? isSwiped)
		{
			if (_isPanEndRequested || _shouldSkipTouch)
			{
				return;
			}

			_lastPanTime = DateTime.Now;
			var interactionItem = _interactions.GetFirstItem(InteractionType.User, InteractionState.Regular);
			interactionItem.State = InteractionState.Removing;
			if(interactionItem.Id == default(Guid))
			{
				return;
			}

			var gestureId = interactionItem.Id;

			_isPanEndRequested = true;
			var absDiff = Abs(CurrentDiff);

			var index = SelectedIndex;
			var diff = CurrentDiff;

			CleanDiffItems();

			bool? isNextSelected = null;

			if (IsEnabled && _currentBackAnimationDirection != AnimationDirection.Null)
			{
				var checkSwipe = CheckSwipe();
				if (checkSwipe.HasValue)
				{
					if (isSwiped.HasValue)
					{
						isNextSelected = isSwiped;
					}

					if (checkSwipe.Value || absDiff > MoveDistance)
					{
						isNextSelected = diff < 0;
					}
				}
			}

			_timeDiffItems.Clear();

			Task endingTask = null;
			if (isNextSelected.HasValue)
			{
				index = GetNewIndexFromDiff();
				if (index < 0)
				{
					return;
				}

				SwapViews(isNextSelected.GetValueOrDefault());
				ShouldIgnoreSetCurrentView = true;

				SelectedIndex = index;

				endingTask = Task.WhenAll(
					FrontViewProcessor.HandlePanApply(Enumerable.Repeat(CurrentView, 1), this, _currentBackAnimationDirection, Enumerable.Empty<View>()),
					BackViewProcessor.HandlePanApply(CurrentBackViews, this, _currentBackAnimationDirection, CurrentInactiveBackViews)
				);
			}
			else
			{
				endingTask = Task.WhenAll(
					FrontViewProcessor.HandlePanReset(Enumerable.Repeat(CurrentView, 1), this, _currentBackAnimationDirection, Enumerable.Empty<View>()),
					BackViewProcessor.HandlePanReset(CurrentBackViews, this, _currentBackAnimationDirection, CurrentInactiveBackViews)
				);
			}

			var oldView = CurrentBackViews.FirstOrDefault();
			var newView = CurrentView;

			FireUserInteracted(UserInteractionStatus.Ending, diff, index, isNextSelected, oldView);
			CurrentDiff = 0;

			await endingTask;

			FireUserInteracted(UserInteractionStatus.Ended, diff, index, isNextSelected, newView);

			var isProcessingNow = !_interactions.CheckLastId(gestureId);
			RemoveRangeViewsInUse(gestureId, isProcessingNow);

			if (!isProcessingNow)
			{
				IsUserInteractionRunning = false;
				if (ShouldSetIndexAfterPan)
				{
					ShouldSetIndexAfterPan = false;
					SetNewIndex();
				}
			}

			RemoveRedundantChildren(isProcessingNow);

			_inCoursePanDelay = 0;

			_interactions.Remove(gestureId);
		}

		private bool? CheckSwipe()
		{
			if (_timeDiffItems.Count < 2)
			{
				return false;
			}

			var lastItem = _timeDiffItems.Last();
			var firstItem = _timeDiffItems.First();

			var distDiff = lastItem.Diff - firstItem.Diff;

			if (Sign(distDiff) != Sign(lastItem.Diff))
			{
				return null;
			}

			var absDistDiff = Abs(distDiff);
			var timeDiff = lastItem.Time - firstItem.Time;

			var acceptValue = SwipeThresholdDistance * timeDiff.TotalMilliseconds / SwipeThresholdTime.TotalMilliseconds;

			return absDistDiff >= acceptValue;
		}

		private void SetupDiffItems(double diff)
		{
			var timeNow = DateTime.Now;

			if (_timeDiffItems.Count >= 25)
			{
				CleanDiffItems();
			}

			_timeDiffItems.Add(new TimeDiffItem
			{
				Time = timeNow,
				Diff = diff
			});
		}

		private void CleanDiffItems()
		{
			var time = _timeDiffItems.LastOrDefault().Time;

			for (var i = _timeDiffItems.Count - 1; i >= 0; --i)
			{
				if (time - _timeDiffItems[i].Time > SwipeThresholdTime)
				{
					_timeDiffItems.RemoveAt(i);
				}
			}
		}

		private int GetNewIndexFromDiff()
		{
			var indexDelta = -Sign(CurrentDiff) * (IsRightToLeftFlowDirectionEnabled ? -1 : 1);
			if (IsOnlyForwardDirection)
			{
				indexDelta = Abs(indexDelta);
			}
			var newIndex = SelectedIndex + indexDelta;

			if (!CheckIndexValid(newIndex))
			{
				if (!IsCyclical)
				{
					return -1;
				}

				newIndex = newIndex.ToCyclingIndex(ItemsCount);
			}

			return newIndex;
		}

		private void ResetActiveInactiveBackViews(double diff)
		{
			var activeViews = NextViews;
			var inactiveViews = PrevViews;
			var animationDirection = AnimationDirection.Next;

			if (diff > 0)
			{
				activeViews = PrevViews;
				inactiveViews = NextViews;
				animationDirection = AnimationDirection.Prev;
			}

			CurrentBackViews = activeViews;
			_currentBackAnimationDirection = CurrentBackViews.Any()
					? animationDirection
					: AnimationDirection.Null;


			CurrentInactiveBackViews = !inactiveViews.SequenceEqual(activeViews)
				? inactiveViews
				: null;
		}

		private void SwapViews(bool isNext)
		{
			var view = CurrentView;
			CurrentView = CurrentBackViews.FirstOrDefault();
			CurrentBackViews = CurrentBackViews.Except(Enumerable.Repeat(CurrentView, 1)).Union(Enumerable.Repeat(view, 1));

			if (isNext)
			{
				NextViews = PrevViews;
				PrevViews = CurrentBackViews;
				return;
			}
			PrevViews = NextViews;
			NextViews = CurrentBackViews;
		}

		private IEnumerable<View> GetViews(AnimationDirection animationDirection, ICardProcessor processor, params int[] indeces)
		{
			var views = new View[indeces.Length];

			for (int i = 0; i < indeces.Length; ++i)
			{
				views[i] = PrepareView(indeces[i], animationDirection, views);
			}

			if (views.All(x => x == null))
			{
				return Enumerable.Empty<View>();
			}

			processor.HandleInitView(views, this, animationDirection);

			SetupLayout(views);

			if (animationDirection == AnimationDirection.Current)
			{
				AddBackChild(views);
			}
			else
			{
				AddChild(CurrentView, views);
			}
			return views;
		}

		private View PrepareView(int index, AnimationDirection animationDirection, IEnumerable<View> bookedViews)
		{
			var context = GetContext(index, animationDirection);

			if (context == null)
			{
				return null;
			}

			var template = ItemTemplate;
			if (template is DataTemplateSelector selector)
			{
				template = selector.SelectTemplate(context, this);
			}

			var view = template != null
				? CreateRetrieveView(context, template, bookedViews)
				: context as View;

			if (view != null && view != context)
			{
				view.BindingContext = context;
				view.Behaviors.Remove(_contextAssignedBehavior);
				view.Behaviors.Add(_contextAssignedBehavior);
			}

			return view;
		}

		private View CreateRetrieveView(object context, DataTemplate template, IEnumerable<View> bookedViews)
		{
			if (!CheckIsCacheEnabled(template))
			{
				return template.CreateView();
			}

			List<View> viewsList;
			if (!_viewsPool.TryGetValue(template, out viewsList))
			{
				viewsList = new List<View>
				{
					template.CreateView()
				};
				_viewsPool.Add(template, viewsList);
			}

			var notUsingViews = viewsList.Where(v => !_viewsInUse.Contains(v));
			var view = notUsingViews.FirstOrDefault(v => v.BindingContext == context)
									?? notUsingViews.FirstOrDefault(v => v.BindingContext == null)
									?? notUsingViews.FirstOrDefault(v => !CheckIsProcessingView(v) && !bookedViews.Contains(v));

			if (view == null)
			{
				view = template.CreateView();
				viewsList.Add(view);
			}

			return view;
		}

		private object GetContext(int index, AnimationDirection animationDirection)
		{
			if (ItemsCount <= 0)
			{
				return null;
			}

			if (!CheckIndexValid(index))
			{
				if (!IsCyclical || (animationDirection != AnimationDirection.Current && ItemsCount <= 1))
				{
					return null;
				}

				index = index.ToCyclingIndex(ItemsCount);
			}

			if (index < 0 || ItemsSource == null)
			{
				return null;
			}

			return ItemsSource[index];
		}

		private void SendChildrenToBackIfNeeded(View view, View topView)
		{
			lock (_childLocker)
			{
				if (view == null || topView == null)
				{
					return;
				}

				var currentIndex = Children.IndexOf(topView);
				var backIndex = Children.IndexOf(view);

				if (currentIndex < backIndex)
				{
					LowerChild(view);
				}
			}
		}

		private void CleanView(View view)
		{
			if (view?.Behaviors.Contains(_contextAssignedBehavior) ?? false)
			{
				view.Behaviors.Remove(_contextAssignedBehavior);
				view.BindingContext = null;
			}
		}

		private void SetItemsCount()
		{
			if (_currentObservableCollection != null)
			{
				_currentObservableCollection.CollectionChanged -= OnObservableCollectionChanged;
			}

			if (ItemsSource is INotifyCollectionChanged observableCollection)
			{
				_currentObservableCollection = observableCollection;
				observableCollection.CollectionChanged += OnObservableCollectionChanged;
			}

			OnObservableCollectionChanged(ItemsSource, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		private void OnObservableCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			ItemsCount = ItemsSource?.Count ?? -1;

			ShouldSetIndexAfterPan = IsUserInteractionRunning;
			if (!IsUserInteractionRunning)
			{
				SetNewIndex();
			}
		}

		private void SetNewIndex()
		{
			if (ItemsCount <= 0)
			{
				SelectedIndex = -1;
				return;
			}

			var index = -1;
			var isCurrentContextPresent = false;

			if (CurrentView != null)
			{
				for (var i = 0; i < ItemsCount; ++i)
				{
					if (ItemsSource[i] == CurrentView.BindingContext)
					{
						index = i;
						isCurrentContextPresent = true;
						break;
					}
				}

				if (index < 0)
				{
					index = SelectedIndex - 1;
				}
			}

			if (index < 0)
			{
				index = 0;
			}

			if (SelectedIndex == index)
			{
				if (!isCurrentContextPresent)
				{
					OldIndex = index;
					SetCurrentView();
				}
				return;
			}

			SelectedIndex = index;
		}

		private void AddBackChild(params View[] views)
		{
			lock (_childLocker)
			{
				foreach (var view in views)
				{
					if (view == null || Children.Contains(view))
					{
						continue;
					}

					++_viewsChildrenCount;
					Children.Insert(0, view);
				}
			}
		}

		private void AddChild(View topView, params View[] views)
		{
			lock (_childLocker)
			{
				foreach (var view in views)
				{
					if (view == null)
					{
						continue;
					}

					if (Children.Contains(view))
					{
						SendChildrenToBackIfNeeded(view, topView);
						return;
					}

					++_viewsChildrenCount;
					var index = Children.IndexOf(topView);
					Children.Insert(index, view);
				}
			}
		}

		private void RemoveRedundantChildren(bool isProcessingNow)
		{
			var maxChildrenCount = isProcessingNow
				? MaxChildrenCount
				: DesiredMaxChildrenCount;

			if (_viewsChildrenCount <= maxChildrenCount)
			{
				return;
			}

			lock (_childLocker)
			{
				var views = Children.Where(c => !CheckIsProtectedView(c) && !CheckIsProcessingView(c) && !_viewsInUse.Contains(c)).Take(_viewsChildrenCount - DesiredMaxChildrenCount).ToArray();
				RemoveChildren(views);
			}
		}

		private void RemoveUnprocessingChildren()
		{
			lock (_childLocker)
			{
				var views = Children.Where(c => !CheckIsProtectedView(c) && !CheckIsProcessingView(c)).ToArray();
				RemoveChildren(views);
			}
		}

		private void RemoveChildren(View[] views)
		{
			_viewsChildrenCount -= views.Length;
			foreach (var view in views)
			{
				Children.Remove(view);
				CleanView(view);
			}
		}

		private bool CheckIsProcessingView(View view)
		=> view == CurrentView || NextViews.Contains(view) || PrevViews.Contains(view);

		private bool CheckIndexValid(int index)
		=> index >= 0 && index < ItemsCount;

		private void AddRangeViewsInUse(Guid gestureId)
		{
			lock (_viewsInUseLocker)
			{
				var views = NextViews.Union(PrevViews).Union(Enumerable.Repeat(CurrentView, 1));

				_viewsGestureCounter[gestureId] = views;

				foreach (var view in views.Where(v => v != null))
				{
					_viewsInUse.Add(view);
				}
			}
		}

		private void RemoveRangeViewsInUse(Guid gestureId, bool isProcessingNow)
		{
			lock (_viewsInUseLocker)
			{
				if(!_viewsGestureCounter.ContainsKey(gestureId))
				{
					return;
				}

				foreach (var view in _viewsGestureCounter[gestureId])
				{
					_viewsInUse.Remove(view);
					if (isProcessingNow && !_viewsInUse.Contains(view))
					{
						CleanView(view);
					}
				}
				_viewsGestureCounter.Remove(gestureId);
			}
		}

		private void HandleParentScroll(PanUpdatedEventArgs e)
		{
			if (Device.RuntimePlatform == Device.iOS)
			{
				var y = e.TotalY;
				var absY = Abs(y);
				var absX = Abs(e.TotalX);

				var isFirst = false;
				if (!_shouldScrollParent.HasValue)
				{
					_shouldScrollParent = absY > absX;
					isFirst = true;
				}

				if (_shouldScrollParent.Value)
				{
					var parent = Parent;
					while (parent != null)
					{
						if (parent is IOrdinateHandlerParentView scrollableView)
						{
							scrollableView.HandleOrdinateValue(y, isFirst);
							break;
						}
						parent = parent.Parent;
					}
				}
			}
		}

		private void OnCardTapped(object sender, System.EventArgs args)
		{
			var tapArgs = new CardTappedEventArgs((sender as View).GetItem());
			ItemTappedCommand?.Execute(tapArgs);
			ItemTapped?.Invoke(this, tapArgs);
		}

		private void FireUserInteracted(UserInteractionStatus status, double diff, int index, bool? isNextSelected = null, View view = null)
		{
			var args = new UserInteractedEventArgs(index, diff, status);
			UserInteractedCommand?.Execute(args);
			UserInteracted?.Invoke(this, args);

			if (isNextSelected.HasValue)
			{
				var item = view.GetItem();
				switch (status)
				{
					case UserInteractionStatus.Ending:
						FireItemDisappearing(InteractionType.User, isNextSelected.GetValueOrDefault(), item);
						return;

					case UserInteractionStatus.Ended:
						FireItemAppearing(InteractionType.User, isNextSelected.GetValueOrDefault(), item);
						return;
				}
			}
		}

		private void FireItemDisappearing(InteractionType type, bool isNextSelected, object item)
		{
			var args = new ItemDisappearingEventArgs(type, isNextSelected, item);
			ItemDisappearingCommand?.Execute(args);
			ItemDisappearing?.Invoke(this, args);
		}

		private void FireItemAppearing(InteractionType type, bool isNextSelected, object item)
		{
			var args = new ItemAppearingEventArgs(type, isNextSelected, item);
			ItemAppearingCommand?.Execute(args);
			ItemAppearing?.Invoke(this, args);
		}
	}
}