using System;
using System.Collections;
using System.Collections.Generic;
using Xamarin.Forms;
using System.Threading.Tasks;

namespace PanCardView.Utility
{
    public sealed class AnimationWrapper : IEnumerable
    {
        private readonly List<AnimationWrapper> _children;

        public AnimationWrapper(Action<double> callback = null, double start = 0, double end = 1)
        {
            _children = new List<AnimationWrapper>();
            Callback = callback;
            Start = start;
            End = end;
        }

        public IEnumerator GetEnumerator() => _children.GetEnumerator();

        public double Start { get; private set; }

        public double End { get; private set; }

        public double BeginsAt { get; private set; }

        public double FinishsAt { get; private set; }

        public Action<double> Callback { get; private set; }

        public void Add(double beginAt, double finishAt, AnimationWrapper animation)
        {
            animation.BeginsAt = beginAt;
            animation.FinishsAt = finishAt;
            _children.Add(animation);
        }

        public Task Commit(View view, string name, uint rate = 16u, uint length = 250u, Easing easing = null)
        {
            if (DependencyService.Get<IAnimationsChecker>()?.AreAnimationsEnabled ?? true)
            {
                var tcs = new TaskCompletionSource<bool>();
                PrepareAnimation(this).Commit(view, name, rate, length, easing, (d, b) => tcs.SetResult(true));
                return tcs.Task;
            }

            try
            {
                view.BatchBegin();
                CommitWithoutAnimation(this);
            }
            finally
            {
                view.BatchCommit();
            }
            return Task.FromResult(true);
        }

        private void CommitWithoutAnimation(AnimationWrapper animation)
        {
            foreach (AnimationWrapper childAnimation in animation)
            {
                CommitWithoutAnimation(childAnimation);
            }
            animation.Callback?.Invoke(animation.End);
        }

        private Animation PrepareAnimation(AnimationWrapper animationWrapper)
        {
            var parentAnimation = animationWrapper.Callback != null
                ? new Animation(animationWrapper.Callback, animationWrapper.Start, animationWrapper.End)
                : new Animation();
            
            foreach (AnimationWrapper childAnimation in animationWrapper)
            {
                var anim = PrepareAnimation(childAnimation);
                parentAnimation.Add(childAnimation.BeginsAt, childAnimation.FinishsAt, anim);
            }
            return parentAnimation;
        }
    }
}