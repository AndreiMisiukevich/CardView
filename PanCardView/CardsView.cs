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

namespace PanCardView
{
	public delegate void CardsViewPanStartEndHandler(CardsView view, int index, double diff);
	public delegate void CardsViewPanChangedHandler(CardsView view, double diff);
	public delegate void CardsViewPositionChangedHandler(CardsView view, bool isNextSelected);

	public class CardsView : AbsoluteLayout
	{
		public event CardsViewPanStartEndHandler PanStarted;
		public event CardsViewPanStartEndHandler PanEnding;
		public event CardsViewPanStartEndHandler PanEnded;
		public event CardsViewPanChangedHandler PanChanged;
		public event CardsViewPositionChangedHandler PositionChanging;
		public event CardsViewPositionChangedHandler PositionChanged;
		public event CardsViewPositionChangedHandler AutoNavigationStarted;
		public event CardsViewPositionChangedHandler AutoNavigationEnded;

		public static readonly BindableProperty CurrentIndexProperty = BindableProperty.Create(nameof(CurrentIndex), typeof(int), typeof(CardsView), -1, BindingMode.TwoWay, propertyChanged: (bindable, oldValue, newValue) =>
		{
			var view = bindable.AsCardsView();
			view.OldIndex = (int)oldValue;
			if (view.ShouldIgnoreSetCurrentView)
			{
				view.ShouldIgnoreSetCurrentView = false;
				return;
			}
			view.SetCurrentView();
		});

		public static readonly BindableProperty ItemsProperty = BindableProperty.Create(nameof(Items), typeof(IList), typeof(CardsView), null, propertyChanged: (bindable, oldValue, newValue) =>
		{
			bindable.AsCardsView().SetCurrentView();
			bindable.AsCardsView().SetItemsCount();
		});

		public static readonly BindableProperty ItemTemplateProperty = BindableProperty.Create(nameof(ItemTemplate), typeof(DataTemplate), typeof(CardsView), propertyChanged: (bindable, oldValue, newValue) =>
		{
			bindable.AsCardsView().SetCurrentView();
		});

		public static readonly BindableProperty CurrentContextProperty = BindableProperty.Create(nameof(CurrentContext), typeof(object), typeof(CardsView), null, BindingMode.OneWay, propertyChanged: (bindable, oldValue, newValue) =>
		{
			bindable.AsCardsView().SetCurrentView(true);
		});

		public static readonly BindableProperty BackViewsDepthProperty = BindableProperty.Create(nameof(BackViewsDepth), typeof(int), typeof(CardsView), 1, propertyChanged: (bindable, oldValue, newValue) =>
		{
			bindable.AsCardsView().SetCurrentView();
		});

		public static BindableProperty ItemsCountProperty = BindableProperty.Create(nameof(ItemsCount), typeof(int), typeof(CardsView), -1);

		public static readonly BindableProperty NextContextProperty = BindableProperty.Create(nameof(NextContext), typeof(object), typeof(CardsView), null, BindingMode.OneWay);

		public static readonly BindableProperty PrevContextProperty = BindableProperty.Create(nameof(PrevContext), typeof(object), typeof(CardsView), null, BindingMode.OneWay);

		public static readonly BindableProperty IsPanEnabledProperty = BindableProperty.Create(nameof(IsPanEnabled), typeof(bool), typeof(CardsView), true);

		public static readonly BindableProperty MoveDistanceProperty = BindableProperty.Create(nameof(MoveDistance), typeof(double), typeof(CardsView), -1.0);

		public static readonly BindableProperty MoveWidthPercentageProperty = BindableProperty.Create(nameof(MoveWidthPercentage), typeof(double), typeof(CardsView), .325);

		public static readonly BindableProperty IsOnlyForwardDirectionProperty = BindableProperty.Create(nameof(IsOnlyForwardDirection), typeof(bool), typeof(CardsView), false);

		public static readonly BindableProperty IsViewCacheEnabledProperty = BindableProperty.Create(nameof(IsViewCacheEnabled), typeof(bool), typeof(CardsView), true);

		public static readonly BindableProperty PanDelayProperty = BindableProperty.Create(nameof(PanDelay), typeof(int), typeof(CardsView), 200);

		public static readonly BindableProperty IsPanInCourseProperty = BindableProperty.Create(nameof(IsPanInCourse), typeof(bool), typeof(CardsView), true);

		public static readonly BindableProperty IsCyclicalProperty = BindableProperty.Create(nameof(IsCyclical), typeof(bool), typeof(CardsView), false);

		public static readonly BindableProperty IsAutoNavigatingProperty = BindableProperty.Create(nameof(IsAutoNavigating), typeof(bool), typeof(CardsView), false, BindingMode.OneWayToSource);

		public static readonly BindableProperty IsPanRunningProperty = BindableProperty.Create(nameof(IsPanRunning), typeof(bool), typeof(CardsView), false, BindingMode.OneWayToSource);

		public static readonly BindableProperty MaxChildrenCountProperty = BindableProperty.Create(nameof(MaxChildrenCount), typeof(int), typeof(CardsView), 12);

		public static readonly BindableProperty DesiredMaxChildrenCountProperty = BindableProperty.Create(nameof(DesiredMaxChildrenCount), typeof(int), typeof(CardsView), 6);

		public static readonly BindableProperty SwipeThresholdDistanceProperty = BindableProperty.Create(nameof(SwipeThresholdDistance), typeof(double), typeof(CardsView), 17.0);

		public static readonly BindableProperty SwipeThresholdTimeProperty = BindableProperty.Create(nameof(SwipeThresholdTime), typeof(TimeSpan), typeof(CardsView), TimeSpan.FromMilliseconds(Device.RuntimePlatform == Device.Android ? 100 : 60));

		public static readonly BindableProperty PanStartedCommandProperty = BindableProperty.Create(nameof(PanStartedCommand), typeof(ICommand), typeof(CardsView), null);

		public static readonly BindableProperty PanEndingCommandProperty = BindableProperty.Create(nameof(PanEndingCommand), typeof(ICommand), typeof(CardsView), null);

		public static readonly BindableProperty PanEndedCommandProperty = BindableProperty.Create(nameof(PanEndedCommand), typeof(ICommand), typeof(CardsView), null);

		public static readonly BindableProperty PanChangedCommandProperty = BindableProperty.Create(nameof(PanChangedCommand), typeof(ICommand), typeof(CardsView), null);

		public static readonly BindableProperty PositionChangingCommandProperty = BindableProperty.Create(nameof(PositionChangingCommand), typeof(ICommand), typeof(CardsView), null);

		public static readonly BindableProperty PositionChangedCommandProperty = BindableProperty.Create(nameof(PositionChangedCommand), typeof(ICommand), typeof(CardsView), null);

		public static readonly BindableProperty AutoNavigationStartedCommandProperty = BindableProperty.Create(nameof(AutoNavigationStartedCommand), typeof(ICommand), typeof(CardsView), null);

		public static readonly BindableProperty AutoNavigationEndedCommandProperty = BindableProperty.Create(nameof(AutoNavigationEndedCommand), typeof(ICommand), typeof(CardsView), null);


		private readonly object _childLocker = new object();
		private readonly object _viewsInUseLocker = new object();

		private readonly Dictionary<object, List<View>> _viewsPool = new Dictionary<object, List<View>>();
		private readonly Dictionary<Guid, IEnumerable<View>> _viewsGestureCounter = new Dictionary<Guid, IEnumerable<View>>();
		private readonly List<TimeDiffItem> _timeDiffItems = new List<TimeDiffItem>();
		private readonly ViewsInUseSet _viewsInUse = new ViewsInUseSet();

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
		private bool? _isPortraitOrientation;
		private bool? _shouldScrollParent;
		private Guid _gestureId;
		private DateTime _lastPanTime;

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
		}

		public double CurrentDiff { get; private set; }

		public int OldIndex { get; private set; } = -1;

		public ICardProcessor FrontViewProcessor { get; }

		public ICardProcessor BackViewProcessor { get; }

		private bool ShouldIgnoreSetCurrentView { get; set; }

		private bool ShouldSetIndexAfterPan { get; set; }

		private bool IsContextMode => CurrentContext != null;

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

		public int CurrentIndex
		{
			get => (int)GetValue(CurrentIndexProperty);
			set => SetValue(CurrentIndexProperty, value);
		}

		public IList Items
		{
			get => GetValue(ItemsProperty) as IList;
			set => SetValue(ItemsProperty, value);
		}

		public DataTemplate ItemTemplate
		{
			get => GetValue(ItemTemplateProperty) as DataTemplate;
			set => SetValue(ItemTemplateProperty, value);
		}

		public int ItemsCount
		{
			get => (int)GetValue(ItemsCountProperty);
			private set => SetValue(ItemsCountProperty, value);
		}

		public object CurrentContext
		{
			get => GetValue(CurrentContextProperty);
			set => SetValue(CurrentContextProperty, value);
		}

		public object NextContext
		{
			get => GetValue(NextContextProperty);
			set => SetValue(NextContextProperty, value);
		}

		public object PrevContext
		{
			get => GetValue(PrevContextProperty);
			set => SetValue(PrevContextProperty, value);
		}

		public bool IsPanEnabled
		{
			get => (bool)GetValue(IsPanEnabledProperty);
			set => SetValue(IsPanEnabledProperty, value);
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

		public int PanDelay
		{
			get => IsPanInCourse ? _inCoursePanDelay : (int)GetValue(PanDelayProperty);
			set => SetValue(PanDelayProperty, value);
		}

		public bool IsPanInCourse
		{
			get => (bool)GetValue(IsPanInCourseProperty);
			set => SetValue(IsPanInCourseProperty, value);
		}

		public bool IsCyclical
		{
			get => (bool)GetValue(IsCyclicalProperty);
			set => SetValue(IsCyclicalProperty, value);
		}

		public bool IsAutoNavigating
		{
			get => (bool)GetValue(IsAutoNavigatingProperty);
			set => SetValue(IsAutoNavigatingProperty, value);
		}

		public bool IsPanRunning
		{
			get => (bool)GetValue(IsPanRunningProperty);
			set => SetValue(IsPanRunningProperty, value);
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

		public int BackViewsDepth
		{
			get => (int)GetValue(BackViewsDepthProperty);
			set => SetValue(BackViewsDepthProperty, value);
		}

		public double SwipeThresholdDistance
		{
			get => (double)GetValue(SwipeThresholdDistanceProperty);
			set => SetValue(SwipeThresholdDistanceProperty, value);
		}

		public TimeSpan SwipeThresholdTime
		{
			get => (TimeSpan)GetValue(SwipeThresholdTimeProperty);
			set => SetValue(SwipeThresholdTimeProperty, value);
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

		public ICommand PanStartedCommand
		{
			get => GetValue(PanStartedCommandProperty) as ICommand;
			set => SetValue(PanStartedCommandProperty, value);
		}

		public ICommand PanEndingCommand
		{
			get => GetValue(PanEndingCommandProperty) as ICommand;
			set => SetValue(PanEndingCommandProperty, value);
		}

		public ICommand PanEndedCommand
		{
			get => GetValue(PanEndedCommandProperty) as ICommand;
			set => SetValue(PanEndedCommandProperty, value);
		}

		public ICommand PanChangedCommand
		{
			get => GetValue(PanChangedCommandProperty) as ICommand;
			set => SetValue(PanChangedCommandProperty, value);
		}

		public ICommand PositionChangingCommand
		{
			get => GetValue(PositionChangingCommandProperty) as ICommand;
			set => SetValue(PositionChangingCommandProperty, value);
		}

		public ICommand PositionChangedCommand
		{
			get => GetValue(PositionChangedCommandProperty) as ICommand;
			set => SetValue(PositionChangedCommandProperty, value);
		}

		public ICommand AutoNavigationStartedCommand
		{
			get => GetValue(AutoNavigationStartedCommandProperty) as ICommand;
			set => SetValue(AutoNavigationStartedCommandProperty, value);
		}

		public ICommand AutoNavigationEndedCommand
		{
			get => GetValue(AutoNavigationEndedCommandProperty) as ICommand;
			set => SetValue(AutoNavigationEndedCommandProperty, value);
		}

		public void OnPanUpdated(object sender, PanUpdatedEventArgs e)
		=> OnPanUpdated(e);

		public void OnPanUpdated(PanUpdatedEventArgs e, bool? isSwiped = null)
		{
			if (ItemsCount <= 0 && !IsContextMode)
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
			if (CurrentView != null)
			{
				var currentViewPair = _viewsPool.FirstOrDefault(p => p.Value.Contains(CurrentView));
				currentViewPair.Value.Clear();
				currentViewPair.Value.Add(CurrentView);
				_viewsPool.Clear();
				_viewsPool.Add(currentViewPair.Key, currentViewPair.Value);
			}

			SetCurrentView(CurrentContext != null);
			RemoveUnprocessingChildren();

			ForceLayout();
		}

		protected virtual void SetupBackViews()
		{
			SetupNextView();
			SetupPrevView();
		}

		protected virtual void SetupLayout(params View[] views)
		{
			foreach (var view in views.Where(v => v != null))
			{
				SetLayoutBounds(view, new Rectangle(0, 0, 1, 1));
				SetLayoutFlags(view, AbsoluteLayoutFlags.All);
			}
		}

		protected virtual async void SetCurrentView(bool canResetContext = false)
		{
			if (await TryAutoNavigate())
			{
				return;
			}

			if (TryResetContext(canResetContext, CurrentView, CurrentContext))
			{
				return;
			}

			if (Items != null || IsContextMode)
			{
				CurrentView = GetViews(AnimationDirection.Current, FrontViewProcessor, CurrentIndex).FirstOrDefault();
				if (CurrentView == null && CurrentIndex >= 0)
				{
					ShouldIgnoreSetCurrentView = true;
					CurrentIndex = -1;
				}
				else if (CurrentIndex != OldIndex)
				{
					var isNextSelected = CurrentIndex > OldIndex;
					FirePositionChanging(isNextSelected);
					FirePositionChanged(isNextSelected);
				}
			}

			SetupBackViews();
		}

		protected virtual async Task<bool> TryAutoNavigate()
		{
			if (CurrentView == null)
			{
				return false;
			}

			var context = GetContext(CurrentIndex, AnimationDirection.Current);

			if (CurrentView.BindingContext == context)
			{
				return false;
			}

			var animationDirection = GetAutoNavigateAnimationDirection();

			var oldView = CurrentView;
			var view = PrepareView(CurrentIndex, AnimationDirection.Current, Enumerable.Empty<View>());
			CurrentView = view;

			BackViewProcessor.HandleInitView(Enumerable.Repeat(CurrentView, 1), this, animationDirection);

			SetupLayout(CurrentView);

			AddChild(oldView, CurrentView);


			var animationId = Guid.NewGuid();
			StartAutoNavigation(oldView, animationId, animationDirection);
			var autoNavigationTask = Task.WhenAll(
				BackViewProcessor.HandleAutoNavigate(Enumerable.Repeat(oldView, 1), this, animationDirection, CurrentInactiveBackViews),
				FrontViewProcessor.HandleAutoNavigate(Enumerable.Repeat(CurrentView, 1), this, animationDirection, Enumerable.Empty<View>()));

            if (ItemTemplate != null)
            {
                SetupBackViews();
            }
            
			await autoNavigationTask;
			EndAutoNavigation(oldView, animationId, animationDirection);

			return true;
		}

		protected virtual void SetupNextView(bool canResetContext = false)
		{
			if (TryResetContext(canResetContext, NextViews.FirstOrDefault(), NextContext))
			{
				return;
			}

			var indeces = new int[BackViewsDepth];
			for (int i = 0; i < indeces.Length; ++i)
			{
				indeces[i] = CurrentIndex + 1 + i;
			}

			NextViews = GetViews(AnimationDirection.Next, BackViewProcessor, indeces);
		}

		protected virtual void SetupPrevView(bool canResetContext = false)
		{
			if (TryResetContext(canResetContext, PrevViews.FirstOrDefault(), PrevContext))
			{
				return;
			}

			var prevIndex = IsOnlyForwardDirection
				? CurrentIndex + 1
				: CurrentIndex - 1;


			var indeces = new int[BackViewsDepth];
			for (int i = 0; i < indeces.Length; ++i)
			{
				var incValue = i + 1;
				if (!IsOnlyForwardDirection)
				{
					incValue = -incValue;
				}
				indeces[i] = CurrentIndex + incValue;
			}

			PrevViews = GetViews(AnimationDirection.Prev, BackViewProcessor, indeces);
		}

		protected virtual bool TryResetContext(bool canResetContext, View view, object context)
		{
			if (canResetContext && view != null)
			{
				view.BindingContext = context;
				return true;
			}
			return false;
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
			if(_isPortraitOrientation.HasValue && _isPortraitOrientation != height > width)
			{
				_isPortraitOrientation = !_isPortraitOrientation;
				OnOrientationChanged();
			}
		}

		private void StartAutoNavigation(View view, Guid animationId, AnimationDirection animationDirection)
		{
			if (view != null)
			{
				_gestureId = animationId;
				IsAutoNavigating = true;
				if (IsPanInCourse)
				{
					_inCoursePanDelay = int.MaxValue;
				}
				lock (_viewsInUseLocker)
				{
					_viewsInUse.Add(view);
				}
				FireAutoNavigationStarted(animationDirection != AnimationDirection.Prev);
			}
		}

		private void EndAutoNavigation(View view, Guid animationId, AnimationDirection animationDirection)
		{
			_inCoursePanDelay = 0;
			if (view != null)
			{
				lock (_viewsInUseLocker)
				{
					_viewsInUse.Remove(view);
					CleanView(view);
				}
			}
			IsAutoNavigating = false;
			var isProcessingNow = _gestureId != animationId;
			RemoveRedundantChildren(isProcessingNow);
			FireAutoNavigationEnded(animationDirection != AnimationDirection.Prev);
		}

		private AnimationDirection GetAutoNavigateAnimationDirection()
		{
			if (IsContextMode)
			{
				return CurrentContext == PrevViews.FirstOrDefault()?.BindingContext
					   ? AnimationDirection.Prev
					   : AnimationDirection.Next;
			}

			if (!IsCyclical)
			{
				return CurrentIndex < Items.IndexOf(CurrentView.BindingContext)
					   ? AnimationDirection.Prev
					   : AnimationDirection.Next;
			}

			var recIndex = CurrentIndex.ToCyclingIndex(ItemsCount);
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
			if (!IsPanEnabled || deltaTime.TotalMilliseconds < PanDelay || CurrentView == null)
			{
				_shouldSkipTouch = true;
				return;
			}
			_shouldSkipTouch = false;

			if (IsPanInCourse)
			{
				_inCoursePanDelay = int.MaxValue;
			}

			_gestureId = Guid.NewGuid();
			FirePanStarted();
			IsPanRunning = true;
			_isPanEndRequested = false;

			SetupBackViews();
			AddRangeViewsInUse();

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

			FirePanChanged();

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
			var gestureId = _gestureId;

			_isPanEndRequested = true;
			var absDiff = Abs(CurrentDiff);

			var index = CurrentIndex;
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

			if (isNextSelected.HasValue)
			{
				index = GetNewIndexFromDiff();
				if (index < 0)
				{
					return;
				}

				SwapViews(isNextSelected.GetValueOrDefault());
				ShouldIgnoreSetCurrentView = true;

				CurrentIndex = index;

				FirePanEnding(isNextSelected, index, diff);

				await Task.WhenAll(
					FrontViewProcessor.HandlePanApply(Enumerable.Repeat(CurrentView, 1), this, _currentBackAnimationDirection, Enumerable.Empty<View>()),
					BackViewProcessor.HandlePanApply(CurrentBackViews, this, _currentBackAnimationDirection, CurrentInactiveBackViews)
				);
			}
			else
			{
				FirePanEnding(isNextSelected, index, diff);
				await Task.WhenAll(
					FrontViewProcessor.HandlePanReset(Enumerable.Repeat(CurrentView, 1), this, _currentBackAnimationDirection, Enumerable.Empty<View>()),
					BackViewProcessor.HandlePanReset(CurrentBackViews, this, _currentBackAnimationDirection, CurrentInactiveBackViews)
				);
			}

			FirePanEnded(isNextSelected, index, diff);

			RemoveRangeViewsInUse(gestureId);
			var isProcessingNow = gestureId != _gestureId;
			if (!isProcessingNow)
			{
				IsPanRunning = false;
				if (ShouldSetIndexAfterPan)
				{
					ShouldSetIndexAfterPan = false;
					SetNewIndex();
				}
				if (!IsContextMode)
				{
					SetupBackViews();
				}
			}

			RemoveRedundantChildren(isProcessingNow);

			_inCoursePanDelay = 0;
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
			if (IsContextMode)
			{
				return 0;
			}

			var indexDelta = -Sign(CurrentDiff);
			if (IsOnlyForwardDirection)
			{
				indexDelta = Abs(indexDelta);
			}
			var newIndex = CurrentIndex + indexDelta;

			if (newIndex < 0 || newIndex >= ItemsCount)
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

			if (view != null)
			{
				view.BindingContext = context;
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
			if (IsContextMode)
			{
				switch (animationDirection)
				{
					case AnimationDirection.Current:
						return CurrentContext;
					case AnimationDirection.Next:
						return NextContext;
					case AnimationDirection.Prev:
						return PrevContext;
				}
			}

			if (ItemsCount <= 0)
			{
				return null;
			}

			if (index < 0 || index >= ItemsCount)
			{
				if (!IsCyclical || (animationDirection != AnimationDirection.Current && ItemsCount <= 1))
				{
					return null;
				}

				index = index.ToCyclingIndex(ItemsCount);
			}

			if (index < 0 || Items == null)
			{
				return null;
			}

			return Items[index];
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
			if (view != null)
			{
				view.BindingContext = null;
			}
		}

		private void SetItemsCount()
		{
			if (_currentObservableCollection != null)
			{
				_currentObservableCollection.CollectionChanged -= OnObservableCollectionChanged;
			}

			if (Items is INotifyCollectionChanged observableCollection)
			{
				_currentObservableCollection = observableCollection;
				observableCollection.CollectionChanged += OnObservableCollectionChanged;
			}

			OnObservableCollectionChanged(Items, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		private void OnObservableCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			ItemsCount = Items?.Count ?? -1;

			ShouldSetIndexAfterPan = IsPanRunning;
			if (!IsPanRunning)
			{
				SetNewIndex();
			}
		}

		private void SetNewIndex()
		{
			if (ItemsCount <= 0)
			{
				CurrentIndex = -1;
				return;
			}

			var index = -1;
			var isCurrentContextPresent = false;

			if (CurrentView != null)
			{
				for (var i = 0; i < ItemsCount; ++i)
				{
					if (Items[i] == CurrentView.BindingContext)
					{
						index = i;
						isCurrentContextPresent = true;
						break;
					}
				}

				if (index < 0)
				{
					index = CurrentIndex - 1;
				}
			}

			if (index < 0)
			{
				index = 0;
			}

			if (CurrentIndex == index)
			{
				if (!isCurrentContextPresent)
				{
					OldIndex = index;
					SetCurrentView();
				}
				return;
			}

			CurrentIndex = index;
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
			lock(_childLocker)
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

		private bool CheckIsProcessingView(View view) => view == CurrentView || NextViews.Contains(view) || PrevViews.Contains(view);

		private void AddRangeViewsInUse()
		{
			lock (_viewsInUseLocker)
			{
				var views = NextViews.Union(PrevViews).Union(Enumerable.Repeat(CurrentView, 1));

				_viewsGestureCounter[_gestureId] = views;

				foreach (var view in views.Where(v => v != null))
				{
					_viewsInUse.Add(view);
				}
			}
		}

		private void RemoveRangeViewsInUse(Guid gestureId)
		{
			lock (_viewsInUseLocker)
			{
				foreach (var view in _viewsGestureCounter[gestureId])
				{
					_viewsInUse.Remove(view);
					if (_gestureId != gestureId && !_viewsInUse.Contains(view))
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

		private void FirePanStarted()
		{
			PanStartedCommand?.Execute(CurrentIndex);
			PanStarted?.Invoke(this, CurrentIndex, 0);
		}

		private void FirePanEnding(bool? isNextSelected, int index, double diff)
		{
			PanEndingCommand?.Execute(index);
			PanEnding?.Invoke(this, index, diff);
			if (isNextSelected.HasValue)
			{
				FirePositionChanging(isNextSelected.GetValueOrDefault());
			}

			CurrentDiff = 0;
		}

		private void FirePanEnded(bool? isNextSelected, int index, double diff)
		{
			PanEndedCommand?.Execute(index);
			PanEnded?.Invoke(this, index, diff);
			if (isNextSelected.HasValue)
			{
				FirePositionChanged(isNextSelected.GetValueOrDefault());
			}
		}

		private void FirePanChanged()
		{
			PanChangedCommand?.Execute(CurrentDiff);
			PanChanged?.Invoke(this, CurrentDiff);
		}

		private void FirePositionChanging(bool isNextSelected)
		{
			PositionChangingCommand?.Execute(isNextSelected);
			PositionChanging?.Invoke(this, isNextSelected);
		}

		private void FirePositionChanged(bool isNextSelected)
		{
			PositionChangedCommand?.Execute(isNextSelected);
			PositionChanged?.Invoke(this, isNextSelected);
		}

		private void FireAutoNavigationStarted(bool isNextSelected)
		{
			AutoNavigationStartedCommand?.Execute(isNextSelected);
			AutoNavigationStarted?.Invoke(this, isNextSelected);
		}

		private void FireAutoNavigationEnded(bool isNextSelected)
		{
			AutoNavigationEndedCommand?.Execute(isNextSelected);
			AutoNavigationEnded?.Invoke(this, isNextSelected);
		}
	}
}