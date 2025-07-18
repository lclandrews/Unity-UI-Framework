namespace UIFramework
{
    public static class AccessAnimationExtensions
    {
        public static AccessOperation InvertAccessOperation(this AccessOperation accessOperation)
        {
            return accessOperation ^ (AccessOperation)1;
        }
    }
}