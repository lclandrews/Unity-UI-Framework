using System;
using System.Collections.Generic;

using UnityEngine;

namespace UIFramework
{
    public class TransitionManager
    {
        private struct TransitionParams
        {
            public WindowTransition transition { get; private set; }
            public IWindow sourceWidow { get; private set; }
            public IWindow targetWindow { get; private set; }

            public TransitionParams(in WindowTransition transition, IWindow sourceWindow, IWindow targetWindow)
            {
                this.transition = transition;
                this.sourceWidow = sourceWindow;
                this.targetWindow = targetWindow;
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

        private int _queuedTransitionsCount { get { return _queuedTransitions.Count; } }
        private List<QueuedTransition> _queuedTransitions = new List<QueuedTransition>();

        private IWindowAnimator _sourceWindowAnimator = null;
        private IWindowAnimator _targetWindowAnimator = null;

        private IWindowAnimator _primaryWindowAnimator
        {
            get
            {
                switch (_activeTransitionParams.transition.animationTargets)
                {
                    case WindowTransition.AnimationTargets.Both:
                    case WindowTransition.AnimationTargets.Target:
                        return _targetWindowAnimator;
                    case WindowTransition.AnimationTargets.Source:
                        return _sourceWindowAnimator;
                }
                return null;
            }
        }

        public void Transition(in WindowTransition transition, IWindow sourceWindow, IWindow targetWindow)
        {
            TransitionParams transitionParams = new TransitionParams(transition, sourceWindow, targetWindow);
            HandleNewTransition(in transitionParams, false);
        }

        public void ReverseTransition(in WindowTransition transition, IWindow sourceWindow, IWindow targetWindow)
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

            TransitionParams? fallbackTransitionParams;
            if (IsSupportedTransition(in transitionParams, out fallbackTransitionParams))
            {
                _activeTransitionParams = transitionParams;
            }
            else
            {
                _activeTransitionParams = fallbackTransitionParams.Value;
                Debug.LogWarning(string.Format("Transition type {0} > {1} is not supported for the given target and source, falling back to {2} > {3}", 
                    transitionParams.transition.exitAnimation, transitionParams.transition.entryAnimation, 
                    _activeTransitionParams.transition.exitAnimation, _activeTransitionParams.transition.entryAnimation));
            }

            if(_activeTransitionParams.transition.sortPriority == WindowTransition.SortPriority.Auto)
            {
                switch (_activeTransitionParams.transition.animationTargets)
                {
                    default:
                    case WindowTransition.AnimationTargets.Both:
                        _activeTransitionParams.sourceWidow.sortOrder = 0;
                        _activeTransitionParams.targetWindow.sortOrder = 0;
                        break;
                    case WindowTransition.AnimationTargets.Target:
                        _activeTransitionParams.sourceWidow.sortOrder = 0;
                        _activeTransitionParams.targetWindow.sortOrder = 1;
                        break;
                    case WindowTransition.AnimationTargets.Source:
                        _activeTransitionParams.sourceWidow.sortOrder = 1;
                        _activeTransitionParams.targetWindow.sortOrder = 0;
                        break;
                }
            }
            else
            {
                switch (_activeTransitionParams.transition.sortPriority)
                {
                    case WindowTransition.SortPriority.Source:
                        _activeTransitionParams.sourceWidow.sortOrder = 1;
                        _activeTransitionParams.targetWindow.sortOrder = 0;
                        break;
                    case WindowTransition.SortPriority.Target:
                        _activeTransitionParams.sourceWidow.sortOrder = 0;
                        _activeTransitionParams.targetWindow.sortOrder = 1;
                        break;
                }
            }

            if (_activeTransitionParams.transition.animationTargets == WindowTransition.AnimationTargets.None)
            {
                ExecuteNone(in _activeTransitionParams, reverse);
                CompleteTransition();
            }
            else
            {
                switch (_activeTransitionParams.transition.animationTargets)
                {
                    case WindowTransition.AnimationTargets.Both:
                        _sourceWindowAnimator = _activeTransitionParams.sourceWidow.animator;
                        _sourceWindowAnimator.onComplete += OnAnimationComplete;
                        _targetWindowAnimator = _activeTransitionParams.targetWindow.animator;
                        _targetWindowAnimator.onComplete += OnAnimationComplete;
                        break;
                    case WindowTransition.AnimationTargets.Source:
                        _sourceWindowAnimator = _activeTransitionParams.sourceWidow.animator;
                        _sourceWindowAnimator.onComplete += OnAnimationComplete;
                        break;
                    case WindowTransition.AnimationTargets.Target:
                        _targetWindowAnimator = _activeTransitionParams.targetWindow.animator;
                        _targetWindowAnimator.onComplete += OnAnimationComplete;
                        break;
                }                

                // Removed as this is now expected to be handled by the screen / window itself.
                //_activeTransitionParams.sourceWindow.isInteractable = false;
                //_activeTransitionParams.targetWindow.isInteractable = false;
                float normalisedStartTime = _isActiveReverse ? 1.0F : 0.0F;
                ExecuteTransition(in _activeTransitionParams, _isActiveReverse, normalisedStartTime);
            }
        }

        private void InvertActiveTransition()
        {
            if (_activeTransitionParams.transition.animationTargets == WindowTransition.AnimationTargets.None)
            {
                throw new InvalidOperationException("Cannot invert transition with no animation targets.");
            }

            _isActiveReverse = !_isActiveReverse;


            float normalisedStartTime = _primaryWindowAnimator.currentNormalisedTime;
            ExecuteTransition(in _activeTransitionParams, _isActiveReverse, normalisedStartTime);
        }

        private void ExecuteTransition(in TransitionParams transitionParams, bool reverse, float normalisedStartTime)
        {
            float startTime = transitionParams.transition.length * normalisedStartTime;
            switch (_activeTransitionParams.transition.animationTargets)
            {
                case WindowTransition.AnimationTargets.Both:
                    ExecuteOnBoth(in transitionParams, reverse, startTime);
                    break;
                case WindowTransition.AnimationTargets.Target:
                    ExecuteOnTarget(in transitionParams, reverse, startTime);
                    break;
                case WindowTransition.AnimationTargets.Source:
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

                WindowAnimation targetWindowAnimation = new WindowAnimation(transitionParams.transition.entryAnimation.Value, transitionParams.transition.length, 
                    Easing.GetInverseEasingMode(transitionParams.transition.easingMode), startTime, PlayMode.Reverse);
                transitionParams.targetWindow.Close(targetWindowAnimation);
            }
            else
            {
                WindowAnimation targetWindowAnimation = new WindowAnimation(transitionParams.transition.entryAnimation.Value, transitionParams.transition.length, 
                    transitionParams.transition.easingMode, startTime, PlayMode.Forward);
                transitionParams.targetWindow.Open(targetWindowAnimation);
            }
        }

        private void ExecuteOnSource(in TransitionParams transitionParams, bool reverse, float startTime)
        {
            startTime = transitionParams.transition.length - startTime;
            if (reverse)
            {
                WindowAnimation sourceWindowAnimation = new WindowAnimation(transitionParams.transition.exitAnimation.Value, transitionParams.transition.length, 
                    Easing.GetInverseEasingMode(transitionParams.transition.easingMode), startTime, PlayMode.Forward);
                transitionParams.sourceWidow.Open(sourceWindowAnimation);
            }
            else
            {
                transitionParams.targetWindow.Open();

                WindowAnimation sourceWindowAnimation = new WindowAnimation(transitionParams.transition.exitAnimation.Value, transitionParams.transition.length, 
                    transitionParams.transition.easingMode, startTime, PlayMode.Reverse);
                transitionParams.sourceWidow.Close(sourceWindowAnimation);
            }
        }

        private void ExecuteOnBoth(in TransitionParams transitionParams, bool reverse, float startTime)
        {
            float targetStartTime = startTime;
            float sourceStartTime = transitionParams.transition.length - startTime;
            if (reverse)
            {
                WindowAnimation sourceWindowAnimation = new WindowAnimation(transitionParams.transition.exitAnimation.Value, transitionParams.transition.length, 
                    transitionParams.transition.easingMode, sourceStartTime, PlayMode.Forward);
                transitionParams.sourceWidow.Open(sourceWindowAnimation);

                WindowAnimation targetWindowAnimation = new WindowAnimation(transitionParams.transition.entryAnimation.Value, transitionParams.transition.length, 
                    Easing.GetInverseEasingMode(transitionParams.transition.easingMode), targetStartTime, PlayMode.Reverse);
                transitionParams.targetWindow.Close(targetWindowAnimation);
            }
            else
            {
                WindowAnimation sourceWindowAnimation = new WindowAnimation(transitionParams.transition.exitAnimation.Value, transitionParams.transition.length, 
                    Easing.GetInverseEasingMode(transitionParams.transition.easingMode), sourceStartTime, PlayMode.Reverse);
                transitionParams.sourceWidow.Close(sourceWindowAnimation);

                WindowAnimation targetWindowAnimation = new WindowAnimation(transitionParams.transition.entryAnimation.Value, transitionParams.transition.length, 
                    transitionParams.transition.easingMode, targetStartTime, PlayMode.Forward);
                transitionParams.targetWindow.Open(targetWindowAnimation);
            }
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
            // Removed as this is now expected to be handled by the screen / window itself.
            //_activeTransitionParams.sourceWindow.isInteractable = true;
            //_activeTransitionParams.targetWindow.isInteractable = true;

            if (_activeTransitionParams.transition.animationTargets != WindowTransition.AnimationTargets.Both)
            {
                if (_isActiveReverse)
                {
                    if (_activeTransitionParams.transition.animationTargets == WindowTransition.AnimationTargets.Source)
                    {
                        _activeTransitionParams.targetWindow.Close();
                    }
                }
                else
                {
                    if (_activeTransitionParams.transition.animationTargets == WindowTransition.AnimationTargets.Target)
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

        private void OnAnimationComplete(IWindowAnimator animator)
        {
            if (animator == _sourceWindowAnimator)
            {
                _sourceWindowAnimator.onComplete -= OnAnimationComplete;
                _sourceWindowAnimator = null;
            }

            if (animator == _targetWindowAnimator)
            {
                _targetWindowAnimator.onComplete -= OnAnimationComplete;
                _targetWindowAnimator = null;
            }

            if (_sourceWindowAnimator == null && _targetWindowAnimator == null)
            {
                CompleteTransition();
            }            
        }

        private bool AreOpposingTransitions(in TransitionParams lhsTransitionParams, bool lhsReverse, in TransitionParams rhsTransitionParams, bool rhsReverse)
        {
            return lhsTransitionParams.targetWindow == rhsTransitionParams.targetWindow && lhsTransitionParams.sourceWidow == rhsTransitionParams.sourceWidow &&
                lhsReverse != rhsReverse;
        }

        private bool IsSupportedTransition(in TransitionParams transitionParams, out TransitionParams? fallbackTransitionParams)
        {
            fallbackTransitionParams = null;
            bool isSupported = false;
            switch (transitionParams.transition.animationTargets)
            {
                case WindowTransition.AnimationTargets.None:
                    isSupported = true;
                    break;
                case WindowTransition.AnimationTargets.Source:
                    isSupported = transitionParams.sourceWidow.animator.IsSupportedType(transitionParams.transition.exitAnimation.Value);
                    if(!isSupported)
                    {
                        fallbackTransitionParams = new TransitionParams(
                            WindowTransition.Custom(transitionParams.transition.length, transitionParams.transition.easingMode, transitionParams.sourceWidow.animator.fallbackType, null),
                            transitionParams.sourceWidow, transitionParams.targetWindow);
                    }                    
                    break;
                case WindowTransition.AnimationTargets.Target:
                    isSupported = transitionParams.targetWindow.animator.IsSupportedType(transitionParams.transition.entryAnimation.Value);
                    if(!isSupported)
                    {
                        fallbackTransitionParams = new TransitionParams(
                            WindowTransition.Custom(transitionParams.transition.length, transitionParams.transition.easingMode, null, transitionParams.targetWindow.animator.fallbackType),
                            transitionParams.sourceWidow, transitionParams.targetWindow);
                    }                    
                    break;
                case WindowTransition.AnimationTargets.Both:
                    isSupported = transitionParams.sourceWidow.animator.IsSupportedType(transitionParams.transition.exitAnimation.Value) &&
                        transitionParams.targetWindow.animator.IsSupportedType(transitionParams.transition.entryAnimation.Value);
                    if (!isSupported)
                    {
                        fallbackTransitionParams = new TransitionParams(
                            WindowTransition.Custom(transitionParams.transition.length, transitionParams.transition.easingMode, 
                                transitionParams.sourceWidow.animator.fallbackType, transitionParams.targetWindow.animator.fallbackType),
                            transitionParams.sourceWidow, transitionParams.targetWindow);
                    }
                    break;
            }
            return isSupported;
        }

        public void Terminate(bool endActiveAnimations)
        {
            if (isTransitionActive)
            {
                if (_sourceWindowAnimator != null)
                {
                    _sourceWindowAnimator.onComplete -= OnAnimationComplete;
                    _sourceWindowAnimator = null;
                }

                if(_targetWindowAnimator != null)
                {
                    _targetWindowAnimator.onComplete -= OnAnimationComplete;
                    _targetWindowAnimator = null;
                }

                if (endActiveAnimations)
                {
                    switch (_activeTransitionParams.transition.animationTargets)
                    {
                        case WindowTransition.AnimationTargets.Source:
                            _activeTransitionParams.sourceWidow.animator.Complete();
                            if (_isActiveReverse)
                            {
                                _activeTransitionParams.targetWindow.Close();
                            }
                            break;
                        case WindowTransition.AnimationTargets.Target:
                            _activeTransitionParams.targetWindow.animator.Complete();
                            if(!_isActiveReverse)
                            {
                                _activeTransitionParams.sourceWidow.Close();
                            }
                            break;
                        case WindowTransition.AnimationTargets.Both:
                            _activeTransitionParams.sourceWidow.animator.Complete();
                            _activeTransitionParams.targetWindow.animator.Complete();
                            break;
                    }
                }

                isTransitionActive = false;
                _isActiveReverse = false;
                _activeTransitionParams.ReleaseReferences();
                _queuedTransitions.Clear();
            }
        }
    }
}