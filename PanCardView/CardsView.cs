using System;
using Xamarin.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Windows.Input;
using PanCardView.Extensions;
using PanCardView.Processors;
using System.Collections;
using PanCardView.Enums;
using static System.Math;
using PanCardView.Behaviors;
using PanCardView.Utility;
using PanCardView.Controls;

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
			var view = bindable.AsCardView();
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
			bindable.AsCardView().SetCurrentView();
			bindable.AsCardView().SetItemsCount();
		});

		public static readonly BindableProperty ItemTemplateProperty = BindableProperty.Create(nameof(ItemTemplate), typeof(DataTemplate), typeof(CardsView), propertyChanged: (bindable, oldValue, newValue) =>
		{
			bindable.AsCardView().SetCurrentView();
		});

		public static readonly BindableProperty CurrentContextProperty = BindableProperty.Create(nameof(CurrentContext), typeof(object), typeof(CardsView), null, BindingMode.OneWay, propertyChanged: (bindable, oldValue, newValue) =>
		{
			bindable.AsCardView().SetCurrentView(true);
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
		private readonly Dictionary<Guid, View[]> _viewsGestureCounter = new Dictionary<Guid, View[]>();
		private readonly List<TimeDiffItem> _timeDiffItems = new List<TimeDiffItem>();
		private readonly ViewsInUseSet _viewsInUse = new ViewsInUseSet();

		private View _currentView;
		private View _nextView;
		private View _prevView;
		private View _currentBackView;
		private AnimationDirection _currentBackAnimationDirection;
		private INotifyCollectionChanged _currentObservableCollection;

		private int _viewsChildrenCount;
		private int _inCoursePanDelay;
		private bool _isPanEndRequested = true;
		private bool _shouldSkipTouch;
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
			if (ItemsCount < 0 && !IsContextMode)
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

		public void StartAutoNavigation(View view, Guid animationId, AnimationDirection animationDirection)
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

		public void EndAutoNavigation(View view, Guid animationId, AnimationDirection animationDirection)
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

		protected virtual void SetupBackViews()
		{
			SetupNextView();
			SetupPrevView();
		}

		protected virtual void SetupLayout(View view)
		{
			SetLayoutBounds(view, new Rectangle(0, 0, 1, 1));
			SetLayoutFlags(view, AbsoluteLayoutFlags.All);
		}

		protected virtual void SetCurrentView(bool canResetContext = false)
		{
			if (TryAutoNavigate())
			{
				return;
			}

			if (TryResetContext(canResetContext, _currentView, CurrentContext))
			{
				return;
			}

			if (Items != null || IsContextMode)
			{
				_currentView = GetView(CurrentIndex, AnimationDirection.Current, FrontViewProcessor);
				if (_currentView == null && CurrentIndex >= 0)
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

		protected virtual bool TryAutoNavigate()
		{
			if (_currentView == null)
			{
				return false;
			}

			var context = GetContext(CurrentIndex, AnimationDirection.Current);

			if (_currentView.BindingContext == context)
			{
				return false;
			}

			var autoNavigatePanPosition = GetAutoNavigateAnimationDirection();

			var oldView = _currentView;
			var view = PrepareView(CurrentIndex, AnimationDirection.Current);
			if (view == null)
			{
				return false;
			}
			_currentView = view;

			BackViewProcessor.HandleInitView(_currentView, this, autoNavigatePanPosition);

			SetupLayout(_currentView);

			AddChild(_currentView, oldView);

			BackViewProcessor.HandleAutoNavigate(oldView, this, autoNavigatePanPosition);
			FrontViewProcessor.HandleAutoNavigate(_currentView, this, autoNavigatePanPosition);

			SetupBackViews();

			return true;
		}

		protected virtual void SetupNextView(bool canResetContext = false)
		{
			if (TryResetContext(canResetContext, _nextView, NextContext))
			{
				return;
			}

			var nextIndex = CurrentIndex + 1;
			_nextView = GetView(nextIndex, AnimationDirection.Next, BackViewProcessor);
		}

		protected virtual void SetupPrevView(bool canResetContext = false)
		{
			if (TryResetContext(canResetContext, _prevView, PrevContext))
			{
				return;
			}

			var prevIndex = IsOnlyForwardDirection
				? CurrentIndex + 1
				: CurrentIndex - 1;
			_prevView = GetView(prevIndex, AnimationDirection.Prev, BackViewProcessor);
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

		private AnimationDirection GetAutoNavigateAnimationDirection()
		{
			if (IsContextMode)
			{
				return CurrentContext == _prevView?.BindingContext
					   ? AnimationDirection.Prev
					   : AnimationDirection.Next;
			}

			if (!IsCyclical)
			{
				return CurrentIndex < Items.IndexOf(_currentView.BindingContext)
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
			if (!IsPanEnabled || deltaTime.TotalMilliseconds < PanDelay)
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

			if (TrySetSelectedBackView(diff))
			{
				_currentBackView.IsVisible = true;
			}

			CurrentDiff = diff;

			SetupDiffItems(diff);

			FirePanChanged();

			FrontViewProcessor.HandlePanChanged(_currentView, this, diff, _currentBackAnimationDirection);
			BackViewProcessor.HandlePanChanged(_currentBackView, this, diff, _currentBackAnimationDirection);
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
					FrontViewProcessor.HandlePanApply(_currentView, this, _currentBackAnimationDirection),
					BackViewProcessor.HandlePanApply(_currentBackView, this, _currentBackAnimationDirection)
				);
			}
			else
			{
				FirePanEnding(isNextSelected, index, diff);
				await Task.WhenAll(
					FrontViewProcessor.HandlePanReset(_currentView, this, _currentBackAnimationDirection),
					BackViewProcessor.HandlePanReset(_currentBackView, this, _currentBackAnimationDirection)
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

		private bool TrySetSelectedBackView(double diff)
		{
			if (diff > 0)
			{
				return SetSelectedBackView(_prevView, _nextView, AnimationDirection.Prev);
			}
			return SetSelectedBackView(_nextView, _prevView, AnimationDirection.Next);
		}

		private bool SetSelectedBackView(View selectedView, View invisibleView, AnimationDirection animationDirection)
		{
			_currentBackView = selectedView;
			_currentBackAnimationDirection = _currentBackView != null
					? animationDirection
					: AnimationDirection.Null;

			if (invisibleView != null && invisibleView != _currentBackView)
			{
				invisibleView.IsVisible = false;
			}

			return _currentBackView != null;
		}

		private void SwapViews(bool isNext)
		{
			var view = _currentView;
			_currentView = _currentBackView;
			_currentBackView = view;

			if (isNext)
			{
				_nextView = _prevView;
				_prevView = _currentBackView;
				return;
			}
			_prevView = _nextView;
			_nextView = _currentBackView;
		}

		private View GetView(int index, AnimationDirection animationDirection, ICardProcessor processor)
		{
			var view = PrepareView(index, animationDirection);
			if (view == null)
			{
				return null;
			}

			processor.HandleInitView(view, this, animationDirection);

			SetupLayout(view);
			if (animationDirection == AnimationDirection.Current)
			{
				AddBackChild(view);
			}
			else
			{
				AddChild(view, _currentView);
			}
			return view;
		}

		private View PrepareView(int index, AnimationDirection animationDirection)
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

			if (template == null)
			{
				return context as View;
			}

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
			var currentContext = context;
			var view = notUsingViews.FirstOrDefault(v => v.BindingContext == currentContext)
									?? notUsingViews.FirstOrDefault(v => v.BindingContext == null)
									?? notUsingViews.FirstOrDefault(v => !CheckIsProcessingView(v));

			if (view == null)
			{
				view = template.CreateView();
				viewsList.Add(view);
			}

			view.BindingContext = context;

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

			if (ItemsCount < 0)
			{
				return null;
			}

			if (index < 0 || index >= ItemsCount)
			{
				if (!IsCyclical || (animationDirection != AnimationDirection.Current && ItemsCount < 2))
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
				view.IsVisible = false;
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
			var index = 0;
			if (_currentView != null)
			{
				for (var i = 0; i < ItemsCount; ++i)
				{
					if (Items[i] == _currentView.BindingContext)
					{
						index = i;
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

			CurrentIndex = index;
		}

		private void AddBackChild(View view)
		{
			lock (_childLocker)
			{
				if (view == null || Children.Contains(view))
				{
					return;
				}

				++_viewsChildrenCount;
				Children.Insert(0, view);
			}
		}

		private void AddChild(View view, View topView)
		{
			lock (_childLocker)
			{
				if (view == null)
				{
					return;
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
				var views = Children.Where(c => !CheckIsProtectedView(c) && c != _prevView && c != _nextView && !c.IsVisible).Take(_viewsChildrenCount - DesiredMaxChildrenCount).ToArray();

				_viewsChildrenCount -= views.Length;

				foreach (var view in views)
				{
					Children.Remove(view);
					CleanView(view);
				}
			}
		}

		private bool CheckIsProcessingView(View view) => view == _currentView || view == _nextView || view == _prevView;

		private void AddRangeViewsInUse()
		{
			lock (_viewsInUseLocker)
			{
				var views = new View[] { _currentView, _nextView, _prevView };

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
