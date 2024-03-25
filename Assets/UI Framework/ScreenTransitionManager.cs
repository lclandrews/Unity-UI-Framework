using System;
using System.Collections.Generic;

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

        private bool _isTransitionActive = false;
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

            if (!_isTransitionActive)
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
                    if(QueueContainsTransition(in transitionParams, reverse))
                    {
                        throw new InvalidOperationException("Attempting to perform a transition that is already queued.");
                    }

                    QueuedTransition leadingQueuedTransition = PeekQueuedTransition();
                    leadingTransitionParams = leadingQueuedTransition.transitionParams;
                    newTransitionOpposesLeading = AreOpposingTransitions(in transitionParams, reverse, in leadingTransitionParams, leadingQueuedTransition.reverse);
                }
                else
                {
                    if(_activeTransitionParams == transitionParams && _isActiveReverse == reverse)
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
                    if(_queuedTransitionsCount > 0)
                    {
                        // If we are attempting to invert a queued transition we simply pop it from the queue as the operation becomes redundant.
                        _ = PopQueuedTransition();
                    }
                    else
                    {
                        if(leadingTransitionParams.transition.isInterruptible)
                        {
                            InvertActiveTransition();
                        }
                        else
                        {
                            EnqueueTransition(in transitionParams, reverse);
                        }                        
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
            _isTransitionActive = true;
            _activeTransitionParams = transitionParams;
            _isActiveReverse = reverse;

            switch (_activeTransitionParams.transition.type)
            {
                default:
                case ScreenTransition.Type.None:
                case ScreenTransition.Type.Fade:
                case ScreenTransition.Type.SlideOverFromLeft:
                case ScreenTransition.Type.SlideOverFromRight:
                case ScreenTransition.Type.SlideOverFromBottom:
                case ScreenTransition.Type.SlideOverFromTop:
                    _activeTransitionParams.sourceScreen.sortOrder = 0;
                    _activeTransitionParams.targetScreen.sortOrder = 1;
                    break;
                case ScreenTransition.Type.SlideFromLeft:
                case ScreenTransition.Type.SlideFromRight:
                case ScreenTransition.Type.SlideFromBottom:
                case ScreenTransition.Type.SlideFromTop:
                    _activeTransitionParams.sourceScreen.sortOrder = 0;
                    _activeTransitionParams.targetScreen.sortOrder = 0;
                    break;                
                case ScreenTransition.Type.Dissolve:
                case ScreenTransition.Type.Flip:
                    _activeTransitionParams.sourceScreen.sortOrder = 1;
                    _activeTransitionParams.targetScreen.sortOrder = 0;
                    break;
            }

            if (_activeTransitionParams.transition.type == ScreenTransition.Type.None)
            {
                ExecuteNone(in _activeTransitionParams, reverse);
                CompleteTransition();
            }
            else
            {
                if(_activeTransitionParams.transition.animationTargets == ScreenTransition.AnimationTargets.Source)
                {
                    _primaryAnimator = _activeTransitionParams.sourceScreen.animator;
                }
                else
                {
                    _primaryAnimator = _activeTransitionParams.targetScreen.animator;
                }
                _activeTransitionParams.sourceScreen.isInteractable = false;
                _activeTransitionParams.targetScreen.isInteractable = false;
                float normalisedStartTime = _isActiveReverse ? 1.0F : 0.0F;
                ExecuteTransition(in _activeTransitionParams, _isActiveReverse, normalisedStartTime);
            }
        }

        private void InvertActiveTransition()
        {
            if (_activeTransitionParams.transition.type == ScreenTransition.Type.None)
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
                switch (transitionParams.transition.type)
            {
                case ScreenTransition.Type.Fade:
                    ExecuteFade(in transitionParams, reverse, startTime);
                    break;
                case ScreenTransition.Type.Dissolve:
                    ExecuteDissolve(in transitionParams, reverse, startTime);
                    break;
                case ScreenTransition.Type.SlideFromLeft:
                    ExecuteSlideFromLeft(in transitionParams, reverse, startTime);
                    break;
                case ScreenTransition.Type.SlideFromRight:
                    ExecuteSlideFromRight(in transitionParams, reverse, startTime);
                    break;
                case ScreenTransition.Type.SlideFromBottom:
                    ExecuteSlideFromBottom(in transitionParams, reverse, startTime);
                    break;
                case ScreenTransition.Type.SlideFromTop:
                    ExecuteSlideFromTop(in transitionParams, reverse, startTime);
                    break;
                case ScreenTransition.Type.SlideOverFromLeft:
                    ExecuteSlideOverFromLeft(in transitionParams, reverse, startTime);
                    break;
                case ScreenTransition.Type.SlideOverFromRight:
                    ExecuteSlideOverFromRight(in transitionParams, reverse, startTime);
                    break;
                case ScreenTransition.Type.SlideOverFromBottom:
                    ExecuteSlideOverFromBottom(in transitionParams, reverse, startTime);
                    break;
                case ScreenTransition.Type.SlideOverFromTop:
                    ExecuteSlideOverFromTop(in transitionParams, reverse, startTime);
                    break;
                case ScreenTransition.Type.Flip:
                    ExecuteFlip(in transitionParams, reverse, startTime);
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

        private void ExecuteFade(in ScreenTransitionParams transitionParams, bool reverse, float startTime)
        {
            ExecuteOnTarget(in transitionParams, reverse, startTime, WindowAnimation.Type.Fade);
        }

        private void ExecuteDissolve(in ScreenTransitionParams transitionParams, bool reverse, float startTime)
        {
            ExecuteOnSource(in transitionParams, reverse, startTime, WindowAnimation.Type.Dissolve);
        }

        private void ExecuteSlideFromLeft(in ScreenTransitionParams transitionParams, bool reverse, float startTime)
        {
            ExecuteOnBoth(in transitionParams, reverse, startTime, WindowAnimation.Type.SlideFromLeft, WindowAnimation.Type.SlideFromRight);
        }

        private void ExecuteSlideFromRight(in ScreenTransitionParams transitionParams, bool reverse, float startTime)
        {
            ExecuteOnBoth(in transitionParams, reverse, startTime, WindowAnimation.Type.SlideFromRight, WindowAnimation.Type.SlideFromLeft);
        }

        private void ExecuteSlideFromBottom(in ScreenTransitionParams transitionParams, bool reverse, float startTime)
        {
            ExecuteOnBoth(in transitionParams, reverse, startTime, WindowAnimation.Type.SlideFromBottom, WindowAnimation.Type.SlideFromTop);
        }

        private void ExecuteSlideFromTop(in ScreenTransitionParams transitionParams, bool reverse, float startTime)
        {
            ExecuteOnBoth(in transitionParams, reverse, startTime, WindowAnimation.Type.SlideFromTop, WindowAnimation.Type.SlideFromBottom);
        }

        private void ExecuteSlideOverFromLeft(in ScreenTransitionParams transitionParams, bool reverse, float startTime)
        {
            ExecuteOnTarget(in transitionParams, reverse, startTime, WindowAnimation.Type.SlideFromLeft);
        }

        private void ExecuteSlideOverFromRight(in ScreenTransitionParams transitionParams, bool reverse, float startTime)
        {
            ExecuteOnTarget(in transitionParams, reverse, startTime, WindowAnimation.Type.SlideFromRight);
        }

        private void ExecuteSlideOverFromBottom(in ScreenTransitionParams transitionParams, bool reverse, float startTime)
        {
            ExecuteOnTarget(in transitionParams, reverse, startTime, WindowAnimation.Type.SlideFromBottom);
        }

        private void ExecuteSlideOverFromTop(in ScreenTransitionParams transitionParams, bool reverse, float startTime)
        {
            ExecuteOnTarget(in transitionParams, reverse, startTime, WindowAnimation.Type.SlideFromTop);
        }

        private void ExecuteFlip(in ScreenTransitionParams transitionParams, bool reverse, float startTime)
        {
            ExecuteOnSource(in transitionParams, reverse, startTime, WindowAnimation.Type.Flip);
        }        

        private void ExecuteOnTarget(in ScreenTransitionParams transitionParams, bool reverse, float startTime, WindowAnimation.Type animationType)
        {            
            if (reverse)
            {
                transitionParams.sourceScreen.Open();

                WindowAnimation targetScreenAnimation =
                    new WindowAnimation(animationType, transitionParams.transition.length, transitionParams.transition.easingMode, startTime, PlayMode.Reverse);
                transitionParams.targetScreen.Close(targetScreenAnimation);
            }
            else
            {
                WindowAnimation targetScreenAnimation =
                    new WindowAnimation(animationType, transitionParams.transition.length, transitionParams.transition.easingMode, startTime, PlayMode.Forward);
                transitionParams.targetScreen.Open(targetScreenAnimation);
            }
        }

        private void ExecuteOnSource(in ScreenTransitionParams transitionParams, bool reverse, float startTime, WindowAnimation.Type animationType)
        {            
            if (reverse)
            {
                WindowAnimation sourceScreenAnimation =
                    new WindowAnimation(animationType, transitionParams.transition.length, transitionParams.transition.easingMode, startTime, PlayMode.Reverse);
                transitionParams.sourceScreen.Open(sourceScreenAnimation);
            }
            else
            {
                transitionParams.targetScreen.Open();

                WindowAnimation sourceScreenAnimation =
                    new WindowAnimation(animationType, transitionParams.transition.length, transitionParams.transition.easingMode, startTime, PlayMode.Forward);
                transitionParams.sourceScreen.Close(sourceScreenAnimation);
            }
        }

        private void ExecuteOnBoth(in ScreenTransitionParams transitionParams, bool reverse, float startTime, WindowAnimation.Type targetAnimationType, WindowAnimation.Type sourceAnimationType)
        {
            float targetStartTime = startTime;
            float sourceStartTime = transitionParams.transition.length - startTime;
            if(reverse)
            {
                WindowAnimation sourceScreenAnimation =
                    new WindowAnimation(sourceAnimationType, transitionParams.transition.length, transitionParams.transition.easingMode, sourceStartTime, PlayMode.Forward);
                transitionParams.sourceScreen.Open(sourceScreenAnimation);

                WindowAnimation targetScreenAnimation =
                    new WindowAnimation(targetAnimationType, transitionParams.transition.length, transitionParams.transition.easingMode, targetStartTime, PlayMode.Reverse);
                transitionParams.targetScreen.Close(targetScreenAnimation);
            }
            else
            {
                WindowAnimation sourceScreenAnimation =
                    new WindowAnimation(sourceAnimationType, transitionParams.transition.length, transitionParams.transition.easingMode, sourceStartTime, PlayMode.Reverse);
                transitionParams.sourceScreen.Close(sourceScreenAnimation);

                WindowAnimation targetScreenAnimation =
                    new WindowAnimation(targetAnimationType, transitionParams.transition.length, transitionParams.transition.easingMode, targetStartTime, PlayMode.Forward);
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
            if (_isActiveReverse)
            {
                _activeTransitionParams.sourceScreen.isInteractable = true;
                if(_activeTransitionParams.transition.animationTargets != ScreenTransition.AnimationTargets.Both)
                {
                    _activeTransitionParams.targetScreen.Close();
                }                
            }
            else
            {
                _activeTransitionParams.targetScreen.isInteractable = true;
                if (_activeTransitionParams.transition.animationTargets != ScreenTransition.AnimationTargets.Both)
                {
                    _activeTransitionParams.sourceScreen.Close();
                }
            }

            if (_queuedTransitionsCount > 0)
            {
                QueuedTransition queuedTransition = DequeueTransition();
                ExecuteTransition(in queuedTransition.GetTransitionParamsRef(), queuedTransition.reverse);
            }
            else
            {
                _isTransitionActive = false;
                _isActiveReverse = false;
                _activeTransitionParams.ReleaseReferences();
            }
        }

        private void OnAnimationComplete(IWindowAnimator animator)
        {
            _primaryAnimator.onAnimationComplete -= OnAnimationComplete;
            _primaryAnimator = null;
            CompleteTransition();
        }

        private bool AreOpposingTransitions(in ScreenTransitionParams lhsTransitionParams, bool lhsReverse, in ScreenTransitionParams rhsTransitionParams, bool rhsReverse)
        {
            return lhsTransitionParams.targetScreen == rhsTransitionParams.targetScreen && lhsTransitionParams.sourceScreen == rhsTransitionParams.sourceScreen &&
                lhsReverse != rhsReverse;
        }
    }
}