using System;
using System.Collections.Generic;

using UnityEngine;

namespace UIFramework
{
    public class ScreenTransitionManager<ControllerType> where ControllerType : Controller<ControllerType>
    {
        // [TODO] Consider a more appropriate name for this wrapper
        private struct ScreenTransitionParams
        {
            public ScreenTransition transition { get; private set; }
            public IScreen<ControllerType> sourceScreen { get; private set; }


            public IScreen<ControllerType> targetScreen { get; private set; }

            public ScreenTransitionParams(in ScreenTransition transition, IScreen<ControllerType> sourceScreen, IScreen<ControllerType> targetScreen)
            {
                this.transition = transition;
                this.sourceScreen = sourceScreen;
                this.targetScreen = targetScreen;
            }

            public override bool Equals(object obj)
            {
                return obj is ScreenTransitionParams other && Equals(other);
            }

            public bool Equals(ScreenTransitionParams other)
            {
                return transition == other.transition && sourceScreen == other.sourceScreen && targetScreen == other.targetScreen;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(transition, sourceScreen, targetScreen);
            }

            public static bool operator ==(ScreenTransitionParams lhs, ScreenTransitionParams rhs)
            {
                return lhs.Equals(rhs);
            }

            public static bool operator !=(ScreenTransitionParams lhs, ScreenTransitionParams rhs)
            {
                return !(lhs == rhs);
            }

            public void ReleaseReferences()
            {
                sourceScreen = null;
                targetScreen = null;
            }
        }

        private class QueuedTransition
        {
            public ScreenTransitionParams transitionParams { get; }
            private ScreenTransitionParams _transitionParams = new ScreenTransitionParams();
            public bool reverse { get; private set; } = false;

            private QueuedTransition() { }

            public QueuedTransition(in ScreenTransitionParams transitionParams, bool reverse)
            {
                _transitionParams = transitionParams;
                this.reverse = reverse;
            }

            public bool ValuesAreEqualTo(QueuedTransition other)
            {
                return _transitionParams == other._transitionParams && reverse == other.reverse;
            }

            public bool ValuesAreEqualTo(in ScreenTransitionParams otherParams, bool otherReverse)
            {
                return _transitionParams == otherParams && reverse == otherReverse;
            }

            public ref ScreenTransitionParams GetTransitionParamsRef()
            {
                return ref _transitionParams;
            }
        }

        public bool isTransitionActive { get; private set; } = false;
        private ScreenTransitionParams _activeTransitionParams = new ScreenTransitionParams();
        private bool _isActiveReverse = false;

        private int _queuedTransitionsCount { get { return _queuedTransitions.Count; } }
        private List<QueuedTransition> _queuedTransitions = new List<QueuedTransition>();

        private IWindowAnimator _primaryAnimator = null;

        public void Transition(IScreen<ControllerType> sourceScreen, IScreen<ControllerType> targetScreen)
        {
            ScreenTransitionParams transitionParams = new ScreenTransitionParams(targetScreen.defaultTransition, sourceScreen, targetScreen);
            HandleNewTransition(in transitionParams, false);
        }

        public void Transition(in ScreenTransition transition, IScreen<ControllerType> sourceScreen, IScreen<ControllerType> targetScreen)
        {
            ScreenTransitionParams transitionParams = new ScreenTransitionParams(transition, sourceScreen, targetScreen);
            HandleNewTransition(in transitionParams, false);
        }

        public void ReverseTransition(IScreen<ControllerType> sourceScreen, IScreen<ControllerType> targetScreen)
        {
            ScreenTransitionParams transitionParams = new ScreenTransitionParams(targetScreen.defaultTransition, sourceScreen, targetScreen);
            HandleNewTransition(in transitionParams, true);
        }

        public void ReverseTransition(in ScreenTransition transition, IScreen<ControllerType> sourceScreen, IScreen<ControllerType> targetScreen)
        {
            ScreenTransitionParams transitionParams = new ScreenTransitionParams(transition, sourceScreen, targetScreen);
            HandleNewTransition(in transitionParams, true);
        }

        private void HandleNewTransition(in ScreenTransitionParams transitionParams, bool reverse)
        {
            if (transitionParams.sourceScreen == null)
            {
                throw new ArgumentNullException(nameof(transitionParams.sourceScreen));
            }

            if (transitionParams.targetScreen == null)
            {
                throw new ArgumentNullException(nameof(transitionParams.targetScreen));
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
                ScreenTransitionParams leadingTransitionParams;
                if (_queuedTransitionsCount > 0)
                {
                    if (QueueContainsTransition(in transitionParams, reverse))
                    {
                        throw new InvalidOperationException("Attempting to perform a transition that is already queued.");
                    }

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
                        // If newTransitionOpposesLeading == false and reverse is true we know we are unable to perform the operation while transitions
                        // are active or queued as we require the leading transition to be the forward playing equivalent of the transition to reverse.
                        throw new InvalidOperationException(
                            "Attempting to reverse a transition while the leading transition differs from the transition to reverse.");
                    }
                    else
                    {
                        // Ensure that the enqueueing transition is viable given the current leading transition.
                        if (leadingTransitionParams.targetScreen != transitionParams.sourceScreen)
                        {
                            throw new InvalidOperationException(
                                "Attempting to transition to a new screen while the leading transitions target differs from the new transitions source.");
                        }

                        EnqueueTransition(in transitionParams, reverse);
                    }
                }
            }
        }

        private void ExecuteTransition(in ScreenTransitionParams transitionParams, bool reverse)
        {
            isTransitionActive = true;
            _isActiveReverse = reverse;

            ScreenTransitionParams? fallbackTransitionParams;
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

            if(_activeTransitionParams.transition.sortPriority == ScreenTransition.SortPriority.Auto)
            {
                switch (_activeTransitionParams.transition.animationTargets)
                {
                    default:
                    case ScreenTransition.AnimationTargets.Both:
                        _activeTransitionParams.sourceScreen.sortOrder = 0;
                        _activeTransitionParams.targetScreen.sortOrder = 0;
                        break;
                    case ScreenTransition.AnimationTargets.Target:
                        _activeTransitionParams.sourceScreen.sortOrder = 0;
                        _activeTransitionParams.targetScreen.sortOrder = 1;
                        break;
                    case ScreenTransition.AnimationTargets.Source:
                        _activeTransitionParams.sourceScreen.sortOrder = 1;
                        _activeTransitionParams.targetScreen.sortOrder = 0;
                        break;
                }
            }
            else
            {
                switch (_activeTransitionParams.transition.sortPriority)
                {
                    case ScreenTransition.SortPriority.Source:
                        _activeTransitionParams.sourceScreen.sortOrder = 1;
                        _activeTransitionParams.targetScreen.sortOrder = 0;
                        break;
                    case ScreenTransition.SortPriority.Target:
                        _activeTransitionParams.sourceScreen.sortOrder = 0;
                        _activeTransitionParams.targetScreen.sortOrder = 1;
                        break;
                }
            }

            if (_activeTransitionParams.transition.animationTargets == ScreenTransition.AnimationTargets.None)
            {
                ExecuteNone(in _activeTransitionParams, reverse);
                CompleteTransition();
            }
            else
            {
                if (_activeTransitionParams.transition.animationTargets == ScreenTransition.AnimationTargets.Source)
                {
                    _primaryAnimator = _activeTransitionParams.sourceScreen.animator;
                }
                else
                {
                    _primaryAnimator = _activeTransitionParams.targetScreen.animator;
                }
                _primaryAnimator.onComplete += OnAnimationComplete;

                // Removed as this is now expected to be handled by the screen / window itself.
                //_activeTransitionParams.sourceScreen.isInteractable = false;
                //_activeTransitionParams.targetScreen.isInteractable = false;
                float normalisedStartTime = _isActiveReverse ? 1.0F : 0.0F;
                ExecuteTransition(in _activeTransitionParams, _isActiveReverse, normalisedStartTime);
            }
        }

        private void InvertActiveTransition()
        {
            if (_activeTransitionParams.transition.animationTargets == ScreenTransition.AnimationTargets.None)
            {
                throw new InvalidOperationException("Cannot invert transition with no animation targets.");
            }

            _isActiveReverse = !_isActiveReverse;

            float normalisedStartTime = _primaryAnimator.currentNormalisedTime;
            ExecuteTransition(in _activeTransitionParams, _isActiveReverse, normalisedStartTime);
        }

        private void ExecuteTransition(in ScreenTransitionParams transitionParams, bool reverse, float normalisedStartTime)
        {
            float startTime = transitionParams.transition.length * normalisedStartTime;
            switch (_activeTransitionParams.transition.animationTargets)
            {
                case ScreenTransition.AnimationTargets.Both:
                    ExecuteOnBoth(in transitionParams, reverse, startTime);
                    break;
                case ScreenTransition.AnimationTargets.Target:
                    ExecuteOnTarget(in transitionParams, reverse, startTime);
                    break;
                case ScreenTransition.AnimationTargets.Source:
                    ExecuteOnSource(in transitionParams, reverse, startTime);
                    break;
            }
        }

        private void ExecuteNone(in ScreenTransitionParams transitionParams, bool reverse)
        {
            if (reverse)
            {
                transitionParams.sourceScreen.Open();
            }
            else
            {
                transitionParams.targetScreen.Open();
            }
        }

        private void ExecuteOnTarget(in ScreenTransitionParams transitionParams, bool reverse, float startTime)
        {
            if (reverse)
            {
                transitionParams.sourceScreen.Open();

                WindowAnimation targetScreenAnimation = new WindowAnimation(transitionParams.transition.entryAnimation.Value, transitionParams.transition.length, 
                    Easing.GetInverseEasingMode(transitionParams.transition.easingMode), startTime, PlayMode.Reverse);
                transitionParams.targetScreen.Close(targetScreenAnimation);
            }
            else
            {
                WindowAnimation targetScreenAnimation = new WindowAnimation(transitionParams.transition.entryAnimation.Value, transitionParams.transition.length, 
                    transitionParams.transition.easingMode, startTime, PlayMode.Forward);
                transitionParams.targetScreen.Open(targetScreenAnimation);
            }
        }

        private void ExecuteOnSource(in ScreenTransitionParams transitionParams, bool reverse, float startTime)
        {
            startTime = transitionParams.transition.length - startTime;
            if (reverse)
            {
                WindowAnimation sourceScreenAnimation = new WindowAnimation(transitionParams.transition.exitAnimation.Value, transitionParams.transition.length, 
                    Easing.GetInverseEasingMode(transitionParams.transition.easingMode), startTime, PlayMode.Forward);
                transitionParams.sourceScreen.Open(sourceScreenAnimation);
            }
            else
            {
                transitionParams.targetScreen.Open();

                WindowAnimation sourceScreenAnimation = new WindowAnimation(transitionParams.transition.exitAnimation.Value, transitionParams.transition.length, 
                    transitionParams.transition.easingMode, startTime, PlayMode.Reverse);
                transitionParams.sourceScreen.Close(sourceScreenAnimation);
            }
        }

        private void ExecuteOnBoth(in ScreenTransitionParams transitionParams, bool reverse, float startTime)
        {
            float targetStartTime = startTime;
            float sourceStartTime = transitionParams.transition.length - startTime;
            if (reverse)
            {
                WindowAnimation sourceScreenAnimation = new WindowAnimation(transitionParams.transition.exitAnimation.Value, transitionParams.transition.length, 
                    transitionParams.transition.easingMode, sourceStartTime, PlayMode.Forward);
                transitionParams.sourceScreen.Open(sourceScreenAnimation);

                WindowAnimation targetScreenAnimation = new WindowAnimation(transitionParams.transition.entryAnimation.Value, transitionParams.transition.length, 
                    Easing.GetInverseEasingMode(transitionParams.transition.easingMode), targetStartTime, PlayMode.Reverse);
                transitionParams.targetScreen.Close(targetScreenAnimation);
            }
            else
            {
                WindowAnimation sourceScreenAnimation = new WindowAnimation(transitionParams.transition.exitAnimation.Value, transitionParams.transition.length, 
                    Easing.GetInverseEasingMode(transitionParams.transition.easingMode), sourceStartTime, PlayMode.Reverse);
                transitionParams.sourceScreen.Close(sourceScreenAnimation);

                WindowAnimation targetScreenAnimation = new WindowAnimation(transitionParams.transition.entryAnimation.Value, transitionParams.transition.length, 
                    transitionParams.transition.easingMode, targetStartTime, PlayMode.Forward);
                transitionParams.targetScreen.Open(targetScreenAnimation);
            }
        }

        private void EnqueueTransition(in ScreenTransitionParams transitionParams, bool reverse)
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

        private bool QueueContainsTransition(in ScreenTransitionParams transitionParams, bool reverse)
        {
            for (int i = _queuedTransitionsCount; i >= 0; i--)
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
            //_activeTransitionParams.sourceScreen.isInteractable = true;
            //_activeTransitionParams.targetScreen.isInteractable = true;

            if (_activeTransitionParams.transition.animationTargets != ScreenTransition.AnimationTargets.Both)
            {
                if (_isActiveReverse)
                {
                    if (_activeTransitionParams.transition.animationTargets == ScreenTransition.AnimationTargets.Source)
                    {
                        _activeTransitionParams.targetScreen.Close();
                    }
                }
                else
                {
                    if (_activeTransitionParams.transition.animationTargets == ScreenTransition.AnimationTargets.Target)
                    {
                        _activeTransitionParams.sourceScreen.Close();
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
            _primaryAnimator.onComplete -= OnAnimationComplete;
            _primaryAnimator = null;
            CompleteTransition();
        }

        private bool AreOpposingTransitions(in ScreenTransitionParams lhsTransitionParams, bool lhsReverse, in ScreenTransitionParams rhsTransitionParams, bool rhsReverse)
        {
            return lhsTransitionParams.targetScreen == rhsTransitionParams.targetScreen && lhsTransitionParams.sourceScreen == rhsTransitionParams.sourceScreen &&
                lhsReverse != rhsReverse;
        }

        private bool IsSupportedTransition(in ScreenTransitionParams transitionParams, out ScreenTransitionParams? fallbackTransitionParams)
        {
            fallbackTransitionParams = null;
            bool isSupported = false;
            switch (transitionParams.transition.animationTargets)
            {
                case ScreenTransition.AnimationTargets.None:
                    isSupported = true;
                    break;
                case ScreenTransition.AnimationTargets.Source:
                    isSupported = transitionParams.sourceScreen.animator.IsSupportedType(transitionParams.transition.exitAnimation.Value);
                    if(!isSupported)
                    {
                        fallbackTransitionParams = new ScreenTransitionParams(
                            ScreenTransition.Custom(transitionParams.transition.length, transitionParams.transition.easingMode, transitionParams.sourceScreen.animator.fallbackType, null),
                            transitionParams.sourceScreen, transitionParams.targetScreen);
                    }                    
                    break;
                case ScreenTransition.AnimationTargets.Target:
                    isSupported = transitionParams.targetScreen.animator.IsSupportedType(transitionParams.transition.entryAnimation.Value);
                    if(!isSupported)
                    {
                        fallbackTransitionParams = new ScreenTransitionParams(
                            ScreenTransition.Custom(transitionParams.transition.length, transitionParams.transition.easingMode, null, transitionParams.targetScreen.animator.fallbackType),
                            transitionParams.sourceScreen, transitionParams.targetScreen);
                    }                    
                    break;
                case ScreenTransition.AnimationTargets.Both:
                    isSupported = transitionParams.sourceScreen.animator.IsSupportedType(transitionParams.transition.exitAnimation.Value) &&
                        transitionParams.targetScreen.animator.IsSupportedType(transitionParams.transition.entryAnimation.Value);
                    if (!isSupported)
                    {
                        fallbackTransitionParams = new ScreenTransitionParams(
                            ScreenTransition.Custom(transitionParams.transition.length, transitionParams.transition.easingMode, 
                                transitionParams.sourceScreen.animator.fallbackType, transitionParams.targetScreen.animator.fallbackType),
                            transitionParams.sourceScreen, transitionParams.targetScreen);
                    }
                    break;
            }
            return isSupported;
        }

        public void Terminate(bool endActiveAnimations)
        {
            if (isTransitionActive)
            {
                if (_primaryAnimator != null)
                {
                    _primaryAnimator.onComplete -= OnAnimationComplete;
                    _primaryAnimator = null;
                }

                if (endActiveAnimations)
                {
                    switch (_activeTransitionParams.transition.animationTargets)
                    {
                        case ScreenTransition.AnimationTargets.Source:
                            _activeTransitionParams.sourceScreen.animator.Complete();
                            if (_isActiveReverse)
                            {
                                _activeTransitionParams.targetScreen.Close();
                            }
                            break;
                        case ScreenTransition.AnimationTargets.Target:
                            _activeTransitionParams.targetScreen.animator.Complete();
                            if(!_isActiveReverse)
                            {
                                _activeTransitionParams.sourceScreen.Close();
                            }
                            break;
                        case ScreenTransition.AnimationTargets.Both:
                            _activeTransitionParams.sourceScreen.animator.Complete();
                            _activeTransitionParams.targetScreen.animator.Complete();
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