using System;
using System.Collections.Generic;

namespace UIFramework
{
    public class WindowTransitionManager
    {
        private struct TransitionParams
        {
            public WindowTransitionPlayable Transition { get; private set; }
            public IWindow SourceWidow { get; private set; }
            public IWindow TargetWindow { get; private set; }
            public TimeMode TimeMode { get; private set; }

            public TransitionParams(in WindowTransitionPlayable transition, IWindow sourceWindow, IWindow targetWindow, TimeMode timeMode)
            {
                this.Transition = transition;
                this.SourceWidow = sourceWindow;
                this.TargetWindow = targetWindow;
                this.TimeMode = timeMode;
            }

            public override bool Equals(object obj)
            {
                return obj is TransitionParams other && Equals(other);
            }

            public bool Equals(TransitionParams other)
            {
                return Transition == other.Transition && SourceWidow == other.SourceWidow && TargetWindow == other.TargetWindow;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Transition, SourceWidow, TargetWindow);
            }

            public static bool operator ==(TransitionParams lhs, TransitionParams rhs)
            {
                return lhs.Equals(rhs);
            }

            public static bool operator !=(TransitionParams lhs, TransitionParams rhs)
            {
                return !(lhs == rhs);
            }

            public void ReleaseReferences()
            {
                SourceWidow = null;
                TargetWindow = null;
            }

            public AnimationPlayable CreateTargetWindowPlayable(float startOffset, bool reverse)
            {
                return CreateWindowPlayable(TargetWindow, Transition.EntryAnimation, startOffset, Transition.Length, Transition.EasingMode, AccessOperation.Open, reverse);
            }

            public AnimationPlayable CreateSourceWindowPlayable(float startOffset, bool reverse)
            {
                return CreateWindowPlayable(SourceWidow, Transition.ExitAnimation, startOffset, Transition.Length, Transition.EasingMode.GetInverseEasingMode(), AccessOperation.Close, reverse);
            }

            private AnimationPlayable CreateWindowPlayable(IWindow window, ImplicitWindowAnimation animation, float startOffset, float length, EasingMode easingMode, AccessOperation accessOperation, bool reverse)
            {
                if (animation != null)
                {
                    if (reverse)
                    {
                        if (!animation.IsGeneric && !animation.IsAccessAnimation)
                        {
                            AnimationPlayable animationPlayable = animation.CreatePlayable(window, length, accessOperation, startOffset, easingMode, TimeMode);
                            animationPlayable.Invert();
                            return animationPlayable;
                        }
                        else
                        {
                            return animation.CreatePlayable(window, length, accessOperation.InvertAccessOperation(), startOffset, easingMode.GetInverseEasingMode(), TimeMode);
                        }
                    }
                    else
                    {
                        return animation.CreatePlayable(window, length, accessOperation, startOffset, easingMode, TimeMode);
                    }
                }
                return default;
            }
        }

        private class QueuedTransition
        {
            public TransitionParams TransitionParams { get { return _transitionParams; } }
            private TransitionParams _transitionParams = new TransitionParams();
            public bool Reverse { get; private set; } = false;

            private QueuedTransition() { }

            public QueuedTransition(in TransitionParams transitionParams, bool reverse)
            {
                _transitionParams = transitionParams;
                this.Reverse = reverse;
            }

            public bool ValuesAreEqualTo(QueuedTransition other)
            {
                return _transitionParams == other._transitionParams && Reverse == other.Reverse;
            }

            public bool ValuesAreEqualTo(in TransitionParams otherParams, bool otherReverse)
            {
                return _transitionParams == otherParams && Reverse == otherReverse;
            }

            public ref TransitionParams GetTransitionParamsRef()
            {
                return ref _transitionParams;
            }
        }

        public bool IsTransitionActive { get; private set; } = false;
        private TransitionParams _activeTransitionParams = new TransitionParams();
        private bool _isActiveReverse = false;

        private TimeMode _timeMode = TimeMode.Scaled;

        private int _queuedTransitionsCount { get { return _queuedTransitions.Count; } }
        private List<QueuedTransition> _queuedTransitions = new List<QueuedTransition>();

        private AnimationPlayer.PlaybackData _sourceWindowPlaybackData = default;
        private AnimationPlayer.PlaybackData _targetWindowPlaybackData = default;

        private AnimationPlayer.PlaybackData _primaryPlaybackData
        {
            get
            {
                switch (_activeTransitionParams.Transition.Target)
                {
                    case WindowTransitionPlayable.AnimationTarget.Both:
                    case WindowTransitionPlayable.AnimationTarget.Target:
                        return _sourceWindowPlaybackData;
                    case WindowTransitionPlayable.AnimationTarget.Source:
                        return _targetWindowPlaybackData;
                }
                return default;
            }
        }

        private bool _terminating = false;

        private WindowTransitionManager() { }

        public WindowTransitionManager(TimeMode timeMode)
        {
            _timeMode = timeMode;
        }

        public void Transition(in WindowTransitionPlayable transition, IWindow sourceWindow, IWindow targetWindow)
        {
            TransitionParams transitionParams = new TransitionParams(transition, sourceWindow, targetWindow, _timeMode);
            HandleNewTransition(in transitionParams, false);
        }

        public void ReverseTransition(in WindowTransitionPlayable transition, IWindow sourceWindow, IWindow targetWindow)
        {
            TransitionParams transitionParams = new TransitionParams(transition, sourceWindow, targetWindow, _timeMode);
            HandleNewTransition(in transitionParams, true);
        }

        private void HandleNewTransition(in TransitionParams transitionParams, bool reverse)
        {
            if (transitionParams.SourceWidow == null)
            {
                throw new ArgumentNullException(nameof(transitionParams.SourceWidow));
            }

            if (transitionParams.TargetWindow == null)
            {
                throw new ArgumentNullException(nameof(transitionParams.TargetWindow));
            }

            if (!IsTransitionActive)
            {
                ExecuteTransition(in transitionParams, reverse);
            }
            else
            {
                // bool to determin if the new transition reverses the leading transition
                // this is true is we attempt to reverse a transition that is running forward or
                // we attempt to run a forward transition while a matching transition is currently running reversed                
                bool newTransitionOpposesLeading;
                TransitionParams leadingTransitionParams;
                if (_queuedTransitionsCount > 0)
                {
                    // Don't actually need to check this as we ensure that with queued transitions the leading transition should match any that are queued.
                    //if (QueueContainsTransition(in transitionParams, reverse))
                    //{
                    //    throw new InvalidOperationException("Attempting to perform a transition that is already queued.");
                    //}

                    QueuedTransition leadingQueuedTransition = PeekQueuedTransition();
                    leadingTransitionParams = leadingQueuedTransition.TransitionParams;
                    newTransitionOpposesLeading = AreOpposingTransitions(in transitionParams, reverse, in leadingTransitionParams, leadingQueuedTransition.Reverse);
                }
                else
                {
                    if (_activeTransitionParams == transitionParams && _isActiveReverse == reverse)
                    {
                        throw new InvalidOperationException("Attempting to perform a transition that is already active.");
                    }

                    leadingTransitionParams = _activeTransitionParams;
                    newTransitionOpposesLeading = AreOpposingTransitions(in transitionParams, reverse, in leadingTransitionParams, _isActiveReverse);
                }

                // If newTransitionOpposesLeading is true we know that target and source screens match for both transitions as AreOpposingTransitions() checks
                // for the equality of both target and source screens as well as the inequality of reverse.
                if (newTransitionOpposesLeading)
                {
                    if (_queuedTransitionsCount > 0)
                    {
                        // If we are attempting to invert a queued transition we simply pop it from the queue as the operation becomes redundant.
                        _ = PopQueuedTransition();
                    }
                    else
                    {
                        InvertActiveTransition();
                    }
                }
                else
                {
                    if (reverse)
                    {
                        // Ensure that the enqueueing transition is viable given the current leading transition.
                        if (leadingTransitionParams.SourceWidow != transitionParams.TargetWindow)
                        {
                            throw new InvalidOperationException(
                                "Attempting to reverse a transition to a previous screen while the leading transitions source differs from the reverse transitions target.");
                        }

                        EnqueueTransition(in transitionParams, reverse);
                    }
                    else
                    {
                        // Ensure that the enqueueing transition is viable given the current leading transition.
                        if (leadingTransitionParams.TargetWindow != transitionParams.SourceWidow)
                        {
                            throw new InvalidOperationException(
                                "Attempting to transition to a new screen while the leading transitions target differs from the new transitions source.");
                        }

                        EnqueueTransition(in transitionParams, reverse);
                    }
                }
            }
        }

        private void ExecuteTransition(in TransitionParams transitionParams, bool reverse)
        {
            IsTransitionActive = true;
            _isActiveReverse = reverse;
            _activeTransitionParams = transitionParams;

            if (_activeTransitionParams.Transition.WindowSortPriority == WindowTransitionPlayable.SortPriority.Auto)
            {
                switch (_activeTransitionParams.Transition.Target)
                {
                    default:
                    case WindowTransitionPlayable.AnimationTarget.Both:
                        _activeTransitionParams.SourceWidow.SortOrder = 0;
                        _activeTransitionParams.TargetWindow.SortOrder = 0;
                        break;
                    case WindowTransitionPlayable.AnimationTarget.Target:
                        _activeTransitionParams.SourceWidow.SortOrder = 0;
                        _activeTransitionParams.TargetWindow.SortOrder = 1;
                        break;
                    case WindowTransitionPlayable.AnimationTarget.Source:
                        _activeTransitionParams.SourceWidow.SortOrder = 1;
                        _activeTransitionParams.TargetWindow.SortOrder = 0;
                        break;
                }
            }
            else
            {
                switch (_activeTransitionParams.Transition.WindowSortPriority)
                {
                    case WindowTransitionPlayable.SortPriority.Source:
                        _activeTransitionParams.SourceWidow.SortOrder = 1;
                        _activeTransitionParams.TargetWindow.SortOrder = 0;
                        break;
                    case WindowTransitionPlayable.SortPriority.Target:
                        _activeTransitionParams.SourceWidow.SortOrder = 0;
                        _activeTransitionParams.TargetWindow.SortOrder = 1;
                        break;
                }
            }

            if (_activeTransitionParams.Transition.Target == WindowTransitionPlayable.AnimationTarget.None)
            {
                ExecuteNone(in _activeTransitionParams, reverse);
                CompleteTransition();
            }
            else
            {
                ExecuteTransition(in _activeTransitionParams, _isActiveReverse, 0.0F);
            }
        }

        private void InvertActiveTransition()
        {
            if (_activeTransitionParams.Transition.Target == WindowTransitionPlayable.AnimationTarget.None)
            {
                throw new InvalidOperationException("Cannot invert transition with no animation targets.");
            }

            _isActiveReverse = !_isActiveReverse;

            float normalisedStartOffset = _isActiveReverse ? 1.0F - _primaryPlaybackData.CurrentNormalisedTime : _primaryPlaybackData.CurrentNormalisedTime;
            ExecuteTransition(in _activeTransitionParams, _isActiveReverse, normalisedStartOffset);
        }

        private void ExecuteTransition(in TransitionParams transitionParams, bool reverse, float normalisedStartOffset)
        {
            float startOffset = transitionParams.Transition.Length * normalisedStartOffset;
            switch (_activeTransitionParams.Transition.Target)
            {
                case WindowTransitionPlayable.AnimationTarget.Both:
                    ExecuteOnBoth(in transitionParams, reverse, startOffset);
                    break;
                case WindowTransitionPlayable.AnimationTarget.Target:
                    ExecuteOnTarget(in transitionParams, reverse, startOffset);
                    break;
                case WindowTransitionPlayable.AnimationTarget.Source:
                    ExecuteOnSource(in transitionParams, reverse, startOffset);
                    break;
            }
        }

        private void ExecuteNone(in TransitionParams transitionParams, bool reverse)
        {
            if (reverse)
            {
                transitionParams.SourceWidow.Open();
                transitionParams.TargetWindow.Close();
            }
            else
            {
                transitionParams.SourceWidow.Close();
                transitionParams.TargetWindow.Open();
            }
        }

        private void ExecuteOnTarget(in TransitionParams transitionParams, bool reverse, float startOffset)
        {
            if (reverse)
            {
                transitionParams.SourceWidow.Open();

                AnimationPlayable targetWindowPlayable = transitionParams.CreateTargetWindowPlayable(startOffset, reverse);
                transitionParams.TargetWindow.Close(in targetWindowPlayable, OnAccessAnimationComplete);
            }
            else
            {
                AnimationPlayable targetWindowPlayable = transitionParams.CreateTargetWindowPlayable(startOffset, reverse);
                transitionParams.TargetWindow.Open(in targetWindowPlayable, OnAccessAnimationComplete);
            }
            _targetWindowPlaybackData = transitionParams.TargetWindow.AccessAnimationPlaybackData;
        }

        private void ExecuteOnSource(in TransitionParams transitionParams, bool reverse, float startOffset)
        {
            if (reverse)
            {
                AnimationPlayable sourceWindowPlayable = transitionParams.CreateSourceWindowPlayable(startOffset, reverse);
                transitionParams.SourceWidow.Open(in sourceWindowPlayable, OnAccessAnimationComplete);
            }
            else
            {
                transitionParams.TargetWindow.Open();

                AnimationPlayable sourceWindowPlayable = transitionParams.CreateSourceWindowPlayable(startOffset, reverse);
                transitionParams.SourceWidow.Close(in sourceWindowPlayable, OnAccessAnimationComplete);
            }
            _sourceWindowPlaybackData = transitionParams.SourceWidow.AccessAnimationPlaybackData;
        }

        private void ExecuteOnBoth(in TransitionParams transitionParams, bool reverse, float startOffset)
        {
            if (reverse)
            {
                AnimationPlayable sourceWindowPlayable = transitionParams.CreateSourceWindowPlayable(startOffset, reverse);
                transitionParams.SourceWidow.Open(in sourceWindowPlayable, OnAccessAnimationComplete);

                AnimationPlayable targetWindowPlayable = transitionParams.CreateTargetWindowPlayable(startOffset, reverse);
                transitionParams.TargetWindow.Close(in targetWindowPlayable, OnAccessAnimationComplete);
            }
            else
            {
                AnimationPlayable sourceWindowPlayable = transitionParams.CreateSourceWindowPlayable(startOffset, reverse);
                transitionParams.SourceWidow.Close(in sourceWindowPlayable, OnAccessAnimationComplete);

                AnimationPlayable targetWindowPlayable = transitionParams.CreateTargetWindowPlayable(startOffset, reverse);
                transitionParams.TargetWindow.Open(in targetWindowPlayable, OnAccessAnimationComplete);
            }
            _targetWindowPlaybackData = transitionParams.TargetWindow.AccessAnimationPlaybackData;
            _sourceWindowPlaybackData = transitionParams.SourceWidow.AccessAnimationPlaybackData;
        }

        private void EnqueueTransition(in TransitionParams transitionParams, bool reverse)
        {
            QueuedTransition queuedTransition = new QueuedTransition(in transitionParams, reverse);
            _queuedTransitions.Add(queuedTransition);
        }

        private QueuedTransition DequeueTransition()
        {
            if (_queuedTransitionsCount == 0)
            {
                throw new InvalidOperationException("The transition queue is empty.");
            }
            QueuedTransition dequeuedTransition = _queuedTransitions[0];
            _queuedTransitions.RemoveAt(0);
            return dequeuedTransition;
        }

        private QueuedTransition PopQueuedTransition()
        {
            if (_queuedTransitionsCount == 0)
            {
                throw new InvalidOperationException("The transition queue is empty.");
            }
            int popIndex = _queuedTransitionsCount - 1;
            QueuedTransition poppedTransition = _queuedTransitions[popIndex];
            _queuedTransitions.RemoveAt(popIndex);
            return poppedTransition;
        }

        private QueuedTransition PeekQueuedTransition()
        {
            if (_queuedTransitionsCount == 0)
            {
                throw new InvalidOperationException("The transition queue is empty.");
            }
            return _queuedTransitions[_queuedTransitionsCount - 1];
        }

        private bool QueueContainsTransition(in TransitionParams transitionParams, bool reverse)
        {
            for (int i = _queuedTransitionsCount - 1; i >= 0; i--)
            {
                if (_queuedTransitions[i].TransitionParams == transitionParams && _queuedTransitions[i].Reverse == reverse)
                {
                    return true;
                }
            }
            return false;
        }

        private void CompleteTransition()
        {
            if (_activeTransitionParams.Transition.Target != WindowTransitionPlayable.AnimationTarget.Both)
            {
                if (_isActiveReverse)
                {
                    if (_activeTransitionParams.Transition.Target == WindowTransitionPlayable.AnimationTarget.Source)
                    {
                        _activeTransitionParams.TargetWindow.Close();
                    }
                }
                else
                {
                    if (_activeTransitionParams.Transition.Target == WindowTransitionPlayable.AnimationTarget.Target)
                    {
                        _activeTransitionParams.SourceWidow.Close();
                    }
                }
            }

            if (_queuedTransitionsCount > 0)
            {
                QueuedTransition queuedTransition = DequeueTransition();
                ExecuteTransition(in queuedTransition.GetTransitionParamsRef(), queuedTransition.Reverse);
            }
            else
            {
                IsTransitionActive = false;
                _isActiveReverse = false;
                _activeTransitionParams.ReleaseReferences();
            }
        }

        private void OnAccessAnimationComplete(IAccessible accessible)
        {
            if (!_terminating)
            {
                if (accessible == _activeTransitionParams.SourceWidow)
                {
                    _sourceWindowPlaybackData.ReleaseReferences();
                }

                if (accessible == _activeTransitionParams.TargetWindow)
                {
                    _targetWindowPlaybackData.ReleaseReferences();
                }

                if (!_sourceWindowPlaybackData.IsPlaying && !_targetWindowPlaybackData.IsPlaying)
                {
                    CompleteTransition();
                }
            }
        }

        private bool AreOpposingTransitions(in TransitionParams lhsTransitionParams, bool lhsReverse, in TransitionParams rhsTransitionParams, bool rhsReverse)
        {
            return lhsTransitionParams.TargetWindow == rhsTransitionParams.TargetWindow && lhsTransitionParams.SourceWidow == rhsTransitionParams.SourceWidow &&
                lhsReverse != rhsReverse;
        }

        public void Terminate(bool skipAnimations)
        {
            _terminating = true;
            if (IsTransitionActive)
            {
                _targetWindowPlaybackData.ReleaseReferences();
                _sourceWindowPlaybackData.ReleaseReferences();

                if (skipAnimations)
                {
                    switch (_activeTransitionParams.Transition.Target)
                    {
                        case WindowTransitionPlayable.AnimationTarget.Source:
                            _activeTransitionParams.SourceWidow.SkipAccessAnimation();
                            if (_isActiveReverse)
                            {
                                _activeTransitionParams.TargetWindow.Close();
                            }
                            break;
                        case WindowTransitionPlayable.AnimationTarget.Target:
                            _activeTransitionParams.TargetWindow.SkipAccessAnimation();
                            if (!_isActiveReverse)
                            {
                                _activeTransitionParams.SourceWidow.Close();
                            }
                            break;
                        case WindowTransitionPlayable.AnimationTarget.Both:
                            _activeTransitionParams.SourceWidow.SkipAccessAnimation();
                            _activeTransitionParams.TargetWindow.SkipAccessAnimation();
                            break;
                    }
                }

                IsTransitionActive = false;
                _isActiveReverse = false;
                _activeTransitionParams.ReleaseReferences();
                _queuedTransitions.Clear();
            }
            _terminating = false;
        }
    }
}