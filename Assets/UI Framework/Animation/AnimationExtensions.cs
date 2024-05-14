namespace UIFramework
{
    public static class AnimationExtensions
    {
        public static PlayMode InvertPlayMode(this PlayMode playMode)
        {
            return playMode ^ (PlayMode)1;
        }

        public static AccessOperation InvertAccessOperation(this AccessOperation accessOperation)
        {
            return accessOperation ^ (AccessOperation)1;
        }
    }
}