namespace UIFramework
{
    public abstract class WindowAccessAnimation : Animation
    {
        public AccessOperation accessOperation { get; protected set; } = AccessOperation.Open;

        protected WindowAccessAnimation(AccessOperation accessOperation, float length) : base(length) 
        {
            this.accessOperation = accessOperation;
        }
    }
}