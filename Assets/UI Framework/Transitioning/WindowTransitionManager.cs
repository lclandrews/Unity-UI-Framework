using System;
using System.Collections.Generic;

using UnityEngine;

namespace UIFramework
{
    public class WindowTransitionManager
    {
        private struct TransitionParams
        {
            public WindowTransitionPlayable transition { get; private set; }
            public IWindow sourceWidow { get; private set; }
            public IWindow targetWindow { get; private set; }
            public bool unscaledTime { get; private set; }

            public TransitionParams(in WindowTransitionPlayable transition, IWindow sourceWindow, IWindow targetWindow, bool unscaledTime)
            {
                this.transition = transition;
                this.sourceWidow = sourceWindow;
                this.targetWindow = targetWindow;
                this.unscaledTime = unscaledTime;
            }

            public override bool Equals(object obj)
            {
                return obj is TransitionParams other && Equals(other);
            }

            public bool Equals(TransitionParams other)
            {
                return transition == other.transition && sourceWidow == other.sourceWidow && targetWindow == other.targetWindow;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(transition, sourceWidow, targetWindow);
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
                sourceWidow = null;
                targetWindow = null;
            }

            public AnimationPlayable CreateTargetWindowPlayable(float startTime, PlayMode playMode, bool inverseEasingMode)
            {
                if(transition.entryAnimation != null)
                {
                    EasingMode easingMode = inverseEasingMode ? transition.easingMode.GetInverseEasingMode() : transition.easingMode;
                    return transition.entryAnimation.CreatePlayable(targetWindow, transition.length, startTime, playMode, easingMode, unscaledTime);
                }
                return default;
            }

            public AnimationPlayable CreateSourceWindowPlayable(float startTime, PlayMode playMode, bool inverseEasingMode)
            {
                if (transition.exitAnimation != null)
                {
                    EasingMode easingMode = inverseEasingMode ? transition.easingMode.GetInverseEasingMode() : transition.easingMode;
                    return transition.exitAnimation.CreatePlayable(sourceWidow, transition.length, startTime, playMode, easingMode, unscaledTime);
                }
                return default;
            }
        }

        private class QueuedTransition
        {
            public TransitionParams transitionParams { get { return _transitionParams; } }
            private TransitionParams _transitionParams = new TransitionParams();
            public bool reverse { get; private set; } = false;

            private QueuedTransition() { }

            public QueuedTransition(in TransitionParams transitionParams, bool reverse)
            {
                _transitionParams = transitionParams;
                this.reverse = reverse;
            }

            public bool ValuesAreEqualTo(QueuedTransition other)
            {
                return _transitionParams == other._transitionParams && reverse == other.reverse;
            }

            public bool ValuesAreEqualTo(in TransitionParams otherParams, bool otherReverse)
            {
                return _transitionParams == otherParams && reverse == otherReverse;
            }

            public ref TransitionParams GetTransitionParamsRef()
            {
                return ref _transitionParams;
            }
        }

        public bool isTransitionActive { get; private set; } = false;
        private TransitionParams _activeTransitionParams = new TransitionParams();
        private bool _isActiveReverse = false;

        private bool _unscaledTime = false;

        private int _queuedTransitionsCount { get { return _queuedTransitions.Count; } }
        private List<QueuedTransition> _queuedTransitions = new List<QueuedTransition>();

        private AnimationPlayer.PlaybackData _sourceWindowPlaybackData = default;
        private AnimationPlayer.PlaybackData _targetWindowPlaybackData = default;       
        
        private AnimationPlayer.PlaybackData _primaryPlaybackData
        {
            get
            {
                switch (_activeTransitionParams.transition.animationTargets)
                {
                    case WindowTransitionPlayable.AnimationTargets.Both:
                    case WindowTransitionPlayable.AnimationTargets.Target:
                        return _sourceWindowPlaybackData;
                    case WindowTransitionPlayable.AnimationTargets.Source:
                        return _targetWindowPlaybackData;
                }
                return default;
            }
        }

        private bool _terminating = false;

        private WindowTransitionManager() { }

        public WindowTransitionManager(bool unscaledTime)
        {
            this._unscaledTime = unscaledTime;
        }

        public void Transition(in WindowTransitionPlayable transition, IWindow sourceWindow, IWindow targetWindow)
        {
            TransitionParams transitionParams = new TransitionParams(transition, sourceWindow, targetWindow);
            HandleNewTransition(in transitionParams, false);
        }

        public void ReverseTransition(in WindowTransitionPlayable transition, IWindow sourceWindow, IWindow targetWindow)
        {
            TransitionParams transitionParams = new TransitionParams(transition, sourceWindow, targetWindow);
            HandleNewTransition(in transitionParams, true);
        }

        private void HandleNewTransition(in TransitionParams transitionParams, bool reverse)
        {
            if (transitionParams.sourceWidow == null)
            {
                throw new ArgumentNullException(nameof(transitionParams.sourceWidow));
            }

            if (transitionParams.targetWindow == null)
            {
                throw new ArgumentNullException(nameof(transitionParams.targetWindow));
            }

            if (!isTransitionActive)
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
                    leadingTransitionParams = leadingQueuedTransition.transitionParams;
                    newTransitionOpposesLeading = AreOpposingTransitions(in transitionParams, reverse, in leadingTransitionParams, leadingQueuedTransition.reverse);
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
                        if (leadingTransitionParams.sourceWidow != transitionParams.targetWindow)
                        {
                            throw new InvalidOperationException(
                                "Attempting to reverse a transition to a previous screen while the leading transitions source differs from the reverse transitions target.");
                        }

                        EnqueueTransition(in transitionParams, reverse);
                    }
                    else
                    {
                        // Ensure that the enqueueing transition is viable given the current leading transition.
                        if (leadingTransitionParams.targetWindow != transitionParams.sourceWidow)
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
            isTransitionActive = true;
            _isActiveReverse = reverse;
            _activeTransitionParams = transitionParams;

            if(_activeTransitionParams.transition.sortPriority == WindowTransitionPlayable.SortPriority.Auto)
            {
                switch (_activeTransitionParams.transition.animationTargets)
                {
                    default:
                    case WindowTransitionPlayable.AnimationTargets.Both:
                        _activeTransitionParams.sourceWidow.sortOrder = 0;
                        _activeTransitionParams.targetWindow.sortOrder = 0;
                        break;
                    case WindowTransitionPlayable.AnimationTargets.Target:
                        _activeTransitionParams.sourceWidow.sortOrder = 0;
                        _activeTransitionParams.targetWindow.sortOrder = 1;
                        break;
                    case WindowTransitionPlayable.AnimationTargets.Source:
                        _activeTransitionParams.sourceWidow.sortOrder = 1;
                        _activeTransitionParams.targetWindow.sortOrder = 0;
                        break;
                }
            }
            else
            {
                switch (_activeTransitionParams.transition.sortPriority)
                {
                    case WindowTransitionPlayable.SortPriority.Source:
                        _activeTransitionParams.sourceWidow.sortOrder = 1;
                        _activeTransitionParams.targetWindow.sortOrder = 0;
                        break;
                    case WindowTransitionPlayable.SortPriority.Target:
                        _activeTransitionParams.sourceWidow.sortOrder = 0;
                        _activeTransitionParams.targetWindow.sortOrder = 1;
                        break;
                }
            }

            if (_activeTransitionParams.transition.animationTargets == WindowTransitionPlayable.AnimationTargets.None)
            {
                ExecuteNone(in _activeTransitionParams, reverse);
                CompleteTransition();
            }
            else
            {              
                float normalisedStartTime = _isActiveReverse ? 1.0F : 0.0F;
                ExecuteTransition(in _activeTransitionParams, _isActiveReverse, normalisedStartTime);
            }
        }

        private void InvertActiveTransition()
        {
            if (_activeTransitionParams.transition.animationTargets == WindowTransitionPlayable.AnimationTargets.None)
            {
                throw new InvalidOperationException("Cannot invert transition with no animation targets.");
            }

            _isActiveReverse = !_isActiveReverse;


            float normalisedStartTime = _primaryPlaybackData.currentNormalisedTime;
            ExecuteTransition(in _activeTransitionParams, _isActiveReverse, normalisedStartTime);
        }

        private void ExecuteTransition(in TransitionParams transitionParams, bool reverse, float normalisedStartTime)
        {
            float startTime = transitionParams.transition.length * normalisedStartTime;
            switch (_activeTransitionParams.transition.animationTargets)
            {
                case WindowTransitionPlayable.AnimationTargets.Both:
                    ExecuteOnBoth(in transitionParams, reverse, startTime);
                    break;
                case WindowTransitionPlayable.AnimationTargets.Target:
                    ExecuteOnTarget(in transitionParams, reverse, startTime);
                    break;
                case WindowTransitionPlayable.AnimationTargets.Source:
                    ExecuteOnSource(in transitionParams, reverse, startTime);
                    break;
            }
        }

        private void ExecuteNone(in TransitionParams transitionParams, bool reverse)
        {
            if (reverse)
            {
                transitionParams.sourceWidow.Open();
            }
            else
            {
                transitionParams.targetWindow.Open();
            }
        }

        private void ExecuteOnTarget(in TransitionParams transitionParams, bool reverse, float startTime)
        {
            if (reverse)
            {
                transitionParams.sourceWidow.Open();

                AnimationPlayable targetWindowPlayable = transitionParams.CreateTargetWindowPlayable(startTime, PlayMode.Reverse, true);
                transitionParams.targetWindow.Close(in targetWindowPlayable, OnAccessAnimationComplete);
            }
            else
            {
                AnimationPlayable targetWindowPlayable = transitionParams.CreateTargetWindowPlayable(startTime, PlayMode.Forward, false);
                transitionParams.targetWindow.Open(in targetWindowPlayable, OnAccessAnimationComplete);
            }
            _targetWindowPlaybackData = transitionParams.targetWindow.accessAnimationPlaybackData;
        }

        private void ExecuteOnSource(in TransitionParams transitionParams, bool reverse, float startTime)
        {
            startTime = transitionParams.transition.length - startTime;
            if (reverse)
            {
                AnimationPlayable sourceWindowPlayable = transitionParams.CreateSourceWindowPlayable(startTime, PlayMode.Forward, true);
                transitionParams.sourceWidow.Open(in sourceWindowPlayable, OnAccessAnimationComplete);
            }
            else
            {
                transitionParams.targetWindow.Open();

                AnimationPlayable sourceWindowPlayable = transitionParams.CreateSourceWindowPlayable(startTime, PlayMode.Reverse, false);
                transitionParams.sourceWidow.Close(in sourceWindowPlayable, OnAccessAnimationComplete);
            }
            _sourceWindowPlaybackData = transitionParams.sourceWidow.accessAnimationPlaybackData;
        }

        private void ExecuteOnBoth(in TransitionParams transitionParams, bool reverse, float startTime)
        {
            float targetStartTime = startTime;
            float sourceStartTime = transitionParams.transition.length - startTime;
            if (reverse)
            {
                AnimationPlayable sourceWindowPlayable = transitionParams.CreateSourceWindowPlayable(startTime, PlayMode.Forward, true);
                transitionParams.sourceWidow.Open(in sourceWindowPlayable, OnAccessAnimationComplete);

                AnimationPlayable targetWindowPlayable = transitionParams.CreateTargetWindowPlayable(startTime, PlayMode.Reverse, true);
                transitionParams.targetWindow.Close(in targetWindowPlayable, OnAccessAnimationComplete);
            }
            else
            {
                AnimationPlayable sourceWindowPlayable = transitionParams.CreateSourceWindowPlayable(startTime, PlayMode.Reverse, false);
                transitionParams.sourceWidow.Close(in sourceWindowPlayable, OnAccessAnimationComplete);

                AnimationPlayable targetWindowPlayable = transitionParams.CreateTargetWindowPlayable(startTime, PlayMode.Forward, false);
                transitionParams.targetWindow.Open(in targetWindowPlayable, OnAccessAnimationComplete);
            }
            _targetWindowPlaybackData = transitionParams.targetWindow.accessAnimationPlaybackData;
            _sourceWindowPlaybackData = transitionParams.sourceWidow.accessAnimationPlaybackData;
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
                if (_queuedTransitions[i].transitionParams == transitionParams && _queuedTransitions[i].reverse == reverse)
                {
                    return true;
                }
            }
            return false;
        }

        private void CompleteTransition()
        {
            if (_activeTransitionParams.transition.animationTargets != WindowTransitionPlayable.AnimationTargets.Both)
            {
                if (_isActiveReverse)
                {
                    if (_activeTransitionParams.transition.animationTargets == WindowTransitionPlayable.AnimationTargets.Source)
                    {
                        _activeTransitionParams.targetWindow.Close();
                    }
                }
                else
                {
                    if (_activeTransitionParams.transition.animationTargets == WindowTransitionPlayable.AnimationTargets.Target)
                    {
                        _activeTransitionParams.sourceWidow.Close();
                    }
                }
            }            

            if (_queuedTransitionsCount > 0)
            {
                QueuedTransition queuedTransition = DequeueTransition();
                ExecuteTransition(in queuedTransition.GetTransitionParamsRef(), queuedTransition.reverse);
            }
            else
            {
                isTransitionActive = false;
                _isActiveReverse = false;
                _activeTransitionParams.ReleaseReferences();
            }
        }

        private void OnAccessAnimationComplete(IAccessible accessible)
        {
            if(!_terminating)
            {
                if (accessible == _activeTransitionParams.sourceWidow)
                {
                    _sourceWindowPlaybackData.ReleaseReferences();
                }

                if (accessible == _activeTransitionParams.targetWindow)
                {
                    _targetWindowPlaybackData.ReleaseReferences();
                }

                if (!_sourceWindowPlaybackData.isPlaying && !_targetWindowPlaybackData.isPlaying)
                {                    
                    CompleteTransition();
                }
            }                     
        }

        private bool AreOpposingTransitions(in TransitionParams lhsTransitionParams, bool lhsReverse, in TransitionParams rhsTransitionParams, bool rhsReverse)
        {
            return lhsTransitionParams.targetWindow == rhsTransitionParams.targetWindow && lhsTransitionParams.sourceWidow == rhsTransitionParams.sourceWidow &&
                lhsReverse != rhsReverse;
        }

        public void Terminate(bool skipAnimations)
        {
            _terminating = true;
            if (isTransitionActive)
            {
                _targetWindowPlaybackData.ReleaseReferences();
                _sourceWindowPlaybackData.ReleaseReferences();

                if (skipAnimations)
                {
                    switch (_activeTransitionParams.transition.animationTargets)
                    {
                        case WindowTransitionPlayable.AnimationTargets.Source:
                            _activeTransitionParams.sourceWidow.SkipAccessAnimation();
                            if (_isActiveReverse)
                            {
                                _activeTransitionParams.targetWindow.Close();
                            }
                            break;
                        case WindowTransitionPlayable.AnimationTargets.Target:
                            _activeTransitionParams.targetWindow.SkipAccessAnimation();
                            if(!_isActiveReverse)
                            {
                                _activeTransitionParams.sourceWidow.Close();
                            }
                            break;
                        case WindowTransitionPlayable.AnimationTargets.Both:
                            _activeTransitionParams.sourceWidow.SkipAccessAnimation();
                            _activeTransitionParams.targetWindow.SkipAccessAnimation();
                            break;
                    }
                }

                isTransitionActive = false;
                _isActiveReverse = false;
                _activeTransitionParams.ReleaseReferences();
                _queuedTransitions.Clear();
            }
            _terminating = false;
        }
    }
}