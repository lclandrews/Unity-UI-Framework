namespace UIFramework
{
    public delegate void AnimationEvent(Animation animation);

    public abstract class Animation
    {
        public float length { get; protected set; } = 0.0F;

        private Animation() { }

        protected Animation(float length)
        {
            this.length = length;
        }

        public abstract void Evaluate(float normalisedTime);

        public virtual void Prepare() { }
    }
}