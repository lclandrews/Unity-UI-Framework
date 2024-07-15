namespace UIFramework
{
    public delegate void AnimationEvent(Animation animation);

    public abstract class Animation
    {
        public float Length { get; protected set; } = 0.0F;

        private Animation() { }

        protected Animation(float length)
        {
            this.Length = length;
        }

        public abstract void Evaluate(float normalisedTime);

        public virtual void Prepare() { }
    }
}