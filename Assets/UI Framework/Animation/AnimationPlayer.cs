using System;

using UnityEngine;

namespace UIFramework
{
    public enum PlayMode
    {
        Forward,
        Reverse
    }

    public class AnimationPlayer
    {
        public struct PlaybackData
        {
            public float currentTime { get { return _player != null ? 0.0F : _player.currentTime; } } 
            public float currentNormalisedTime { get { return _player != null ? 0.0F : _player.currentNormalisedTime; } }
            public float length { get { return _player != null ? 0.0F : _player.length; } }
            public float remainingTime { get { return _player != null ? 0.0F : _player.remainingTime; } }
            public bool isPlaying { get { return _player != null ? false : _player.isPlaying; } }
            public bool isPaused { get { return _player != null ? false : _player.isPaused; } }

            private AnimationPlayer _player;

            public PlaybackData(AnimationPlayer player)
            {
                _player = player;
            }

            public void ReleaseReferences()
            {
                _player = null;
            }
        }

        public Animation animation { get; private set; } = null;
        public PlayMode playMode { get; private set; } = PlayMode.Forward;
        public EasingMode easingMode { get; private set; } = EasingMode.Linear;
        public float playbackSpeed { get; private set; } = 1.0F;
        public bool unscaledTime { get; private set; } = false;
        public float currentTime { get; private set; } = 0.0F;
        public float currentNormalisedTime { get { return currentTime / length; } }

        public float length
        {
            get
            {
                return animation != null ? animation.length : 0.0F;
            }
        }

        public float remainingTime
        {
            get
            {
                switch (playMode)
                {
                    case PlayMode.Forward:
                        return length - currentTime;
                    case PlayMode.Reverse:
                        return currentTime;
                }
                return 0.0F;
            }
        }

        public bool isPlaying { get; private set; } = false;
        public bool isPaused { get; private set; } = false;

        public PlaybackData playbackData { get; private set; } = default;

        public AnimationEvent onComplete { get; set; } = default;

        private int _lastUpdateFrame = 0;

        private AnimationPlayer() { }

        public AnimationPlayer(Animation animation)
        {
            if(animation == null)
            {
                throw new ArgumentNullException(nameof(animation));
            }

            this.animation = animation;
            playbackData = new PlaybackData(this);
        }

        public void Play(float startTime = 0.0F, PlayMode playMode = PlayMode.Forward, EasingMode easingMode = EasingMode.Linear, bool unscaledTime = false, 
            float playbackSpeed = 1.0F)
        {   
            if(!isPlaying)
            {
                isPlaying = true;
                AnimationSystemRunner.AddPlayer(this);
            }

            this.unscaledTime = unscaledTime;
            this.easingMode = easingMode;
            this.playMode = playMode;
            currentTime = Mathf.Clamp(startTime, 0.0F, length);

            EvaluateAnimation();
        }

        public bool Rewind()
        {
            if (isPlaying)
            {
                playMode ^= (PlayMode)1;
                return true;
            }
            return false;
        }

        public bool Stop()
        {
            if (isPlaying)
            {
                isPlaying = false;
                isPaused = false;
                return true;
            }
            return false;
        }

        public bool Pause()
        {
            if(isPlaying && !isPaused)
            {
                isPaused = true;
                return true;
            }
            return false;
        }

        public bool Resume()
        {
            if(isPlaying && isPaused)
            {
                isPaused = false;
                return true;
            }
            return false;
        }

        public bool SetCurrentTime(float time)
        {
            if (isPlaying)
            {
                currentTime = Mathf.Clamp(time, 0.0F, length);
                float endTime = playMode == PlayMode.Forward ? length : 0.0F;
                bool isAtEnd = Mathf.Approximately(currentTime, endTime);

                EvaluateAnimation();

                if (isAtEnd)
                {
                    isPlaying = false;
                    onComplete.Invoke(animation);
                }
                return true;
            }
            return false;
        }

        public bool Complete()
        {
            if (isPlaying)
            {
                currentTime = playMode == PlayMode.Forward ? length : 0.0F;

                EvaluateAnimation();

                isPlaying = false;
                onComplete.Invoke(animation);
                return true;
            }
            return false;
        }

        public void Update()
        {
            if (isPlaying && _lastUpdateFrame != Time.frameCount)
            {
                float deltaTime = unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                deltaTime *= playbackSpeed;

                bool isAtEnd = false;
                if (!isPaused)
                {
                    float animationDelta = playMode == PlayMode.Forward ? deltaTime : -deltaTime;
                    currentTime = Mathf.Clamp(currentTime + animationDelta, 0.0F, length);

                    float endTime = playMode == PlayMode.Forward ? length : 0.0F;
                    isAtEnd = Mathf.Approximately(currentTime, endTime);
                    // Double set to account for tolerance
                    currentTime = isAtEnd ? endTime : currentTime;
                }

                EvaluateAnimation();

                if (isAtEnd)
                {
                    isPlaying = false;
                    onComplete.Invoke(animation);
                }
            }                        
        }

        private void EvaluateAnimation()
        {
            _lastUpdateFrame = Time.frameCount;
            animation.Evaluate(Easing.PerformEase(currentNormalisedTime, easingMode));
        }
    }
}
